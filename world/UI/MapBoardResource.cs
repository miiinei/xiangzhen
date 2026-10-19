using Godot;
using Godot.Collections;

[GlobalClass]
public partial class MapBoardResource : Resource
{
    [Export] public Array<Vector2I> Positions { get; set; } = new();
    [Export] public Array<int> TileStates { get; set; } = new();
}
