#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridManager))]
public class GridManagerEditor : Editor
{
    private GridManager _grid;

    private void OnEnable() => _grid = (GridManager)target;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(15);
        EditorGUILayout.LabelField("Level Design Tools", EditorStyles.boldLabel);

        GUI.enabled = !Application.isPlaying;

        using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
        {
            GUI.backgroundColor = Color.cyan;
            if (GUILayout.Button(new GUIContent("1. Build Grid", "Generates the grid structure based on width/height"), GUILayout.Height(35)))
            {
                Undo.RecordObject(_grid.environmentParent, "Build Grid");
                _grid.GenerateGridInEditor();
                EditorUtility.SetDirty(_grid);
            }

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button(new GUIContent("2. Clear Grid", "Removes all children from the environment parent"), GUILayout.Height(35)))
            {
                Undo.RecordObject(_grid.environmentParent, "Clear Grid");
                _grid.ClearGrid();
                EditorUtility.SetDirty(_grid);
            }

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button(new GUIContent("3. Save Level", "Creates a ScriptableObject from current layout"), GUILayout.Height(35)))
            {
                _grid.SaveLevelData();
            }
        }

        GUI.enabled = true;
        GUI.backgroundColor = Color.white;
    }
}
#endif