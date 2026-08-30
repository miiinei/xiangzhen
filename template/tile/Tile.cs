using Godot;

public enum TileState
{
    Grass = 0,
    Mountain = 1,
    Water = 2
}

public partial class Tile : Area2D
{
    private static readonly Color NormalBorderColor = new(0.55f, 0.55f, 0.55f, 1f);
    private static readonly Texture2D MountainTexture = GD.Load<Texture2D>("res://template/tile/Mountain.png");
    private const int HighlightedZIndex = 100;
    private const int DefaultZIndex = 0;
    private TileState _state;
    private Camera2D _camera;

    [Export(PropertyHint.Range, "1.0,5.0,0.1")]
    public float DynamicActivationScale { get; set; } = 2.5f;

    [Export]
    public Color HighlightBorderColor { get; set; } = new(0.25f, 0.55f, 0.75f, 1f);

    private Polygon2D _rangeOverlay;

    public bool IsDynamicVisible { get; private set; } = true;

    [Export]
    public int BianChang { get; set; } = 64;

    [Export(PropertyHint.Range, "0.5,1.0,0.01")]
    public float YCompression { get; set; } = 0.9f;

    [Export(PropertyHint.Range, "20.0,200.0,1.0")]
    public float GrassWindWaveSpacing { get; set; } = 70f;

    [Export(PropertyHint.Range, "0.1,3.0,0.1")]
    public float GrassWindSpeed { get; set; } = 1f;

    [Export(PropertyHint.Range, "1.0,8.0,1.0")]
    public float GrassPixelSize { get; set; } = 3f;

    [Export(PropertyHint.Range, "0.5,20.0,0.5")]
    public float GrassJitterFrequency { get; set; } = 5f;

    [Export(PropertyHint.Range, "1.0,16.0,1.0")]
    public float GrassJitterBlockSize { get; set; } = 3f;

    [Export(PropertyHint.Range, "8.0,64.0,1.0")]
    public float GrassMottledPatchSize { get; set; } = 24f;

    [Export(PropertyHint.Range, "0.0,3.0,0.05")]
    public float GrassMottledSoftness { get; set; } = 0.35f;

    [Export(PropertyHint.Range, "0.05,6.0,0.05")]
    public float GrassMottledInvasionFrequency { get; set; } = 1.5f;

    [Export(PropertyHint.Range, "0.0,32.0,0.5")]
    public float GrassMottledInvasionDepth { get; set; } = 8f;

    [Export]
    public Color GrassDarkColor { get; set; } = new(0.23f, 0.36f, 0.24f, 1f);

    [Export]
    public Color GrassBaseColor { get; set; } = new(0.24f, 0.37f, 0.25f, 1f);

    [Export]
    public Color GrassLightColor { get; set; } = new(0.25f, 0.38f, 0.26f, 1f);

    [Export]
    public Vector2 GrassWindDirection { get; set; } = new(-1f, 1f);

    [Export(PropertyHint.Range, "0.0,45.0,1.0")]
    public float GrassWindDivergence { get; set; } = 14f;

    [Export]
    public CollisionPolygon2D Shape { get; set; }

    [Export]
    public Line2D SelectionLine { get; set; }

    [Export]
    public Sprite2D TileSprite { get; set; }

    [Export]
    public Polygon2D GrassPolygon { get; set; }

    [Export]
    public Polygon2D WindOverlay { get; set; }

    [Export]
    public Polygon2D WaterPolygon { get; set; }

    [Export]
    public Polygon2D WaterOverlay { get; set; }

    [Export]
    public TileState State
    {
        get => _state;
        set
        {
            _state = value;
            if (IsNodeReady())
            {
                UpdateTileVisual();
            }
        }
    }

    public int ObstacleGroup { get; private set; }

    [Export]
    public Vector2I GridPosition { get; set; }
    public World WorldRef { get; set; }

    private Piece _occupiedPiece;

