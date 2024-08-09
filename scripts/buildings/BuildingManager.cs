using Godot;
using SacaSimulationGame.scripts.building;
using SacaSimulationGame.scripts.buildings;
using SacaSimulationGame.scripts.buildings.dataStructures.blueprints;
using SacaSimulationGame.scripts.buildings.DO;
using SacaSimulationGame.scripts.map;
using SacaSimulationGame.scripts.pathfinding;
using System.Collections.Generic;
using System.Linq;
namespace SacaSimulationGame.scripts.managers
{

    public partial class BuildingManager : Node3D, IBuildingManager
    {
        [Export] public PackedScene HouseBuilding { get; set; }
        [Export] public PackedScene RoadBuilding { get; set; }
        [Export] public PackedScene FishingpostBuilding { get; set; }
        [Export] public PackedScene StockpileBuilding { get; set; }
        [Export] public PackedScene LumberjackBuilding { get; set; }

        private readonly float _buildingHeight = 0.25f;

        public struct BuildingTypeIdPair(int id, BuildingType type)
        {
            public int Id = id;
            public BuildingType Type = type;
        }

        private readonly Vector2I _defaultVec = new(int.MinValue, int.MinValue);



        private IWorldMapManager MapManager { get; set; }
        private Camera3D Camera { get; set; }

        /// <summary>
        /// Temporary variable
        /// </summary>
        private Player dummyPlayer { get; set; }

        /// <summary>
        /// Contains all buildings for each player
        /// </summary>
        private Dictionary<Player, List<BuildingDataObject>> buildingData = [];
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
        private List<PathfindingNodeGrid> SelectionPath { get; set; } = null;


        private BuildingBlueprintBase selectedBuilding = null;
        public BuildingTypeIdPair[,] OccupiedCells { get; private set; }

        public BuildingDataObject GetBuildingAtCell(Vector2I cell)
        {
            var pair = OccupiedCells[cell.X, cell.Y];
            if (pair.Id == 0) return null;
            //TODO: this is very inefficient, probably a dictionary/hashmap will be a lot more efficient
            return GetBuildings().FirstOrDefault(b => b.Id == pair.Id);
        }

        private GameManager _gameManager;
        private GameManager GameManager { get=> _gameManager ??= GetParent<GameManager>(); }

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

            OccupiedCells = new BuildingTypeIdPair[MapManager.MapWidth, MapManager.MapHeight];
        }


        public override void _Process(double delta)
        {
            HandleHoverBehaviour(delta);
        }

