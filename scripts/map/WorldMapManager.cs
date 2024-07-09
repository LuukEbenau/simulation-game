using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
namespace SacaSimulationGame.scripts.map
{
    public partial class WorldMapManager : Node3D, IWorldMapManager
    {
        [Export]
        public Vector2I CellSize { get; set; } = new Vector2I(4, 4);

        [Export]
        public Node3D Terrain { get; set; }

        [Export]
        public StaticBody3D RiverCollider { get; set; }

        [Export]
        public bool MapPropertiesCacheEnabled { get; set; } = true;


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
        public TerrainMapper TerrainMapper { get; set; }
        public Dictionary<Vector2I, MapDataItem> MapData { get; set; } 


        public Vector2 CellToWorld(Vector2I cell, bool centered = false)
        {
            var origin = new Vector2(Terrain.GlobalPosition.X, Terrain.GlobalPosition.Z);
            var worldPos = origin + cell * CellSize;
            if (centered)
            {
                worldPos.X +=  CellSize.X/2f;
                worldPos.Y += CellSize.Y / 2f;
            }
            return worldPos;
        }

        public Vector2I WorldToCell(Vector3 worldPos)
        {
            var origin = new Vector2(Terrain.GlobalPosition.X, Terrain.GlobalPosition.Z);

            var worldPos2d = new Vector2(worldPos.X, worldPos.Z);
            var relativePos = worldPos2d - origin;

            var cellX = Mathf.FloorToInt(relativePos.X / CellSize.X);
            var cellY = Mathf.FloorToInt(relativePos.Y / CellSize.Y);
            return new Vector2I(cellX, cellY);

        }

        public override void _Ready()
        {
            GradientVisualizer = GetNode<TerrainGradientVisualizer>("TerrainGradientVisualizer");
            TerrainMapper = GetNode<TerrainMapper>("TerrainMapper");

            MapData = TerrainMapper.LoadMapdata(Terrain, CellSize, this.MapPropertiesCacheEnabled);

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
}