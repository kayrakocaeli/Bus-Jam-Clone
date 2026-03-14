using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(LevelDataSO))]
public class LevelDataSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Draw default fields (width, height, duration, lists)
        DrawDefaultInspector();

        LevelDataSO data = (LevelDataSO)target;

        if (data.cellDataList == null || data.cellDataList.Count != data.width * data.height)
        {
            EditorGUILayout.HelpBox("Grid dimensions do not match list size. Please trigger an update.", MessageType.Warning);
            return;
        }

        // Validate level mechanics
        GUILayout.Space(10);
        ValidateLevel(data);

        GUILayout.Space(20);
        GUILayout.Label("Grid Visualizer", EditorStyles.boldLabel);

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

    private void ValidateLevel(LevelDataSO data)
    {
        Dictionary<GameColors, int> passengerCounts = new();
        foreach (var cell in data.cellDataList)
        {
            if (cell.PassengerColor != GameColors.None)
            {
                if (!passengerCounts.ContainsKey(cell.PassengerColor))
                    passengerCounts[cell.PassengerColor] = 0;

                passengerCounts[cell.PassengerColor]++;
            }
        }

        Dictionary<GameColors, int> busCounts = new();
        foreach (var color in data.busSpawnSequence)
        {
            if (!busCounts.ContainsKey(color))
                busCounts[color] = 0;

            busCounts[color]++;
        }

        bool isImpossible = false;
        string errorMessage = "It's impossible to pass the level, please fix it.:\n";

        // Check passenger amounts and required buses
        foreach (var kvp in passengerCounts)
        {
            if (kvp.Value % 3 != 0)
            {
                isImpossible = true;
                errorMessage += $"- The number of passengers in {kvp.Key} is not a multiple of 3.\n";
            }

            int requiredBuses = Mathf.CeilToInt(kvp.Value / 3f);
            int currentBuses = busCounts.ContainsKey(kvp.Key) ? busCounts[kvp.Key] : 0;

            if (requiredBuses != currentBuses)
            {
                isImpossible = true;
                errorMessage += $"- {kvp.Key} bus count mismatch. Required: {requiredBuses}, Added: {currentBuses}\n";
            }
        }

        // Check for extra buses with no passengers
        foreach (var kvp in busCounts)
        {
            if (!passengerCounts.ContainsKey(kvp.Key))
            {
                isImpossible = true;
                errorMessage += $"- There are no {kvp.Key} passengers on stage, but the {kvp.Key} bus has been added.\n";
            }
        }

        if (isImpossible)
        {
            EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
        }
        else if (passengerCounts.Count > 0)
        {
            EditorGUILayout.HelpBox("Level valid! Passenger and bus matches are correct.", MessageType.Info);
        }
    }

    // Map your GameColors enum to Unity GUI Colors
    private Color GetGUIColor(GameColors color)
    {
        switch (color)
        {
            case GameColors.Red: return Color.red;
            case GameColors.Blue: return Color.blue;
            case GameColors.Green: return Color.green;
            case GameColors.Yellow: return Color.yellow;
            case GameColors.Purple: return Color.magenta;
            case GameColors.Orange: return new Color(1f, 0.5f, 0f);
            default: return Color.white;
        }
    }
}