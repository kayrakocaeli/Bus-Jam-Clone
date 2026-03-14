using System.Collections.Generic;
using UnityEngine;

public class Pathfinder
{
    private const int StraightCost = 10;

    public List<Tile> FindPath(Vector2Int startCoords, Vector2Int endCoords, Passenger movingPassenger)
    {
        var gridManager = GameContext.Get<GridManager>();
        var startTile = gridManager.GetTile(startCoords);
        var endTile = gridManager.GetTile(endCoords);

        if (startTile == null || endTile == null) return null;

        var openList = new List<Tile> { startTile };
        var closedList = new HashSet<Tile>();

        foreach (var tile in gridManager.Tiles)
        {
            tile.GCost = int.MaxValue;
            tile.Parent = null;
        }

        startTile.GCost = 0;
        startTile.HCost = CalculateManhattanDistance(startTile, endTile);

        while (openList.Count > 0)
        {
            var currentTile = GetLowestFCostNode(openList);
            if (currentTile == endTile) return CalculatePath(endTile);

            openList.Remove(currentTile);
            closedList.Add(currentTile);

            foreach (var dir in GridManager.Directions)
            {
                var neighbor = gridManager.GetTile(currentTile.Coordinates + dir);
                if (neighbor == null || closedList.Contains(neighbor) || neighbor.IsObstacle) continue;

                if (neighbor.HasPassenger)
                {
                    if (neighbor.Coordinates != startCoords && neighbor.Coordinates != endCoords)
                        continue;
                }

                int tentativeGCost = currentTile.GCost + StraightCost;
                if (tentativeGCost < neighbor.GCost)
                {
                    neighbor.Parent = currentTile;
                    neighbor.GCost = tentativeGCost;
                    neighbor.HCost = CalculateManhattanDistance(neighbor, endTile);

                    if (!openList.Contains(neighbor)) openList.Add(neighbor);
                }
            }
        }
        return null;
    }

    private int CalculateManhattanDistance(Tile a, Tile b) =>
        (Mathf.Abs(a.Coordinates.x - b.Coordinates.x) + Mathf.Abs(a.Coordinates.y - b.Coordinates.y)) * StraightCost;

    private Tile GetLowestFCostNode(List<Tile> pathList)
    {
        Tile lowestNode = pathList[0];
        for (int i = 1; i < pathList.Count; i++)
        {
            if (pathList[i].FCost < lowestNode.FCost) lowestNode = pathList[i];
        }
        return lowestNode;
    }

    private List<Tile> CalculatePath(Tile endNode)
    {
        var path = new List<Tile> { endNode };
        var current = endNode;
        while (current.Parent != null)
        {
            path.Add(current.Parent);
            current = current.Parent;
        }
        path.Reverse();
        return path;
    }
}