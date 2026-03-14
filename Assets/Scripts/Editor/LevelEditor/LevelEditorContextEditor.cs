using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(LevelEditorContext))]
public class LevelEditorContextEditor : Editor
{
    private int _newLevelWidth = 6;
    private int _newLevelHeight = 8;

    private LevelEditorContext _ctx;
    private int _selectedBrushIndex = -1;
    private bool _isPainting = false;

    private List<string> _dynamicBrushNames = new();
    private List<IEditorBrush> _dynamicBrushes = new();

    private SerializedObject _soLevel;
    private string _targetLevelName = "";

    private void OnEnable()
    {
        _ctx = (LevelEditorContext)target;
        if (_ctx.currentLevel != null)
        {
            _soLevel = new SerializedObject(_ctx.currentLevel);
            _targetLevelName = _ctx.currentLevel.name;
        }
        SetupDynamicBrushes();
    }

    private void SetupDynamicBrushes()
    {
        _dynamicBrushNames.Clear();
        _dynamicBrushes.Clear();

        if (_ctx.colorCatalog != null)
        {
            foreach (var pair in _ctx.colorCatalog.catalog)
            {
                _dynamicBrushNames.Add($"{pair.color} Pass");
                _dynamicBrushes.Add(new PassengerBrush(pair.color));
            }
        }

        _dynamicBrushNames.Add("Obstacle");
        _dynamicBrushes.Add(new ObstacleBrush());

        _dynamicBrushNames.Add("Empty Floor");
        _dynamicBrushes.Add(new FloorBrush());

        _dynamicBrushNames.Add("Object Eraser");
        _dynamicBrushes.Add(new ObjectEraserBrush());

        _dynamicBrushNames.Add("Grid Eraser");
        _dynamicBrushes.Add(new GridEraserBrush());
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Core References", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("gridManager"));

        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("passengerPrefab"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("colorCatalog"));
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            SetupDynamicBrushes();
        }

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Level Setup", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        LevelDataSO assignedLevel = (LevelDataSO)EditorGUILayout.ObjectField("Level Data", _ctx.currentLevel, typeof(LevelDataSO), false);
        if (EditorGUI.EndChangeCheck())
        {
            _ctx.currentLevel = assignedLevel;
            if (assignedLevel != null)
            {
                _soLevel = new SerializedObject(assignedLevel);
                _targetLevelName = assignedLevel.name;
            }
            serializedObject.ApplyModifiedProperties();
            return;
        }

        if (_ctx.currentLevel == null)
        {
            EditorGUILayout.HelpBox("Assign or create a Level Data SO to start.", MessageType.Warning);

            _newLevelWidth = EditorGUILayout.IntSlider("Grid Width (X)", _newLevelWidth, 1, 15);
            _newLevelHeight = EditorGUILayout.IntSlider("Grid Height (Y)", _newLevelHeight, 1, 15);

            if (GUILayout.Button("Create New Level SO", GUILayout.Height(30)))
            {
                CreateNewLevel();
            }

            serializedObject.ApplyModifiedProperties();
            return;
        }

        _targetLevelName = EditorGUILayout.TextField("Level Name", _targetLevelName);

        EditorGUI.BeginChangeCheck();

        int newIndex = EditorGUILayout.IntField("Level Index", _ctx.currentLevel.levelIndex);

        int newWidth = EditorGUILayout.IntSlider("Grid Width (X)", _ctx.currentLevel.width, 1, 15);
        int newHeight = EditorGUILayout.IntSlider("Grid Height (Y)", _ctx.currentLevel.height, 1, 15);
        float newDuration = EditorGUILayout.FloatField("Level Timer", _ctx.currentLevel.levelDuration);

        if (EditorGUI.EndChangeCheck())
        {
            bool sizeChanged = (newWidth != _ctx.currentLevel.width || newHeight != _ctx.currentLevel.height);

            _ctx.currentLevel.levelIndex = newIndex;
            _ctx.currentLevel.width = newWidth;
            _ctx.currentLevel.height = newHeight;
            _ctx.currentLevel.levelDuration = newDuration;

            if (sizeChanged)
            {
                ResizeCurrentLevel(_ctx.currentLevel);
                _ctx.RefreshEditorGrid();
            }

            EditorUtility.SetDirty(_ctx.currentLevel);
        }

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Tools & Brushes", EditorStyles.boldLabel);

        if (_dynamicBrushNames.Count > 0)
        {
            int newBrush = GUILayout.SelectionGrid(_selectedBrushIndex, _dynamicBrushNames.ToArray(), 3, GUILayout.Height(60));
            if (newBrush != _selectedBrushIndex)
            {
                _selectedBrushIndex = newBrush;
                _ctx.SetBrush(_dynamicBrushes[_selectedBrushIndex]);
            }
        }

