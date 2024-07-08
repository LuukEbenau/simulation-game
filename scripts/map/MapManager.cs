using GameTemplate.scripts.map;
using Godot;
using System;
using System.Collections.Generic;

public partial class MapManager : Node3D
{
    [Export]
    public Vector2I CellSize { get; set; } = new Vector2I(1, 1);

    [Export]
    public Node3D Terrain { get; set; }

    [Export]
    public bool ShowSlopeGradients
    {
        get => _showSlopeGradients;
        set
        {
            _showSlopeGradients = value;
            if (GradientVisualizer != null)
            {
                GradientVisualizer.ShowSlopeGradients = value;
            }
        }
    }
    private bool _showSlopeGradients = true;

    private TerrainGradientVisualizer GradientVisualizer;
    private TerrainMapper TerrainMapper;
    private Dictionary<Vector2I, MapDataItem> MapData;

    public override void _Ready()
    {
        GradientVisualizer = GetNode<TerrainGradientVisualizer>("TerrainGradientVisualizer");
        TerrainMapper = GetNode<TerrainMapper>("TerrainMapper");

        MapData = TerrainMapper.LoadMapdata(Terrain, CellSize);

        // Initialize gradient
        GradientVisualizer.Position = new Vector3(
            Terrain.GlobalTransform.Origin.X,
            GradientVisualizer.GlobalPosition.Y,
            Terrain.GlobalTransform.Origin.Z
        );

        GradientVisualizer.SetGradients(MapData, CellSize);
        GradientVisualizer.ShowSlopeGradients = ShowSlopeGradients;
    }
}