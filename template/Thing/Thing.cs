using Godot;

public partial class Thing : Node2D
{
    [Export]
    public int ObstacleGroup { get; set; } = 0;

    public World WorldRef { get; set; }
    public Tile OccupiedTile { get; set; }

    public virtual void SetSelected(bool selected)
    {
    }

    public virtual void ResetTurn()
    {
    }
}