    public Piece OccupiedPiece
    {
        get => _occupiedPiece;
        set
        {
            _occupiedPiece = value;
            if (_occupiedPiece != null)
            {
                SyncPieceZIndex(ZIndex);
            }
            else
            {
                ZIndex = DefaultZIndex;
            }
        }
    }

    // Shadows Node2D.ZIndex so the occupying piece is kept exactly 1 above.
    public new int ZIndex
    {
        get => base.ZIndex;
        set
        {
            base.ZIndex = value;
            SyncPieceZIndex(value);
        }
    }

    private void SyncPieceZIndex(int tileZIndex)
    {
        if (_occupiedPiece == null)
        {
            return;
        }

        _occupiedPiece.ZIndex = tileZIndex + 1;
    }

    public bool IsBlocked => ObstacleGroup != 0;

    public override void _Ready()
    {
        _camera = GetViewport()?.GetCamera2D();

        if (Shape == null)
        {
            Shape = GetNode<CollisionPolygon2D>("CollisionShape2D");
        }

        if (SelectionLine == null)
        {
            SelectionLine = GetNodeOrNull<Line2D>("SelectionLine");
        }

        if (TileSprite == null)
        {
            TileSprite = GetNodeOrNull<Sprite2D>("Sprite2D");
        }

        if (GrassPolygon == null)
        {
            GrassPolygon = GetNodeOrNull<Polygon2D>("GrassPolygon");
        }

        if (WindOverlay == null)
        {
            WindOverlay = GetNodeOrNull<Polygon2D>("WindOverlay");
        }

        if (WaterPolygon == null)
        {
            WaterPolygon = GetNodeOrNull<Polygon2D>("WaterPolygon");
        }

        if (WaterOverlay == null)
        {
            WaterOverlay = GetNodeOrNull<Polygon2D>("WaterOverlay");
        }

        var hexagon = BuildRegularHexagon(BianChang, YCompression);

        if (Shape != null)
        {
            Shape.Polygon = hexagon;
        }

        if (GrassPolygon != null)
        {
            GrassPolygon.Polygon = hexagon;
            UpdateGrassWindWaveSpacing();
            UpdateGrassWindSpeed();
            UpdateGrassPixelSize();
            UpdateGrassJitterFrequency();
            UpdateGrassJitterBlockSize();
            UpdateGrassMottledPatchSize();
            UpdateGrassMottledSoftness();
            UpdateGrassMottledInvasionFrequency();
            UpdateGrassMottledInvasionDepth();
            UpdateGrassColors();
            UpdateGrassWindDirection();
            UpdateGrassWindDivergence();
        }

        if (WindOverlay != null)
        {
            WindOverlay.Polygon = hexagon;
        }

        if (WaterPolygon != null)
        {
            WaterPolygon.Polygon = hexagon;
        }

        if (WaterOverlay != null)
        {
            WaterOverlay.Polygon = hexagon;
        }

        UpdateTileVisual();
        UpdateDynamicVisibility();
    }

    public override void _Process(double delta)
    {
        if (_camera == null)
        {
            _camera = GetViewport()?.GetCamera2D();
        }

        UpdateDynamicVisibility();
    }

    private void UpdateDynamicVisibility()
    {
        if (GetViewport() == null)
        {
            SetTileDynamicEnabled(false);
            return;
        }

        if (_camera == null)
        {
            _camera = GetViewport().GetCamera2D();
        }

        if (_camera == null)
        {
            SetTileDynamicEnabled(false);
            return;
        }

        Vector2 zoom = _camera.Zoom;
        if (zoom.X <= 0f || zoom.Y <= 0f)
        {
            SetTileDynamicEnabled(false);
            return;
        }

        Rect2 viewportRect = GetViewportRect();
        Vector2 viewportSize = viewportRect.Size;
        Vector2 viewportCenter = viewportSize * 0.5f;
        Vector2 relativeToCamera = GlobalPosition - _camera.GlobalPosition;
        Vector2 screenPosition = relativeToCamera * zoom + viewportCenter;

        Vector2 activationHalfSize = viewportSize * 0.5f * DynamicActivationScale;
        Rect2 activationRect = new Rect2(
            viewportCenter - activationHalfSize,
            activationHalfSize * 2f
        );

        SetTileDynamicEnabled(activationRect.HasPoint(screenPosition));
    }

