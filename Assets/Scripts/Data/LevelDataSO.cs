using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewLevelData", menuName = "BusJam/LevelData")]
public class LevelDataSO : ScriptableObject
{
    public int levelIndex;
    public float levelDuration;

    [Header("Grid Dimensions")]
    [Min(1)] public int width = 6;
    [Min(1)] public int height = 8;

    public List<GridCellData> cellDataList = new();
    public List<GameColors> busSpawnSequence = new();

    // Helper to access data easily from other scripts
    public GridCellData GetCellData(int x, int y)
    {
        int index = y * width + x;
        if (index >= 0 && index < cellDataList.Count)
            return cellDataList[index];
        return null;
    }

    private void OnValidate()
    {
        int totalCells = width * height;

        // Efficient list resizing
        if (cellDataList.Count != totalCells)
        {
            if (cellDataList.Count < totalCells)
            {
                while (cellDataList.Count < totalCells)
                    cellDataList.Add(new GridCellData());
            }
            else
            {
                cellDataList.RemoveRange(totalCells, cellDataList.Count - totalCells);
            }
        }

        // Auto-update coordinates but keep them hidden in inspector
        for (int i = 0; i < cellDataList.Count; i++)
        {
            cellDataList[i].Coordinates = new Vector2Int(i % width, i / width);
        }
    }
}