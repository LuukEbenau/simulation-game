using Godot;
using SacaSimulationGame.scripts;
using SacaSimulationGame.scripts.buildings;
using SacaSimulationGame.scripts.buildings.dataObjects;
using SacaSimulationGame.scripts.map;
using System.Collections.Generic;
namespace SacaSimulationGame.scripts.managers
{
    public partial class BuildingManager : Node3D
    {
        private readonly Vector2I _defaultVec = new(int.MinValue, int.MinValue);

        [Export]
        public PackedScene HouseBuilding { get; set; }
        [Export]
        public PackedScene RoadBuilding { get; set; }

        private WorldMapManager MapManager { get; set; }
        private Camera3D Camera { get; set; }
        private BuildingBlueprintBase selectedBuilding = null;
        
        private Dictionary<Player, List<BuildingDataObject>> buildingData;

        public List<BuildingDataObject> GetBuildings()
        {
            return buildingData.GetValueOrDefault(dummyPlayer);
        }

        public int[,] occupiedCells;

        public Player dummyPlayer { get; private set; }
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

        private void ChangeBuildingSelection(BuildingBlueprintBase blueprint)
        {
            this.selectedBuilding = blueprint;
        }

        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("Building Slot 1"))
            {
                ChangeBuildingSelection(new HouseBlueprint());
            }
            else if (@event.IsActionPressed("Building Slot 2"))
            {
                ChangeBuildingSelection(new RoadBlueprint());
            }
            else if (@event.IsActionPressed("Building Slot 3"))
            {
                ChangeBuildingSelection(new FishingPostBlueprint());
            }
            else if (@event.IsActionPressed("Building Slot 4"))
            {
                ChangeBuildingSelection(new HouseBlueprint());
            }
            else if (@event.IsActionPressed("Cancel Selection"))
            {
                selectedBuilding = null;
                ClearHoverIndicator();
            }

            if(@event.IsActionPressed("Rotate Building"))
            {
                GD.Print("rotating building");
                if(selectedBuilding != null)
                {
                    CycleRotation();
                }
                
            }

            if (selectedBuilding != null && lastHoveredCell != default)
            {
                if (@event.IsActionPressed("Build"))
                {
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
        public bool BuildBuilding(Vector2I cell, BuildingBlueprintBase buildingBlueprint)
        {
            if (MapManager.GetCell(cell).CellType != CellType.GROUND) {
                GD.Print("Tried to place building on terrain different than ground");
                return false; 
            }
            // Instantiate the scene
            PackedScene scene;
            if(buildingBlueprint is HouseBlueprint)
            {
                scene = this.HouseBuilding;
            }
            else if(buildingBlueprint is RoadBlueprint)
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

            buildingInstance.Cell = cell;
            buildingInstance.Blueprint = buildingBlueprint;

            Vector3 worldPosition = MapManager.CellToWorld(cell, height: MapManager.GetCell(cell).Height + 0.01f, centered: true);
            ApplyBuildingRotation(buildingInstance, buildingBlueprint.Rotation);

            var buildingDataObject = new BuildingDataObject(dummyPlayer, buildingInstance);
            buildingDataObject.OccupiedCells = CalculateOccupiedCellsByBuilding(buildingBlueprint, cell, buildingDataObject.Id);

            if (!buildingBlueprint.RequiresBuilding) {
                buildingDataObject.Building.CompleteBuilding();
            }

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
        private List<Vector2I> CalculateOccupiedCellsByBuilding(BuildingBlueprintBase building, Vector2I cell, int buildingId) {
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
            
            switch (selectedBuilding.Rotation)
            {
                case BuildingRotation.Top:
                    selectedBuilding.Rotation = BuildingRotation.Right;
                    break;
                case BuildingRotation.Right:
                    selectedBuilding.Rotation = BuildingRotation.Bottom;
                    break;
                case BuildingRotation.Bottom:
                    selectedBuilding.Rotation = BuildingRotation.Left;
                    break;
                case BuildingRotation.Left:
                    selectedBuilding.Rotation = BuildingRotation.Top;
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
