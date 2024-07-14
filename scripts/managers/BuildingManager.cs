using Godot;
using SacaSimulationGame.scripts;
using SacaSimulationGame.scripts.buildings;
using SacaSimulationGame.scripts.buildings.dataObjects;
using SacaSimulationGame.scripts.map;
using System.Collections.Generic;
namespace SacaSimulationGame.scripts.managers
{
    public class BuildingDataObject
    {
        private static int buildingIndexCounter = 1; //reserve id 0 for empty tiles
        public BuildingDO Building { get; set; }
        public int Id { get; }
        public List<Vector2I> OccupiedCells { get; set; }
        public Player Player { get; set; }
        public BuildingDataObject(Player player, BuildingDO building)
        {
            this.Id = buildingIndexCounter++;
            this.Player = player;
            this.Building = building;
        }
    }


    public partial class BuildingManager : Node3D
    {
        [Export]
        public PackedScene HouseBuilding { get; set; }
        [Export]
        public PackedScene RoadBuilding { get; set; }

        private WorldMapManager MapManager { get; set; }
        private Camera3D Camera { get; set; }


        private readonly Vector2I _defaultVec = new(int.MinValue, int.MinValue);
        private BuildingDO selectedBuilding = null;
        private Vector2I lastHoveredCell = default;
        private BuildingRotation buildingRotation = BuildingRotation.Top;

        private double timeElapsedSinceLastHoverUpdate = 0;
        private double hoverIndicatorUpdateInterval = 1 / 60;

        /// <summary>
        /// What do we need to know of a building?
        /// building
        /// location
        /// orientation
        /// based on this, we can find out which cells are occupied and which are not. Store this in a matrix so that its efficiently kept in sync?
        /// </summary>
        /// 

        public Dictionary<Player, List<BuildingDataObject>> buildingData;
        public int[,] occupiedCells;

        private Player dummyPlayer;
        public override void _Ready()
        {
            this.MapManager = GetParent().GetNode<WorldMapManager>("MapManager");
            if (this.MapManager.MapWidth == default)
            {
                GD.Print($"WARNING: it seems like the BuildingManager is ran before the MapManager, verify the order...");
            }
            this.Camera = GetViewport().GetCamera3D();

            this.buildingData = [];
            dummyPlayer = new Player();
            buildingData.Add(dummyPlayer, []);

            occupiedCells = new int[MapManager.MapWidth, MapManager.MapHeight];
        }


        public override void _Process(double delta)
        {
            HandleHoverBehaviour(delta);
        }

        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("Building Slot 1"))
            {
                selectedBuilding = new HouseDO();
            }
            else if (@event.IsActionPressed("Building Slot 2"))
            {
                selectedBuilding = new RoadDO();
            }
            else if (@event.IsActionPressed("Building Slot 3"))
            {
                selectedBuilding = new FishingPostDO();
            }
            else if (@event.IsActionPressed("Building Slot 4"))
            {
                selectedBuilding = new HouseDO();
            }
            else if (@event.IsActionPressed("Cancel Selection"))
            {
                selectedBuilding = null;
                ClearHoverIndicator();
            }

            if(@event.IsActionPressed("Rotate Building"))
            {
                GD.Print("rotating building");
                CycleRotation();
            }

