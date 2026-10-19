using Godot;

public partial class BuildPanel : Control
{
    private const float PanelHeight = 80f;
    private const float ToolbarHalfWidth = 100f;
    private const float ToolbarHalfHeight = 28f;
    private const float TileButtonSize = 56f;
    private const int ButtonSeparation = 16;
    private const int ButtonBorderWidth = 3;
    private const int SelectedButtonBorderWidth = 5;
    private const float SelectedColorMultiplier = 1.25f;
    private static readonly Color GrassButtonColor = new(0.23f, 0.43f, 0.25f, 1f);
    private static readonly Color MountainButtonColor = new(0.38f, 0.34f, 0.28f, 1f);
    private static readonly Color WaterButtonColor = new(0.12f, 0.34f, 0.48f, 1f);
    private static readonly Color ButtonBorderColor = new(0.16f, 0.18f, 0.2f, 1f);
    private static readonly Color SelectedButtonBorderColor = new(0.55f, 0.85f, 1f, 1f);
    private static readonly NodePath GrassButtonNodePath = "Panel/Toolbar/GrassButton";
    private static readonly NodePath MountainButtonNodePath = "Panel/Toolbar/MountainButton";
    private static readonly NodePath WaterButtonNodePath = "Panel/Toolbar/WaterButton";
    private static readonly NodePath ToolbarNodePath = "Panel/Toolbar";

    private World _world;
    private Button _grassButton;
    private Button _mountainButton;
    private Button _waterButton;
    private TileState? _selectedState;

    public bool IsBuildMode => _selectedState.HasValue;

    public override void _Ready()
    {
        OffsetTop = -PanelHeight;
        var toolbar = GetNodeOrNull<HBoxContainer>(ToolbarNodePath);
        if (toolbar != null)
        {
            toolbar.OffsetLeft = -ToolbarHalfWidth;
            toolbar.OffsetTop = -ToolbarHalfHeight;
            toolbar.OffsetRight = ToolbarHalfWidth;
            toolbar.OffsetBottom = ToolbarHalfHeight;
            toolbar.AddThemeConstantOverride(GodotThemeKeys.Separation, ButtonSeparation);
        }

        _grassButton = GetNodeOrNull<Button>(GrassButtonNodePath);
        _mountainButton = GetNodeOrNull<Button>(MountainButtonNodePath);
        _waterButton = GetNodeOrNull<Button>(WaterButtonNodePath);

        SetButtonSize(_grassButton);
        SetButtonSize(_mountainButton);
        SetButtonSize(_waterButton);

        BindButton(_grassButton, TileState.Grass);
        BindButton(_mountainButton, TileState.Mountain);
        BindButton(_waterButton, TileState.Water);
        RefreshButtons();
    }

    public void Initialize(World world)
    {
        _world = world;
        LoadMap();
    }

    public void ToggleBuildState(TileState state)
    {
        _selectedState = _selectedState == state ? null : state;
        RefreshButtons();
    }

    public void CancelBuildMode()
    {
        if (!IsBuildMode) return;
        _selectedState = null;
        RefreshButtons();
    }

    public bool TryBuild(Tile tile)
    {
        if (!_selectedState.HasValue || tile == null || tile.IsOccupied) return false;
        tile.SetState(_selectedState.Value);
        return true;
    }

    public void SaveMap()
    {
        if (_world == null) return;

        string absoluteDirectory = ProjectSettings.GlobalizePath(ScenePaths.MapDirectory);
        DirAccess.MakeDirRecursiveAbsolute(absoluteDirectory);

        var map = new MapBoardResource();
        foreach (var entry in _world.Tiles)
        {
            map.Positions.Add(entry.Key);
            map.TileStates.Add((int)entry.Value.State);
        }
        ResourceSaver.Save(map, ScenePaths.CurrentMap);
    }

    private void LoadMap()
    {
        if (_world == null || !ResourceLoader.Exists(ScenePaths.CurrentMap)) return;
        var map = GD.Load<MapBoardResource>(ScenePaths.CurrentMap);
        if (map == null) return;

        int count = Mathf.Min(map.Positions.Count, map.TileStates.Count);
        for (int index = 0; index < count; index++)
        {
            _world.GetTile(map.Positions[index])?.SetState((TileState)map.TileStates[index]);
        }
    }

    private void BindButton(Button button, TileState state)
    {
        if (button != null) button.Pressed += () => ToggleBuildState(state);
    }

    private static void SetButtonSize(Button button)
    {
        if (button != null) button.CustomMinimumSize = new Vector2(TileButtonSize, TileButtonSize);
    }

    private void RefreshButtons()
    {
        ApplyButtonStyle(_grassButton, GrassButtonColor, _selectedState == TileState.Grass);
        ApplyButtonStyle(_mountainButton, MountainButtonColor, _selectedState == TileState.Mountain);
        ApplyButtonStyle(_waterButton, WaterButtonColor, _selectedState == TileState.Water);
    }

    private static void ApplyButtonStyle(Button button, Color color, bool selected)
    {
        if (button == null) return;
        Color backgroundColor = selected ? color * SelectedColorMultiplier : color;
        int borderWidth = selected ? SelectedButtonBorderWidth : ButtonBorderWidth;
        Color borderColor = selected ? SelectedButtonBorderColor : ButtonBorderColor;
        var style = new StyleBoxFlat
        {
            BgColor = backgroundColor,
            BorderColor = borderColor,
            BorderWidthLeft = borderWidth,
            BorderWidthTop = borderWidth,
            BorderWidthRight = borderWidth,
            BorderWidthBottom = borderWidth
        };
        button.AddThemeStyleboxOverride(GodotThemeKeys.Normal, style);
        button.AddThemeStyleboxOverride(GodotThemeKeys.Hover, style);
        button.AddThemeStyleboxOverride(GodotThemeKeys.Pressed, style);
    }
}
