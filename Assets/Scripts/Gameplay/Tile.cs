using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector2Int Coordinates;
    public bool IsObstacle;
    public bool HasPassenger;

    // Pathfinding values
    public int GCost;
    public int HCost;
    public int FCost => GCost + HCost;
    public Tile Parent;

    public void Setup(Vector2Int coords, bool isObstacle)
    {
        Coordinates = coords;
        IsObstacle = isObstacle;
    }
}