    private void SetTileDynamicEnabled(bool enabled)
    {
        IsDynamicVisible = enabled;

        bool isGrass = State == TileState.Grass;
        bool isWater = State == TileState.Water;

        if (GrassPolygon != null)
        {
            GrassPolygon.Visible = enabled && isGrass;
        }

        if (WindOverlay != null)
        {
            WindOverlay.Visible = enabled && isGrass;
        }

        if (WaterPolygon != null)
        {
            WaterPolygon.Visible = enabled && isWater;
        }

        if (WaterOverlay != null)
        {
            WaterOverlay.Visible = enabled && isWater;
        }

        SetProcessInput(enabled);
        SetProcessUnhandledInput(enabled);
        SetProcessShortcutInput(enabled);

        if (GrassPolygon?.Material is ShaderMaterial grassMaterial)
        {
            grassMaterial.SetShaderParameter("enable_dynamic", enabled);
        }

        if (WindOverlay?.Material is ShaderMaterial windMaterial)
        {
            windMaterial.SetShaderParameter("enable_dynamic", enabled);
        }

        if (WaterPolygon?.Material is ShaderMaterial waterMaterial)
        {
            waterMaterial.SetShaderParameter("enable_dynamic", enabled);
        }

        if (WaterOverlay?.Material is ShaderMaterial waterOverlayMaterial)
        {
            waterOverlayMaterial.SetShaderParameter("enable_dynamic", enabled);
        }
    }

    private void UpdateGrassWindWaveSpacing()
    {
        if (GrassPolygon?.Material is not ShaderMaterial grassMaterial)
        {
            return;
        }

        grassMaterial.SetShaderParameter("wind_wave_spacing", GrassWindWaveSpacing);

        if (WindOverlay?.Material is ShaderMaterial windMaterial)
        {
            windMaterial.SetShaderParameter("wind_wave_spacing", GrassWindWaveSpacing);
        }

        if (WaterPolygon?.Material is ShaderMaterial waterMaterial)
        {
            waterMaterial.SetShaderParameter("wind_wave_spacing", GrassWindWaveSpacing);
        }

        if (WaterOverlay?.Material is ShaderMaterial waterOverlayMaterial)
        {
            waterOverlayMaterial.SetShaderParameter("wind_wave_spacing", GrassWindWaveSpacing);
        }
    }

    private void UpdateGrassWindSpeed()
    {
        if (GrassPolygon?.Material is ShaderMaterial grassMaterial)
        {
            grassMaterial.SetShaderParameter("wind_speed", GrassWindSpeed);
        }

        if (WindOverlay?.Material is ShaderMaterial windMaterial)
        {
            windMaterial.SetShaderParameter("wind_speed", GrassWindSpeed);
        }

        if (WaterPolygon?.Material is ShaderMaterial waterMaterial)
        {
            waterMaterial.SetShaderParameter("wind_speed", GrassWindSpeed);
        }

        if (WaterOverlay?.Material is ShaderMaterial waterOverlayMaterial)
        {
            waterOverlayMaterial.SetShaderParameter("wind_speed", GrassWindSpeed);
        }
    }

    private void UpdateGrassPixelSize()
    {
        if (GrassPolygon?.Material is ShaderMaterial grassMaterial)
        {
            grassMaterial.SetShaderParameter("pixel_size", GrassPixelSize);
        }

        if (WaterPolygon?.Material is ShaderMaterial waterMaterial)
        {
            waterMaterial.SetShaderParameter("pixel_size", GrassPixelSize);
        }
    }