        EditorGUILayout.Space(15);
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Load / Refresh Grid", GUILayout.Height(40)))
        {
            _ctx.RefreshEditorGrid();
        }
        GUI.backgroundColor = Color.white;

        bool isLevelValid = DrawLevelMechanicsAndValidation();

        EditorGUILayout.Space(20);

        GUI.backgroundColor = isLevelValid ? Color.green : Color.grey;
        EditorGUI.BeginDisabledGroup(!isLevelValid);
        if (GUILayout.Button("PUBLISH / SAVE LEVEL", GUILayout.Height(50)))
        {
            SaveLevel();
        }
        EditorGUI.EndDisabledGroup();
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(10);

        GUI.backgroundColor = new Color(1f, 0.7f, 0.2f);
        if (GUILayout.Button("CLOSE & CREATE NEW", GUILayout.Height(35)))
        {
            _ctx.currentLevel = null;
            _soLevel = null;

            if (_ctx.gridManager != null) _ctx.gridManager.ClearGrid();

            GameObject pObj = GameObject.Find("EditorPassengers");
            if (pObj != null) DestroyImmediate(pObj);

            serializedObject.ApplyModifiedProperties();

            GUIUtility.ExitGUI();
        }
        GUI.backgroundColor = Color.white;

        serializedObject.ApplyModifiedProperties();
    }

    private void SaveLevel()
    {
        if (_ctx.currentLevel == null) return;

        EditorUtility.SetDirty(_ctx.currentLevel);
        AssetDatabase.SaveAssets();

        if (!System.IO.Directory.Exists("Assets/Resources/Levels"))
            System.IO.Directory.CreateDirectory("Assets/Resources/Levels");

        string oldPath = AssetDatabase.GetAssetPath(_ctx.currentLevel);
        string newName = string.IsNullOrWhiteSpace(_targetLevelName) ? _ctx.currentLevel.name : _targetLevelName;
        string newPath = $"Assets/Resources/Levels/{newName}.asset";

        if (oldPath != newPath)
        {
            newPath = AssetDatabase.GenerateUniqueAssetPath(newPath);
            string moveError = AssetDatabase.MoveAsset(oldPath, newPath);

            if (!string.IsNullOrEmpty(moveError))
            {
                Debug.LogWarning($"[LevelEditor] Failed to move/rename asset: {moveError}");
            }
            else
            {
                Debug.Log($"<color=green>[LevelEditor] Level published successfully to: {newPath}</color>");
            }
        }
        else
        {
            Debug.Log($"<color=green>[LevelEditor] Level saved successfully!</color>");
        }

        AssetDatabase.Refresh();
    }

    private bool DrawLevelMechanicsAndValidation()
    {
        if (_soLevel == null) return false;

        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Level Mechanics & Validation", EditorStyles.boldLabel);

        _soLevel.Update();

        SerializedProperty busSeqProp = _soLevel.FindProperty("busSpawnSequence");
        EditorGUILayout.PropertyField(busSeqProp, true);

        if (_soLevel.ApplyModifiedProperties())
        {
            EditorUtility.SetDirty(_ctx.currentLevel);
        }

        GUILayout.Space(10);
        bool isValid = ValidateLevel(_ctx.currentLevel);

        GUILayout.Space(20);
        GUILayout.Label("Grid Visualizer", EditorStyles.boldLabel);
        DrawGridVisualizer(_ctx.currentLevel);

        return isValid;
    }

    private void DrawGridVisualizer(LevelDataSO data)
    {
        if (data.cellDataList == null || data.cellDataList.Count != data.width * data.height) return;

        for (int y = 0; y < data.height; y++)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            for (int x = 0; x < data.width; x++)
            {
                int index = y * data.width + x;
                GridCellData cell = data.cellDataList[index];

                Color bgColor = Color.gray;
                string cellText = "";

                if (cell.IsObstacle)
                {
                    bgColor = Color.black;
                    cellText = "X";
                }
                else if (cell.PassengerColor != GameColors.None)
                {
                    bgColor = GetGUIColor(cell.PassengerColor);
                    cellText = "P";
                }

                GUI.backgroundColor = bgColor;
                GUILayout.Box(cellText, GUILayout.Width(30), GUILayout.Height(30));
                GUI.backgroundColor = Color.white;
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
    }

    private bool ValidateLevel(LevelDataSO data)
    {
        Dictionary<GameColors, int> passengerCounts = new();
        foreach (var cell in data.cellDataList)
        {
            if (cell.PassengerColor != GameColors.None)
            {
                if (!passengerCounts.ContainsKey(cell.PassengerColor)) passengerCounts[cell.PassengerColor] = 0;
                passengerCounts[cell.PassengerColor]++;
            }
        }

        Dictionary<GameColors, int> busCounts = new();
        foreach (var color in data.busSpawnSequence)
        {
            if (!busCounts.ContainsKey(color)) busCounts[color] = 0;
            busCounts[color]++;
        }

        bool isImpossible = false;
        string errorMessage = "It's impossible to pass the level, please fix it:\n";

        foreach (var kvp in passengerCounts)
        {
            if (kvp.Value % 3 != 0)
            {
                isImpossible = true;
                errorMessage += $"- {kvp.Key} passengers are not a multiple of 3.\n";
            }

            int requiredBuses = Mathf.CeilToInt(kvp.Value / 3f);
            int currentBuses = busCounts.ContainsKey(kvp.Key) ? busCounts[kvp.Key] : 0;

            if (requiredBuses != currentBuses)
            {
                isImpossible = true;
                errorMessage += $"- {kvp.Key} bus count mismatch. Req: {requiredBuses}, Added: {currentBuses}\n";
            }
        }

        foreach (var kvp in busCounts)
        {
            if (!passengerCounts.ContainsKey(kvp.Key))
            {
                isImpossible = true;
                errorMessage += $"- No {kvp.Key} passengers, but {kvp.Key} bus added.\n";
            }
        }

        if (isImpossible)
        {
            EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
            return false;
        }
        else if (passengerCounts.Count > 0)
        {
            EditorGUILayout.HelpBox("Level valid! Passenger and bus matches are correct.", MessageType.Info);
            return true;
        }

        EditorGUILayout.HelpBox("Level is empty. Add passengers to validate.", MessageType.Warning);
        return false;
    }

    private Color GetGUIColor(GameColors color)
    {
        return color switch
        {
            GameColors.Red => Color.red,
            GameColors.Blue => Color.blue,
            GameColors.Green => Color.green,
            GameColors.Yellow => Color.yellow,
            GameColors.Purple => Color.magenta,
            GameColors.Orange => new Color(1f, 0.5f, 0f),
            _ => Color.white
        };
    }

    private void OnSceneGUI()
    {
        if (_ctx.currentLevel == null || _selectedBrushIndex == -1) return;

        Event e = Event.current;
        int controlID = GUIUtility.GetControlID(FocusType.Passive);

        if (e.type == EventType.Layout) HandleUtility.AddDefaultControl(controlID);

        if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0)
        {
            _isPainting = true;
            PaintAtMousePosition(e.mousePosition);
            e.Use();
        }
        else if (e.type == EventType.MouseUp && e.button == 0)
        {
            _isPainting = false;
        }
    }

    private void PaintAtMousePosition(Vector2 mousePos)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Tile clickedTile = hit.collider.GetComponentInParent<Tile>();
            if (clickedTile != null)
            {
                _ctx.PaintCell(clickedTile.Coordinates);
                SceneView.RepaintAll();
            }
        }
    }

    private void CreateNewLevel()
    {
        if (_ctx.gridManager == null) _ctx.gridManager = _ctx.GetComponent<GridManager>();
        if (_ctx.gridManager == null) return;

        LevelDataSO newLevel = ScriptableObject.CreateInstance<LevelDataSO>();
        newLevel.width = _newLevelWidth;
        newLevel.height = _newLevelHeight;

        for (int i = 0; i < newLevel.width * newLevel.height; i++)
        {
            newLevel.cellDataList.Add(new GridCellData()
            {
                Coordinates = new Vector2Int(i % newLevel.width, i / newLevel.width),
                IsActive = true
            });
        }

        if (!System.IO.Directory.Exists("Assets/Resources/TestLevels"))
            System.IO.Directory.CreateDirectory("Assets/Resources/TestLevels");

        string path = AssetDatabase.GenerateUniqueAssetPath("Assets/Resources/TestLevels/TestLevel.asset");
        AssetDatabase.CreateAsset(newLevel, path);
        AssetDatabase.SaveAssets();

        _ctx.currentLevel = newLevel;
        _soLevel = new SerializedObject(newLevel);
        _targetLevelName = newLevel.name;
        _ctx.RefreshEditorGrid();
    }

    private void ResizeCurrentLevel(LevelDataSO data)
    {
        int totalCells = data.width * data.height;
        if (data.cellDataList.Count == totalCells) return;

        List<GridCellData> newList = new List<GridCellData>(totalCells);

        for (int y = 0; y < data.height; y++)
        {
            for (int x = 0; x < data.width; x++)
            {
                GridCellData existingCell = data.cellDataList.Find(c => c.Coordinates.x == x && c.Coordinates.y == y);

                if (existingCell != null)
                {
                    newList.Add(existingCell);
                }
                else
                {
                    newList.Add(new GridCellData { Coordinates = new Vector2Int(x, y), IsActive = true });
                }
            }
        }

        data.cellDataList = newList;
    }
}