using Godot;

public static class HexGeometry
{
    public const int EdgeCount = 6;
    public const int OddRowMask = 1;
    public const int OffsetParityDivisor = 2;
    public const float FirstVertexAngleDegrees = 30f;
    public const float AngleStepDegrees = 60f;
    public const float Half = 0.5f;
    public const float RowSpacing = 1.5f;
    public static readonly float SqrtThree = Mathf.Sqrt(3f);
}

public static class WorldGeometry
{
    public const float PieceElevation = 0.35f;
}

public static class PreviewGeometry
{
    public const float SurfaceElevation = 0.22f;
    public static readonly Color RangeColor = new(0.2f, 0.7f, 1f, 1f);
}

public static class ObstacleGroups
{
    public const int Ground = 0;
    public const int Mountain = 1;
    public const int FlyingUnit = 2;
}

public enum Faction
{
    Neutral,
    Player,
    Enemy
}

public static class ScenePaths
{
    public const string Tile = "res://template/tile/Tile.tscn";
    public const string DefaultSkill = "res://template/Skill/Skill.tscn";
    public const string BuildPanel = "res://world/UI/BuildPanel.tscn";
    public const string MapDirectory = "res://Map/";
    public const string CurrentMap = "res://Map/current_map.tres";
}

public static class SceneNames
{
    public static string Tile(int column, int row) => $"Tile_{column}_{row}";
    public static string Piece(Vector2I position) => $"Piece_{position.X}_{position.Y}";
    public static string SkillSlot(int index, bool empty) => empty ? $"EmptySlot{index}" : $"Skill{index}";
    public static string HealthBar(StringName owner) => $"{owner}HealthBar";
    public static string HealthSegment(int index) => $"Health_{index}";
    public static string SkillPanel(StringName owner) => $"{owner}SkillPanel";
    public static string Enemy(Vector2I position) => $"Enemy_{position.X}_{position.Y}";
    public static string EnemyActionOrder(StringName owner) => $"{owner}ActionOrder";
    public static string MapTileState(int index) => $"TileState_{index}";
}

public static class GodotThemeKeys
{
    public static readonly StringName Panel = "panel";
    public static readonly StringName Separation = "separation";
    public static readonly StringName FontColor = "font_color";
    public static readonly StringName FontSize = "font_size";
    public static readonly StringName Normal = "normal";
    public static readonly StringName Hover = "hover";
    public static readonly StringName Pressed = "pressed";
}