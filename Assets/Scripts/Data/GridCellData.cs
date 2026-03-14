using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class GridCellData
{
    [HideInInspector] public Vector2Int Coordinates;

    public bool IsObstacle;
    public bool IsActive = true;
    public GameColors PassengerColor;
    public bool IsSecret;
    public bool IsReserved;

    public bool HasPassenger => PassengerColor != GameColors.None;

    public GridCellData()
    {
        PassengerColor = GameColors.None;
    }
}