using Godot;
using SacaSimulationGame.scripts.buildings;
using SacaSimulationGame.scripts.map;
using SacaSimulationGame.scripts.pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SacaSimulationGame.scripts.managers
{

    public struct CellOccupationData
    {
        public bool Unit;
        public bool NaturalResource;
        public bool Building;
        public bool IsOccupied => Unit || NaturalResource || Building;
    }
    public partial class WorldMapManager : Node3D, IWorldMapManager
    {
        [Export]
        public Vector3I CellSize { get; set; } = new Vector3I(4, 4, 4);

        public Node3D Terrain { get; set; }

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
        public int MapLength { get; private set; }
        //private Dictionary<Vector2I, MapDataItem> MapData2 { get; set; }
        private MapDataItem[,] MapData { get; set; }

        public bool CellIsTraversable(Vector2I cell, CellType allowedTerrainTypes, BuildingType obstacleBuildings = BuildingType.ObstacleBuildings)
        {
            //if (!CellInsideBounds(cell)) return false;
            var cellExists = GameManager.MapManager.TryGetCell(cell, out var neighborData);
            if (!cellExists) return false;

            var obstacleAtCell = GameManager.BuildingManager.OccupiedCells[cell.X, cell.Y];
            //var cellType = this.MapData[cell.X, cell.Y].CellType;

            //if (neighborData.CellType != allowedTerrainTypes)
            if ((allowedTerrainTypes & neighborData.CellType) == 0)
            {
                if (allowedTerrainTypes.HasFlag(CellType.GROUND) && obstacleAtCell.BuildingCompleted && obstacleAtCell.Type == BuildingType.Bridge)
                {
                    // its a bridge, so its traversable
                }
                else return false;
            }

            if ((obstacleAtCell.Type & obstacleBuildings) > 0)
            {
                return false;
            }

            return true;
        }

        public MapDataItem GetCell(Vector2I cell)
        {

            if (!CellInsideBounds(cell))
            {
                GD.Print("coordinate outside bounds");
                return null;
            }

            return this.MapData[cell.X, cell.Y];

            //if (!this.MapData.TryGetValue(cell, out MapDataItem content))
            //{
            //    return null;
            //}
            //return content;
        }
        public bool CellInsideBounds(Vector2I cell)
        {
            if (cell.X < 0 || cell.Y < 0 || cell.X >= MapWidth || cell.Y >= MapLength)
            {
                return false;
            }
            return true;
            //return this.MapData.TryGetValue(cell, out MapDataItem _);
        }


        public bool TryGetCell(Vector2I cell, out MapDataItem item)
        {
            var data = GetCell(cell);
            if (data != null)
            {
                item = data;
                return true;
            }
            item = null;
            return false;
        }
        public int CellCount => MapData.Length;

        public GameManager GameManager { get; private set; }
        public AstarPathfinder Pathfinder { get; private set; }


        /// <summary>
        /// Gets whether a cell is occupied by any unit
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public CellOccupationData GetCellOccupation(Vector2I cell)
        {
            try
            {
                return new CellOccupationData
                {
                    Building = this.GameManager.BuildingManager.OccupiedCells[cell.X, cell.Y].Type != BuildingType.None,
                    NaturalResource = this.GameManager.NaturalResourceManager.OccupiedCells[cell.X, cell.Y]
                };
            }
            catch(Exception ex)
            {
                GD.PrintErr($"The cell Index was out of range, cell: {cell}, mapdimensions:(w:{GameManager.MapManager.MapWidth}, l:{GameManager.MapManager.MapLength})", ex);
                throw;
            }
        }



        #region cell mapping to world
        public Vector3 CellToWorld(Vector2I cell, float height = 0, bool centered = false)
        {
            var origin = Terrain.GlobalPosition;
            // Since we removed this when calculating cellnumbers, now we have to add them
            cell += new Vector2I(MinX, MinY);

            var worldPos = new Vector3(origin.X + cell.X * CellSize.X, height, origin.Z + cell.Y * CellSize.Z);

            worldPos += new Vector3(CellSize.X, 0, CellSize.Z); //is this a hack?

            if (centered)
            {
                worldPos.X -= CellSize.X / 2f;
                worldPos.Z -= CellSize.Z / 2f;
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

            cell += new Vector2I(MinX, MinY);

            var worldPos = new Vector3(
                origin.X + cell.X * CellSize.X,
                height,
                origin.Z + cell.Y * CellSize.Z
            );


            worldPos += new Vector3(CellSize.X, 0, CellSize.Z); //is this a hack?

            return worldPos;
        }

        public bool WorldPosInsideBounds(Vector3 worldPos)
        {
            var cell = WorldToCell(worldPos);
            return CellInsideBounds(cell);
        }


        public Vector2I WorldToCell(Vector3 worldPos)
        {
            var origin = new Vector2(Terrain.GlobalPosition.X, Terrain.GlobalPosition.Z);

            var worldPos2d = new Vector2(worldPos.X, worldPos.Z);
            var relativePos = worldPos2d - origin;

            var cellX = Mathf.FloorToInt(relativePos.X / CellSize.X);
            var cellY = Mathf.FloorToInt(relativePos.Y / CellSize.Z);

            // Since we added this when calculating cellnumbers, now we have to remove them
            cellX -= this.MinX;
            cellY -= this.MinY;



            return new Vector2I(cellX, cellY);
        }

        #endregion

        private (int minX, int minY, int maxX, int maxY) GetMapDimensions(Dictionary<Vector2I, MapDataItem> mapData)
        {
            var minX = mapData.Keys.Min(k => k.X);
            var minY = mapData.Keys.Min(k => k.Y);
            var maxX = mapData.Keys.Max(k => k.X);
            var maxY = mapData.Keys.Max(k => k.Y);

            return (minX, minY, maxX, maxY);
        }

        private MapDataItem[,] TransformMapdataToArray(Dictionary<Vector2I, MapDataItem> mapData)
        {
            MapDataItem[,] mapArray = new MapDataItem[MapWidth, MapLength];

            foreach (var kvp in mapData)
            {
                Vector2I position = kvp.Key;
                MapDataItem item = kvp.Value;

                int arrayX = position.X - MinX;
                int arrayY = position.Y - MinY;
                if (item == null)
                {
                    GD.Print($"WARNING: item is set to null at {position}");
                }
                mapArray[arrayX, arrayY] = item;
            }

            return mapArray;
        }

        private void StoreMapdata(Dictionary<Vector2I, MapDataItem> mapData)
        {
            (this.MinX, this.MinY, this.MaxX, this.MaxY) = GetMapDimensions(mapData);
            this.MapWidth = this.MaxX - this.MinX + 1;
            this.MapLength = this.MaxY - this.MinY + 1;

            this.MapData = this.TransformMapdataToArray(mapData);
        }

        public override void _Ready()
        {
            var treeRoot = FindParent("root");
            this.RiverCollider = treeRoot.GetNode("RiverManagerMesh").GetNode<StaticBody3D>("StaticBody3D");
            this.Terrain = treeRoot.GetNode<Node3D>("Map");


            GradientVisualizer = GetNode<TerrainGradientVisualizer>("TerrainGradientVisualizer");
            TerrainMapper = GetNode<TerrainMapper>("TerrainMapper");

            var mapData = TerrainMapper.LoadMapdata(Terrain, CellSize, this.MapPropertiesCacheEnabled);
            StoreMapdata(mapData);

            // Initialize gradient
            GradientVisualizer.Position = new Vector3(
                Terrain.GlobalTransform.Origin.X,
                GradientVisualizer.GlobalPosition.Y,
                Terrain.GlobalTransform.Origin.Z
            );

            GradientVisualizer.SetGradients(MapData, CellSize, this);
            GradientVisualizer.ShowSlopeGradients = ShowSlopeGradients;


            this.GameManager = this.GetParent<GameManager>();
            this.Pathfinder = new AstarPathfinder(this.GameManager);
        }
    }
}