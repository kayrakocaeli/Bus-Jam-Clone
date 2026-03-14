using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GridManager : MonoBehaviour
{
    [Header("Level Creation")]
    [Range(1, 15)] public int width = 6;
    [Range(1, 15)] public int height = 8;

    [Header("Alignment")]
    public Transform queueReference;
    public float gapFromQueue = 2f;
    public bool buildDownwards = true;

    [Header("Shader Mask")]
    public Renderer environmentRenderer;
    [Range(0f, 0.5f)] public float cornerRadius = 0.1f;
    [Range(0f, 1f)] public float tileTextureScale = 1.0f;
    public float tileSize = 1f;

    public GameObject tilePrefab;
    public Transform environmentParent;
    public ColorCatalogSO colorCatalog;

    private readonly Dictionary<Vector2Int, Tile> _tiles = new();
    private readonly List<Passenger> _activePassengers = new();
    private readonly Queue<Tile> _bfsQueue = new(128);
    private readonly HashSet<Tile> _reachableTiles = new(128);

    public static readonly Vector2Int[] Directions = {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    public IReadOnlyList<Passenger> ActivePassengers => _activePassengers;
    public IEnumerable<Tile> Tiles => _tiles.Values;

    private MaterialPropertyBlock _propBlock;
    private static readonly int GridSizeId = Shader.PropertyToID("_GridSize");
    private static readonly int GridCenterId = Shader.PropertyToID("_GridCenter");
    private static readonly int CornerRadiusId = Shader.PropertyToID("_CornerRadius");
    private static readonly int TileScaleId = Shader.PropertyToID("_TileScale");

    private void Awake() => GameContext.Register(this);
    private void OnDestroy() => GameContext.Unregister<GridManager>();

    public void InitializeLevel(LevelDataSO levelData)
    {
        ClearGrid();
        _activePassengers.Clear();
        width = levelData.width;
        height = levelData.height;

        Transform tilesParent = new GameObject("Tiles").transform;
        tilesParent.SetParent(environmentParent);
        Transform passengersParent = new GameObject("Passengers").transform;
        passengersParent.SetParent(environmentParent);

        var pool = GameContext.Get<PoolManager>();

        foreach (var cell in levelData.cellDataList)
        {
            if (!cell.IsActive) continue;
            var tileObj = pool.Get("Tile");
            tileObj.transform.position = CalculateWorldPosition(cell.Coordinates.x, cell.Coordinates.y);
            tileObj.transform.SetParent(tilesParent);

            var tile = tileObj.GetComponent<Tile>();
            tile.Setup(cell.Coordinates, cell.IsObstacle);
            _tiles.Add(cell.Coordinates, tile);

            if (cell.HasPassenger)
            {
                var pObj = pool.Get("Passenger");
                pObj.transform.position = new Vector3(tileObj.transform.position.x, 0f, tileObj.transform.position.z);
                pObj.transform.SetParent(passengersParent);

                tile.HasPassenger = true;
                var passenger = pObj.GetComponent<Passenger>();
                passenger.Setup(cell.PassengerColor, cell.Coordinates, colorCatalog.GetPassengerMaterial(cell.PassengerColor));
                _activePassengers.Add(passenger);
            }
        }
        UpdateShaderParams(levelData);
        RefreshOutlines();
    }

    public void RefreshOutlines()
    {
        _reachableTiles.Clear();
        _bfsQueue.Clear();

        int exitY = buildDownwards ? 0 : height - 1;
        var bus = GameContext.Get<BusManager>();
        var queue = GameContext.Get<QueueManager>();

        for (int x = 0; x < width; x++)
        {
            if (_tiles.TryGetValue(new Vector2Int(x, exitY), out Tile t) && !t.IsObstacle && !t.HasPassenger)
            {
                _reachableTiles.Add(t);
                _bfsQueue.Enqueue(t);
            }
        }

        while (_bfsQueue.Count > 0)
        {
            Tile current = _bfsQueue.Dequeue();
            foreach (var dir in Directions)
            {
                if (_tiles.TryGetValue(current.Coordinates + dir, out Tile neighbor))
                {
                    if (!neighbor.IsObstacle && !neighbor.HasPassenger && _reachableTiles.Add(neighbor))
                        _bfsQueue.Enqueue(neighbor);
                }
            }
        }

        bool queueHasSpace = queue.HasEmptySlot();
        GameColors currentBusColor = bus.CurrentBus?.Color ?? GameColors.None;

        for (int i = _activePassengers.Count - 1; i >= 0; i--)
        {
            Passenger p = _activePassengers[i];
            if (!p.gameObject.activeInHierarchy)
            {
                _activePassengers.RemoveAt(i);
                continue;
            }

            bool canMove = IsAtExit(p.CurrentCoords) || HasReachableNeighbor(p.CurrentCoords);
            bool busMatch = p.Color == currentBusColor && bus.HasAvailableBusFor(p.Color, true);

            p.SetOutline(canMove && (busMatch || queueHasSpace));
        }
    }

    private bool IsAtExit(Vector2Int coords) => coords.y == (buildDownwards ? 0 : height - 1);

    private bool HasReachableNeighbor(Vector2Int coords)
    {
        foreach (var dir in Directions)
        {
            if (_tiles.TryGetValue(coords + dir, out Tile neighbor) && _reachableTiles.Contains(neighbor))
                return true;
        }
        return false;
    }

    public List<Tile> FindPathToExit(Vector2Int startCoords)
    {
        _bfsQueue.Clear();
        var cameFrom = new Dictionary<Vector2Int, Tile>();

        Tile startTile = GetTile(startCoords);
        _bfsQueue.Enqueue(startTile);
        cameFrom[startCoords] = null;

        int exitY = buildDownwards ? 0 : height - 1;
        Tile exitTile = null;

        while (_bfsQueue.Count > 0)
        {
            Tile current = _bfsQueue.Dequeue();
            if (current.Coordinates.y == exitY) { exitTile = current; break; }

            foreach (var dir in Directions)
            {
                Vector2Int nextPos = current.Coordinates + dir;
                if (_tiles.TryGetValue(nextPos, out Tile neighbor) &&
                    !neighbor.IsObstacle && !neighbor.HasPassenger && !cameFrom.ContainsKey(nextPos))
                {
                    cameFrom[nextPos] = current;
                    _bfsQueue.Enqueue(neighbor);
                }
            }
        }

        if (exitTile == null) return null;

        var path = new List<Tile>();
        Tile temp = exitTile;
        while (temp != null) { path.Add(temp); temp = cameFrom[temp.Coordinates]; }
        path.Reverse();
        return path;
    }

    public void UpdateShaderParams(LevelDataSO levelData)
    {
        if (environmentRenderer == null || levelData == null) return;
        _propBlock ??= new MaterialPropertyBlock();

        Vector4[] shaderPositions = new Vector4[255];
        int activeCount = 0;

        foreach (var cell in levelData.cellDataList)
        {
            if (cell.IsActive && activeCount < 255)
            {
                Vector3 wPos = CalculateWorldPosition(cell.Coordinates.x, cell.Coordinates.y);
                shaderPositions[activeCount] = new Vector4(wPos.x, wPos.z, 0, 0);
                activeCount++;
            }
        }

        _propBlock.SetInt("_ActiveTileCount", activeCount);

        _propBlock.SetVectorArray("_ActiveTiles", shaderPositions);

        _propBlock.SetFloat("_TileSize", tileSize);
        _propBlock.SetFloat("_CornerRadius", cornerRadius);

        float openSign = buildDownwards ? 1f : -1f;
        float openLimitZ = CalculateWorldPosition(0, 0).z + (tileSize * 0.5f * openSign);
        _propBlock.SetFloat("_OpenLimitZ", openLimitZ);
        _propBlock.SetFloat("_OpenSign", openSign);

        environmentRenderer.SetPropertyBlock(_propBlock);
    }


    private Vector3 CalculateWorldPosition(int x, int y)
    {
        float worldX = x - (width - 1) * 0.5f;
        float startZ = queueReference != null ? queueReference.position.z : 0f;
        float worldZ = buildDownwards ? startZ - gapFromQueue - y : startZ + gapFromQueue + y;
        return new Vector3(worldX, 0, worldZ);
    }

    public Tile GetTile(Vector2Int coords) => _tiles.TryGetValue(coords, out var tile) ? tile : null;

    public void RemovePassengerFromList(Passenger p) => _activePassengers.Remove(p);

    public void ClearGrid()
    {
        if (environmentParent == null) return;
        for (int i = environmentParent.childCount - 1; i >= 0; i--)
            DestroyImmediate(environmentParent.GetChild(i).gameObject);
        _tiles.Clear();
    }

#if UNITY_EDITOR

    public void GenerateGridInEditor()
    {
        ClearGrid();
        var tempTiles = new Dictionary<Vector2Int, Tile>();

        Transform tilesParent = new GameObject("Tiles").transform;
        tilesParent.SetParent(environmentParent);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var tileGo = (GameObject)PrefabUtility.InstantiatePrefab(tilePrefab, tilesParent);
                tileGo.transform.position = CalculateWorldPosition(x, y);

                var tile = tileGo.GetComponent<Tile>();
                tile.Coordinates = new Vector2Int(x, y);
                tempTiles.Add(tile.Coordinates, tile);
            }
        }
    }

    public void SaveLevelData()
    {
        var newLevelData = ScriptableObject.CreateInstance<LevelDataSO>();
        newLevelData.width = width;
        newLevelData.height = height;
        newLevelData.cellDataList = new List<GridCellData>();

        foreach (var tile in Tiles)
        {
            var cellData = new GridCellData
            {
                Coordinates = tile.Coordinates,
                IsObstacle = tile.IsObstacle,
                PassengerColor = GameColors.None
            };
            newLevelData.cellDataList.Add(cellData);
        }

        if (!System.IO.Directory.Exists("Assets/Resources/Levels"))
            System.IO.Directory.CreateDirectory("Assets/Resources/Levels");

        string path = "Assets/Resources/Levels/Level_Generated.asset";
        AssetDatabase.CreateAsset(newLevelData, path);
        AssetDatabase.SaveAssets();

        Debug.Log($"Level saved to: {path}");
    }

    public void LoadLevelInEditor(LevelDataSO levelData)
    {
        ClearGrid();
        width = levelData.width;
        height = levelData.height;

        Transform tilesParent = new GameObject("Tiles").transform;
        tilesParent.SetParent(environmentParent);

        foreach (var cell in levelData.cellDataList)
        {
            if (!cell.IsActive) continue;
            var tileGo = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(tilePrefab, tilesParent);
            tileGo.transform.position = CalculateWorldPosition(cell.Coordinates.x, cell.Coordinates.y);

            var tile = tileGo.GetComponent<Tile>();
            tile.Coordinates = cell.Coordinates;
            tile.Setup(cell.Coordinates, cell.IsObstacle);

            _tiles.Add(cell.Coordinates, tile);
        }

        UpdateShaderParams(levelData);
    }
#endif
}