        private void ChangeBuildingSelection(BuildingBlueprintBase blueprint)
        {
            this.selectedBuilding = blueprint;
            VisualizeBuildingBlueprint();
        }

        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("Action Slot 1"))
            {
                ChangeBuildingSelection(new HouseBlueprint());
            }
            else if (@event.IsActionPressed("Action Slot 2"))
            {
                ChangeBuildingSelection(new RoadBlueprint());
            }
            else if (@event.IsActionPressed("Action Slot 3"))
            {
                ChangeBuildingSelection(new FishingPostBlueprint());
            }
            else if (@event.IsActionPressed("Action Slot 4"))
            {
                ChangeBuildingSelection(new StockpileBlueprint());
            }
            else if (@event.IsActionPressed("Action Slot 5"))
            {
                ChangeBuildingSelection(new LumberjackBlueprint());
            }
            else if (@event.IsActionPressed("Cancel Selection"))
            {
                selectedBuilding = null;
                ClearHoverIndicator();
            }

            if (@event.IsActionPressed("Rotate Building"))
            {

                if (selectedBuilding != null)
                {
                    CycleRotation();
                    GD.Print($"rotating building {selectedBuilding.Rotation}, {(int)selectedBuilding.Rotation}");
                    VisualizeBuildingBlueprint();
                }
            }

            // For path/area selection
            if (@event.IsActionReleased("Build") && selectedBuilding != null && SelectionPathStart != Vector2I.MaxValue)
            {
                if (selectedBuilding.SelectionMode == SelectionMode.Path)
                {
                    if (lastHoveredCell == SelectionPathStart)
                    {
                        // just single building
                        var cells = CheckBuildingBuildable(lastHoveredCell, selectedBuilding);
                        if (cells.All(b => b.isBuildable != BuildabilityStatus.BLOCKED))
                        {
                            foreach (var cell in cells)
                            {
                                BuildBuilding(cell.cell, selectedBuilding);
                            }
                        }
                    }
                    else if (SelectionPath != null && SelectionPath.Count >= 1)
                    {
                        bool isPathBuildable = true;
                        List<PathfindingNodeGrid> pathToBuild = [];
                        foreach (var pathCell in SelectionPath)
                        {
                            var building = GetBuildingAtCell(pathCell.Cell);
                            if (building != null)
                            {
                                if (building.Instance.Type == selectedBuilding.Type)
                                {
                                    continue;//same type, we can skip it since it already exists
                                }
                            }
                            //if any of the cells in the path is obstructed, the path is not possible
                            if (!CheckBuildingBuildable(pathCell.Cell, selectedBuilding).All(b => b.isBuildable != BuildabilityStatus.BLOCKED))
                            {
                                isPathBuildable = false;
                            }
                            else
                            {
                                pathToBuild.Add(pathCell);
                            }
                        }

                        if (isPathBuildable)
                        {
                            foreach (var node in pathToBuild)
                            {
                                //TODO: smart rotations of the building
                                BuildBuilding(node.Cell, selectedBuilding);
                            }
                        }
                    }
                }

                //always reset back to max, regardless mode
                SelectionPathStart = Vector2I.MaxValue;
                SelectionPath = null;
                ClearHoverIndicator();
            }


            if (selectedBuilding != null && lastHoveredCell != default)
            {
                if (@event.IsActionPressed("Build"))
                {
                    if (CheckBuildingBuildable(lastHoveredCell, selectedBuilding).All(b => b.isBuildable != BuildabilityStatus.BLOCKED))
                    {
                        // Build building
                        if (selectedBuilding.SelectionMode == SelectionMode.Single)
                        {
                            if (BuildBuilding(lastHoveredCell, selectedBuilding))
                            {
                                ClearHoverIndicator();
                            }
                            else
                            {
                                GD.Print("building building failed");
                            }

                        }
                        else if (selectedBuilding.SelectionMode == SelectionMode.Path)
                        {
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
            if (MapManager.GetCell(cell).CellType != CellType.GROUND)
            {
                GD.Print("Tried to place building on terrain different than ground");
                return false;
            }
            // Instantiate the scene
            PackedScene scene;
            if (buildingBlueprint is HouseBlueprint)
            {
                scene = this.HouseBuilding;
            }
            else if (buildingBlueprint is RoadBlueprint)
            {
                scene = this.RoadBuilding;
            }
            else if (buildingBlueprint is FishingPostBlueprint)
            {
                scene = this.FishingpostBuilding;
            }
            else if (buildingBlueprint is StockpileBlueprint)
            {
                scene = this.StockpileBuilding;
            }
            else if (buildingBlueprint is LumberjackBlueprint)
            {
                scene = this.LumberjackBuilding;
            }
            else
            {
                throw new System.Exception($"Unknown building type {buildingBlueprint}");
            }

            BuildingBase buildingInstance = scene.Instantiate<BuildingBase>();

            if (buildingInstance == null)
            {
                GD.PrintErr("Failed to instantiate building scene");
                return false;
            }

            buildingInstance.Cell = cell;
            buildingInstance.Blueprint = buildingBlueprint;
            buildingInstance.GameManager = GameManager;
            Vector3 worldPosition = MapManager.CellToWorld(cell, height: MapManager.GetCell(cell).Height + _buildingHeight, centered: false);

            var buildingDataObject = new BuildingDataObject(dummyPlayer, buildingInstance);
            buildingDataObject.OccupiedCells = CalculateOccupiedCellsByBuilding(buildingBlueprint, cell, buildingDataObject.Id);

            this.buildingData[dummyPlayer]
                .Add(buildingDataObject);

            // Add the building to the scene
            AddChild(buildingInstance);

            buildingInstance.GlobalPosition = worldPosition;
            buildingInstance.Scale = MapManager.CellSize;
            buildingInstance.RotateBuilding(buildingBlueprint.Rotation);

            if (!buildingBlueprint.RequiresBuilding)
            {
                buildingDataObject.Instance.CompleteBuilding();
            }

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
        private List<Vector2I> CalculateOccupiedCellsByBuilding(BuildingBlueprintBase building, Vector2I cell, int buildingId)
        {
            //// KEEP TRACK OF Building
            var occupiedCellsByBuilding = new List<Vector2I>();
            for (var occX = 0; occX < building.CellConstraints.GetLength(0); occX++)
            {
                for (var occY = 0; occY < building.CellConstraints.GetLength(1); occY++)
                {
                    var offset = new Vector2I(occX, occY);
                    offset = offset.Rotate((int)building.Rotation);

                    var cellPos = cell + offset;
                    //var occXMap = cell.X + occX;
                    //var occYMap = cell.Y + occY;
                    ////TODO: take into account rotation

                    occupiedCellsByBuilding.Add(cellPos);

                    this.OccupiedCells[cellPos.X, cellPos.Y] = new BuildingTypeIdPair(buildingId, building.Type);
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

        #endregion
    }
}