            if (selectedBuilding != null && lastHoveredCell != default)
            {
                if (@event.IsActionPressed("Build"))
                {

                    selectedBuilding.Rotation = buildingRotation;

                    GD.Print("check building buildable");
                    if (CheckBuildingBuildable(lastHoveredCell, selectedBuilding))
                    {
                        // Build building
                        GD.Print("start building");
                        
                        if(BuildBuilding(lastHoveredCell, selectedBuilding))
                        {
                            GD.Print("building building success");
                        }
                        else
                        {
                            GD.Print("building building failed");
                        }
                    }
                }
            }
        }

        #region building feature
        public bool BuildBuilding(Vector2I cell, BuildingDO building)
        {
            if (MapManager.GetCell(cell).CellType != CellType.GROUND) {
                GD.Print("Tried to place building on terrain different than ground");
                return false; 
            }
            // Instantiate the scene
            PackedScene scene;
            if(building.Name == "House")
            {
                scene = this.HouseBuilding;
            }
            else if(building.Name == "Road")
            {
                scene = this.RoadBuilding;
            }
            else
            {
                scene = this.HouseBuilding;
            }
            Building buildingInstance = scene.Instantiate<Building>();

            if (buildingInstance == null)
            {
                GD.PrintErr("Failed to instantiate building scene");
                return false;
            }

            buildingInstance.BuildingData = building;


            Vector3 worldPosition = MapManager.CellToWorld(cell, height: MapManager.GetCell(cell).Height + 0.15f, centered: true);
            ApplyBuildingRotation(buildingInstance, building.Rotation);

            var buildingDataObject = new BuildingDataObject(player: dummyPlayer, building = building);
            buildingDataObject.OccupiedCells = CalculateOccupiedCellsByBuilding(building, cell, buildingDataObject.Id);

            this.buildingData[dummyPlayer]
                .Add(buildingDataObject);

            // Add the building to the scene
            AddChild(buildingInstance);
            buildingInstance.GlobalPosition = worldPosition;
            buildingInstance.Scale = MapManager.CellSize;

            GD.Print($"Building building at coordinate {worldPosition}. Total building count: {this.buildingData[dummyPlayer].Count}");
            return true;
        }

        /// <summary>
        /// TODO: make this function more sence, now it has side effects
        /// </summary>
        /// <param name="building"></param>
        /// <param name="cell"></param>
        /// <param name="buildingId"></param>
        /// <returns></returns>
        private List<Vector2I> CalculateOccupiedCellsByBuilding(BuildingDO building, Vector2I cell, int buildingId) {
            //// KEEP TRACK OF Building
            var occupiedCellsByBuilding = new List<Vector2I>();
            for (var occX = 0; occX < building.Shape.GetLength(0); occX++)
            {
                for (var occY = 0; occY < building.Shape.GetLength(1); occY++)
                {
                    var occXMap = cell.X + occX;
                    var occYMap = cell.Y + occY;
                    //TODO: take into account rotation
                    occupiedCellsByBuilding.Add(new Vector2I(occXMap, occYMap));

                    this.occupiedCells[occXMap, occYMap] = buildingId;
                }
            }

            return occupiedCellsByBuilding;
        }

        private void CycleRotation()
        {
            switch (buildingRotation)
            {
                case BuildingRotation.Top:
                    buildingRotation = BuildingRotation.Right;
                    break;
                case BuildingRotation.Right:
                    buildingRotation = BuildingRotation.Bottom;
                    break;
                case BuildingRotation.Bottom:
                    buildingRotation = BuildingRotation.Left;
                    break;
                case BuildingRotation.Left:
                    buildingRotation = BuildingRotation.Top;
                    break;

            }
        }

        private void ApplyBuildingRotation(Node3D buildingInstance, BuildingRotation rotation)
        {
            switch (rotation)
            {
                case BuildingRotation.Top:
                    //buildingInstance.RotateY(0);
                    buildingInstance.RotationDegrees = new Vector3(0, 0, 0);
                    break;
                case BuildingRotation.Right:
                    //buildingInstance.RotateY(90);
                    buildingInstance.RotationDegrees = new Vector3(0, -90, 0);
                    break;
                case BuildingRotation.Bottom:
                    //buildingInstance.RotateY(180);
                    buildingInstance.RotationDegrees = new Vector3(0, -180, 0);
                    break;
                case BuildingRotation.Left:
                    //buildingInstance.RotateY(270);
                    buildingInstance.RotationDegrees = new Vector3(0, -270, 0);
                    break;
            }
        }

        #endregion
    }
}
