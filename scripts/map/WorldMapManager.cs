using GameTemplate.scripts.map;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class WorldMapManager : Node3D
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
    public TerrainMapper TerrainMapper;
    public Dictionary<Vector2I, MapDataItem> MapData;

    public Vector2I WorldToCell(Vector3 worldPos)
    {
        //var initialCellPos = new Vector2I((int)Terrain.GlobalTransform.Origin.X, (int)Terrain.GlobalTransform.Origin.Z);

        var inputCellPos = new Vector2I((int)worldPos.X, (int)worldPos.Z);
        //TerrainMapper.directions
        var dir = new Vector2I(1, 1);
        var multiplier = dir / CellSize;

        var cellPos = multiplier * inputCellPos;

        return cellPos;

        //dir* cellSize

        //return MapData.First().Key;
    }

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