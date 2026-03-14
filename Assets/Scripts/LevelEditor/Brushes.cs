public class PassengerBrush : IEditorBrush
{
    private readonly GameColors _color;

    public PassengerBrush(GameColors color) => _color = color;

    public void Apply(GridCellData cellData, Tile tileVisual)
    {
        cellData.IsObstacle = false;
        cellData.PassengerColor = _color;
    }
}

public class ObstacleBrush : IEditorBrush
{
    public void Apply(GridCellData cellData, Tile tileVisual)
    {
        cellData.IsObstacle = true;
        cellData.PassengerColor = GameColors.None;
    }
}

public class ObjectEraserBrush : IEditorBrush
{
    public void Apply(GridCellData cellData, Tile tileVisual)
    {
        cellData.IsObstacle = false;
        cellData.PassengerColor = GameColors.None;
    }
}

public class GridEraserBrush : IEditorBrush
{
    public void Apply(GridCellData cellData, Tile tileVisual)
    {
        cellData.IsActive = false;
        cellData.IsObstacle = false;
        cellData.PassengerColor = GameColors.None;
    }
}

public class FloorBrush : IEditorBrush
{
    public void Apply(GridCellData cellData, Tile tileVisual)
    {
        cellData.IsActive = true;
        cellData.IsObstacle = false;
        cellData.PassengerColor = GameColors.None;
    }
}