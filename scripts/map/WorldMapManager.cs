using Godot;
using SacaSimulationGame.scripts.pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
namespace SacaSimulationGame.scripts.map
{
    public partial class WorldMapManager : Node3D, IWorldMapManager
    {
        [Export]
        public Vector3I CellSize { get; set; } = new Vector3I(4, 4, 4);

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

        public int MinX { get; private set; }
        public int MinY { get; private set; }
        public int MaxX { get; private set; }
        public int MaxY { get; private set; }
        public int MapWidth { get; private set; }
        public int MapHeight {  get; private set; }
        public Dictionary<Vector2I, MapDataItem> MapData { get; set; } 

        public AstarPathfinder Pathfinder { get; private set; }

        public Vector3 CellToWorld(Vector2I cell, float height = 0, bool centered = false)
        {
            var origin = Terrain.GlobalPosition;
            //var worldPos = origin + cell * CellSize;

            var worldPos = new Vector3(origin.X + cell.X * CellSize.X, height, origin.Z + cell.Y * CellSize.Z);
            if (centered)
            {
                worldPos.X += CellSize.X / 2f;
                worldPos.Z += CellSize.Z / 2f;
            }

            return worldPos;
        }
        /// <summary>
        /// Results a world position on a interpolated position inside the cell
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="height"></param>
        /// <param name="centered"></param>
        /// <returns></returns>
        public Vector3 CellToWorldInterpolated(Vector2 cell, float height = 0)
        {
            var origin = Terrain.GlobalPosition;

            var worldPos = new Vector3(origin.X + cell.X * CellSize.X, height, origin.Z + cell.Y * CellSize.Z);

            return worldPos;
        }


        public Vector2I WorldToCell(Vector3 worldPos)
        {
            var origin = new Vector2(Terrain.GlobalPosition.X, Terrain.GlobalPosition.Z);

            var worldPos2d = new Vector2(worldPos.X, worldPos.Z);
            var relativePos = worldPos2d - origin;

            var cellX = Mathf.FloorToInt(relativePos.X / CellSize.X);
            var cellY = Mathf.FloorToInt(relativePos.Y / CellSize.Z);

            return new Vector2I(cellX, cellY);
        }

        private (int minX, int minY, int maxX, int maxY) GetMapDimensions(Dictionary<Vector2I,MapDataItem> mapData) 
        {
            var minX = mapData.Keys.Min(k => k.X);
            var minY = mapData.Keys.Min(k => k.Y);
            var maxX = mapData.Keys.Max(k => k.X);
            var maxY = mapData.Keys.Max(k => k.Y);
            return (minX, minY, maxX, maxY);
        }

        public override void _Ready()
        {
            GradientVisualizer = GetNode<TerrainGradientVisualizer>("TerrainGradientVisualizer");
            TerrainMapper = GetNode<TerrainMapper>("TerrainMapper");

            MapData = TerrainMapper.LoadMapdata(Terrain, CellSize, this.MapPropertiesCacheEnabled);
            (this.MinX, this.MinY, this.MaxX, this.MaxY) = GetMapDimensions(MapData);
            this.MapWidth = this.MaxX - this.MinX;
            this.MapHeight = this.MaxY - this.MinY;

            // Initialize gradient
            GradientVisualizer.Position = new Vector3(
                Terrain.GlobalTransform.Origin.X,
                GradientVisualizer.GlobalPosition.Y,
                Terrain.GlobalTransform.Origin.Z
            );

            GradientVisualizer.SetGradients(MapData, CellSize);
            GradientVisualizer.ShowSlopeGradients = ShowSlopeGradients;

            this.Pathfinder = new AstarPathfinder(this);
        }
    }
}