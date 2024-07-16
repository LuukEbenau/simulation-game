using Godot;
using SacaSimulationGame.scripts;
using SacaSimulationGame.scripts.buildings;
using SacaSimulationGame.scripts.buildings.dataObjects;
using SacaSimulationGame.scripts.map;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// Temporary variable
        /// </summary>
        private Player dummyPlayer { get; set; }

        /// <summary>
        /// Contains all buildings for each player
        /// </summary>
        private Dictionary<Player, List<BuildingDataObject>> buildingData=[];
        public List<BuildingDataObject> GetBuildings()
        {
            return buildingData.GetValueOrDefault(dummyPlayer);
        }

        /// <summary>
        /// For Path Selection or area Selection buildings
        /// </summary>
        private Vector2I SelectionPathStart { get; set; } = Vector2I.MaxValue;
        /// <summary>
        /// When using selectionmode.Path, this is used to store the path
        /// </summary>
        private List<Vector2I> SelectionPath { get; set; } = null;


        private BuildingBlueprintBase selectedBuilding = null;
        public int[,] OccupiedCells { get; private set; }

        //TODO: i need to make the selectedbuilding a copy instead, otherwise they all share the same instance. Or, not use the blueprint anymore after building.
        public override void _Ready()
        {
            this.MapManager = GetParent().GetNode<WorldMapManager>("MapManager");
            if (this.MapManager.MapWidth == default)
            {
                GD.Print($"WARNING: it seems like the BuildingManager is ran before the MapManager, verify the order...");
            }
            this.Camera = GetViewport().GetCamera3D();

            dummyPlayer = new Player();
            buildingData.Add(dummyPlayer, []);

            OccupiedCells = new int[MapManager.MapWidth, MapManager.MapHeight];
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

            // For path/area selection
            if (@event.IsActionReleased("Build") && selectedBuilding != null && SelectionPathStart != Vector2I.MaxValue)
            {
                if (selectedBuilding.SelectionMode == SelectionMode.Path)
                {
                    if(lastHoveredCell == SelectionPathStart)
                    {
                        // just single building
                        var cells = CheckBuildingBuildable(lastHoveredCell, selectedBuilding);
                        if (cells.All(b=>b.isBuildable))
                        {
                            foreach (var cell in cells)
                            {
                                BuildBuilding(cell.cell, selectedBuilding);
                            }
                        }
                    }
                    else if (SelectionPath != null && SelectionPath.Count >= 1)
                    {
                        bool isPathBuildable = SelectionPath.All(cell => CheckBuildingBuildable(cell, selectedBuilding).All(b=>b.isBuildable));

                        if (isPathBuildable)
                        {
                            foreach (var cell in SelectionPath)
                            {
                                //TODO: smart rotations of the building
                                BuildBuilding(cell, selectedBuilding);
                            }
                        }
                    }
                }

                //always reset back to max, regardless mode
                SelectionPathStart = Vector2I.MaxValue;
                SelectionPath = null;
            }
            

            if (selectedBuilding != null && lastHoveredCell != default)
            {
                if (@event.IsActionPressed("Build"))
                {
                    GD.Print("check building buildable");
                    if (CheckBuildingBuildable(lastHoveredCell, selectedBuilding).All(b=>b.isBuildable))
                    {
                        // Build building
                        GD.Print("start building");
                        if (selectedBuilding.SelectionMode == SelectionMode.Single)
                        {
                            if (BuildBuilding(lastHoveredCell, selectedBuilding))
                            {
                                GD.Print("building building success");
                            }
                            else
                            {
                                GD.Print("building building failed");
                            }
                        }
                        else if (selectedBuilding.SelectionMode == SelectionMode.Path) {
                            this.SelectionPathStart = lastHoveredCell;
                        }
                        else
                        {
                            throw new System.Exception($"Selection mode {selectedBuilding.SelectionMode} is not implemented yet");
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

            Vector3 worldPosition = MapManager.CellToWorld(cell, height: MapManager.GetCell(cell).Height + 0.25f, centered: true);
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

                    this.OccupiedCells[occXMap, occYMap] = buildingId;
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