    private void UpdateGrassJitterFrequency()
    {
        if (GrassPolygon?.Material is ShaderMaterial grassMaterial)
        {
            grassMaterial.SetShaderParameter("jitter_frequency", GrassJitterFrequency);
        }

        if (WaterPolygon?.Material is ShaderMaterial waterMaterial)
        {
            waterMaterial.SetShaderParameter("jitter_frequency", GrassJitterFrequency);
        }
    }

    private void UpdateGrassJitterBlockSize()
    {
        if (GrassPolygon?.Material is ShaderMaterial grassMaterial)
        {
            grassMaterial.SetShaderParameter("jitter_block_size", GrassJitterBlockSize);
        }

        if (WaterPolygon?.Material is ShaderMaterial waterMaterial)
        {
            waterMaterial.SetShaderParameter("jitter_block_size", GrassJitterBlockSize);
        }
    }

    private void UpdateGrassMottledPatchSize()
    {
        if (GrassPolygon?.Material is ShaderMaterial grassMaterial)
        {
            grassMaterial.SetShaderParameter("mottled_patch_size", GrassMottledPatchSize);
        }
    }

    private void UpdateGrassMottledSoftness()
    {
        if (GrassPolygon?.Material is ShaderMaterial grassMaterial)
        {
            grassMaterial.SetShaderParameter("mottled_softness", GrassMottledSoftness);
        }
    }

    private void UpdateGrassMottledInvasionFrequency()
    {
        if (GrassPolygon?.Material is ShaderMaterial grassMaterial)
        {
            grassMaterial.SetShaderParameter("mottled_invasion_frequency", GrassMottledInvasionFrequency);
        }
    }

    private void UpdateGrassMottledInvasionDepth()
    {
        if (GrassPolygon?.Material is ShaderMaterial grassMaterial)
        {
            grassMaterial.SetShaderParameter("mottled_invasion_depth", GrassMottledInvasionDepth);
        }
    }

    private void UpdateGrassColors()
    {
        if (GrassPolygon?.Material is ShaderMaterial grassMaterial)
        {
            grassMaterial.SetShaderParameter("dark_color", GrassDarkColor);
            grassMaterial.SetShaderParameter("base_color", GrassBaseColor);
            grassMaterial.SetShaderParameter("light_color", GrassLightColor);
        }
    }

    private void UpdateGrassWindDirection()
    {
        if (GrassPolygon?.Material is ShaderMaterial grassMaterial)
        {
            grassMaterial.SetShaderParameter("wind_direction", GrassWindDirection);
        }

        if (WindOverlay?.Material is ShaderMaterial windMaterial)
        {
            windMaterial.SetShaderParameter("wind_direction", GrassWindDirection);
        }

        if (WaterPolygon?.Material is ShaderMaterial waterMaterial)
        {
            waterMaterial.SetShaderParameter("wind_direction", GrassWindDirection);
        }

        if (WaterOverlay?.Material is ShaderMaterial waterOverlayMaterial)
        {
            waterOverlayMaterial.SetShaderParameter("wind_direction", GrassWindDirection);
        }
    }

    private void UpdateGrassWindDivergence()
    {
        if (GrassPolygon?.Material is ShaderMaterial grassMaterial)
        {
            grassMaterial.SetShaderParameter("wind_divergence", GrassWindDivergence);
        }

        if (WindOverlay?.Material is ShaderMaterial windMaterial)
        {
            windMaterial.SetShaderParameter("wind_divergence", GrassWindDivergence);
        }

        if (WaterPolygon?.Material is ShaderMaterial waterMaterial)
        {
            waterMaterial.SetShaderParameter("wind_divergence", GrassWindDivergence);
        }

        if (WaterOverlay?.Material is ShaderMaterial waterOverlayMaterial)
        {
            waterOverlayMaterial.SetShaderParameter("wind_divergence", GrassWindDivergence);
        }
    }

