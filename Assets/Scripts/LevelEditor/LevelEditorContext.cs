using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class LevelEditorContext : MonoBehaviour
{
    public LevelDataSO currentLevel;
    public GridManager gridManager;

    [Header("Editor Spawning")]
    public GameObject passengerPrefab;
    public ColorCatalogSO colorCatalog;

    private IEditorBrush _activeBrush;
    private Dictionary<Vector2Int, GameObject> _editorPassengers = new();

    private Transform _passengersParent;

    public void SetBrush(IEditorBrush brush)
    {
        _activeBrush = brush;
    }

    public void PaintCell(Vector2Int coords)
    {
        if (currentLevel == null || _activeBrush == null) return;

        GridCellData cell = currentLevel.GetCellData(coords.x, coords.y);
        Tile tile = gridManager.GetTile(coords);

        if (cell != null && tile != null)
        {
            _activeBrush.Apply(cell, tile);
            UpdateTileVisual(tile, cell);

#if UNITY_EDITOR
            EditorUtility.SetDirty(currentLevel);
#endif
        }
    }

    public void RefreshEditorGrid()
    {
        if (currentLevel == null || gridManager == null) return;

#if UNITY_EDITOR
        gridManager.LoadLevelInEditor(currentLevel);
        ClearEditorPassengers();

        GameObject pObj = new GameObject("EditorPassengers");
        pObj.transform.SetParent(gridManager.environmentParent);
        _passengersParent = pObj.transform;

        foreach (var cell in currentLevel.cellDataList)
        {
            Tile tile = gridManager.GetTile(cell.Coordinates);
            if (tile != null) UpdateTileVisual(tile, cell);
        }
#endif
    }

    private void ClearEditorPassengers()
    {
        foreach (var p in _editorPassengers.Values)
        {
            if (p != null) DestroyImmediate(p);
        }
        _editorPassengers.Clear();

        if (_passengersParent != null)
        {
            DestroyImmediate(_passengersParent.gameObject);
        }
    }

    private void UpdateTileVisual(Tile tile, GridCellData data)
    {
        tile.Setup(data.Coordinates, data.IsObstacle);

        Renderer tileRen = tile.GetComponentInChildren<Renderer>();
        if (tileRen != null)
        {
            tileRen.enabled = data.IsActive;

            if (data.IsActive)
            {
                MaterialPropertyBlock mpb = new MaterialPropertyBlock();
                tileRen.GetPropertyBlock(mpb);
                Color tileColor = data.IsObstacle ? Color.black : Color.white;
                mpb.SetColor("_Color", tileColor);
                mpb.SetColor("_BaseColor", tileColor);
                tileRen.SetPropertyBlock(mpb);
            }
        }

#if UNITY_EDITOR
        if (data.IsActive && data.PassengerColor != GameColors.None && !data.IsObstacle)
        {
            if (!_editorPassengers.TryGetValue(data.Coordinates, out GameObject passObj) || passObj == null)
            {
                if (_passengersParent == null)
                {
                    _passengersParent = new GameObject("EditorPassengers").transform;
                    _passengersParent.SetParent(gridManager.environmentParent);
                }

                passObj = (GameObject)PrefabUtility.InstantiatePrefab(passengerPrefab, _passengersParent);
                passObj.transform.position = new Vector3(tile.transform.position.x, 0f, tile.transform.position.z);

                foreach (var col in passObj.GetComponentsInChildren<Collider>(true)) col.enabled = false;

                _editorPassengers[data.Coordinates] = passObj;
            }

            if (colorCatalog != null)
            {
                Material mat = colorCatalog.GetPassengerMaterial(data.PassengerColor);
                if (mat != null)
                {
                    SkinnedMeshRenderer[] smrs = passObj.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                    foreach (var smr in smrs)
                    {
                        if (smr.gameObject.name == "Character_Main")
                        {
                            smr.sharedMaterial = mat;
                            break;
                        }
                    }
                }
            }
        }
        else
        {
            if (_editorPassengers.TryGetValue(data.Coordinates, out GameObject passObj) && passObj != null)
            {
                DestroyImmediate(passObj);
                _editorPassengers.Remove(data.Coordinates);
            }
        }
#endif
    }
}