    private void UpdateTileVisual()
    {
        var hexagon = BuildRegularHexagon(BianChang, YCompression);
        bool isGrass = State == TileState.Grass;
        bool isWater = State == TileState.Water;

        if (GrassPolygon != null)
        {
            GrassPolygon.Polygon = hexagon;
            GrassPolygon.Visible = isGrass && IsDynamicVisible;
        }

        if (WindOverlay != null)
        {
            WindOverlay.Polygon = hexagon;
            WindOverlay.Visible = isGrass && IsDynamicVisible;
        }

        if (WaterPolygon != null)
        {
            WaterPolygon.Polygon = hexagon;
            WaterPolygon.Visible = isWater && IsDynamicVisible;
        }

        if (WaterOverlay != null)
        {
            WaterOverlay.Polygon = hexagon;
            WaterOverlay.Visible = isWater && IsDynamicVisible;
        }

        if (TileSprite != null)
        {
            TileSprite.Visible = !isGrass && !isWater;
            TileSprite.Scale = new Vector2(TileSprite.Scale.X, YCompression);
            TileSprite.Position = new Vector2(
                TileSprite.Position.X,
                State == TileState.Mountain ? -26f : 0f
            );
            TileSprite.ZIndex = State == TileState.Mountain ? 1 : 0;
            TileSprite.Texture = State switch
            {
                TileState.Mountain => MountainTexture,
                _ => null
            };
        }

        ObstacleGroup = State == TileState.Mountain ? 1 : 0;

        if (SelectionLine != null)
        {
            SelectionLine.Points = hexagon;
            SelectionLine.DefaultColor = NormalBorderColor;
            SelectionLine.Width = 2f;
            SelectionLine.Closed = true;
            SelectionLine.Visible = true;
        }

        if (_rangeOverlay != null)
        {
            _rangeOverlay.Polygon = hexagon;
        }
    }

    public void SetRangeOverlay(bool enabled, Color? color = null)
    {
        if (_rangeOverlay == null)
        {
            _rangeOverlay = new Polygon2D
            {
                Name = "RangeOverlay",
                Polygon = BuildRegularHexagon(BianChang, YCompression),
                ZIndex = 2,
                Visible = false
            };
            AddChild(_rangeOverlay);
        }

        if (enabled)
        {
            _rangeOverlay.Polygon = BuildRegularHexagon(BianChang, YCompression);
            _rangeOverlay.Color = color ?? new Color(1f, 0.2f, 0.2f, 0.35f);
            _rangeOverlay.Visible = true;
        }
        else
        {
            _rangeOverlay.Visible = false;
        }
    }

    public void SetSelected(bool selected)
    {
        ZIndex = selected ? HighlightedZIndex : DefaultZIndex;

        if (SelectionLine != null)
        {
            SelectionLine.Visible = true;
            SelectionLine.DefaultColor = selected
                ? HighlightBorderColor
                : NormalBorderColor;
        }
    }

    public void OnTileInput(Viewport viewport, InputEvent @event, long shapeIdx)
    {
        if (!IsDynamicVisible)
        {
            return;
        }

        if (@event is InputEventMouseMotion)
        {
            if (WorldRef != null)
            {
                WorldRef.HandleTileHover(this);
            }

            return;
        }

        if (!(@event is InputEventMouseButton mouseButton))
        {
            return;
        }

        if (!mouseButton.Pressed || mouseButton.ButtonIndex != MouseButton.Left)
        {
            return;
        }

        if (WorldRef != null)
        {
            WorldRef.HandleTileClick(this);
        }
    }

    public void SetGridPosition(Vector2I position)
    {
        GridPosition = position;
    }

    public Vector2[] GetHexagonVertices()
    {
        return BuildRegularHexagon(BianChang, YCompression);
    }

    public static Vector2[] BuildRegularHexagon(float sideLength, float yCompression)
    {
        var points = new Vector2[6];
        // yCompression is applied to the Y component of each vertex
        for (int i = 0; i < 6; i++)
        {
            float angle = Mathf.DegToRad(30 + i * 60);
            points[i] = new Vector2(
                Mathf.Cos(angle) * sideLength,
                Mathf.Sin(angle) * sideLength * yCompression
            );
        }

        return points;
    }

}
