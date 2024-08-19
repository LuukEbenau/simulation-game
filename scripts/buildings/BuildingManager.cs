using Godot;
using SacaSimulationGame.scripts.building;
using SacaSimulationGame.scripts.buildings;
using SacaSimulationGame.scripts.buildings.dataStructures.blueprints;
using SacaSimulationGame.scripts.buildings.DO;
using SacaSimulationGame.scripts.map;
using SacaSimulationGame.scripts.naturalResources;
using SacaSimulationGame.scripts.pathfinding;
using SacaSimulationGame.scripts.units.tasks;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
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
        [Export] public PackedScene BridgeBuilding { get; set; }

        private readonly float _buildingHeight = 0.25f;

        public struct BuildingTypeIdPair(int id, BuildingType type, BuildingBase building)
        {
            public int Id = id;
            public BuildingType Type = type;
            public BuildingBase Building = building;
            public bool BuildingCompleted => Building == null ? false : Building.BuildingCompleted;
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

        private void _buildBuildingPath()
        {
            if (lastHoveredCell == SelectionPathStart)
            {
                // just single building
                var cells = CheckBuildingBuildable(lastHoveredCell, selectedBuilding, visualiseHover: false);
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
                for(int i = 0; i< SelectionPath.Count; i++)
                //foreach (var pathCell in SelectionPath)
                {
                    var pathCell = SelectionPath[i];
                    bool isBase = false, isDestination = false;
                    if (i==0)
                    {
                        isBase = true;
                    }
                    if (i== SelectionPath.Count-1)
                    {
                        isDestination = true;
                    }


                    var building = GetBuildingAtCell(pathCell.Cell);
                    if (building != null)
                    {
                        if (building.Instance.Type == selectedBuilding.Type)
                        {
                            continue;//same type, we can skip it since it already exists
                        }
                        else
                        {
                            Debug.Print($"Warning: different building than current on this position, should we also skip it?");
                            continue;
                        }
                    }
                    //if any of the cells in the path is obstructed, the path is not possible
                    if (!CheckBuildingBuildable(pathCell.Cell, selectedBuilding, visualiseHover:false, isBase, isDestination).All(b => b.isBuildable != BuildabilityStatus.BLOCKED))
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
                    float baseHeight = default;
                    for(int i = 0; i < pathToBuild.Count; i++)
                    {
                        var node = pathToBuild[i];
                        //TODO: smart rotations of the building
                        bool isBase = i == 0;
                        bool isDestination = i == pathToBuild.Count - 1;
                        //float baseHeight = default;
                        if (isBase)
                        {
                            baseHeight = MapManager.GetCell(node.Cell).Height;
                        }

                        BuildBuilding(node.Cell, selectedBuilding, isBase, isDestination, baseHeight );
                    }
                }
            }
        }

        private void _buildBuildingSingle()
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
            else if (@event.IsActionPressed("Action Slot 6"))
            {
                ChangeBuildingSelection(new BridgeBlueprint());
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
                if ((SelectionMode.Path|SelectionMode.Line).HasFlag(selectedBuilding.SelectionMode))
                {
                    _buildBuildingPath();
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
                    if (CheckBuildingBuildable(lastHoveredCell, selectedBuilding, visualiseHover:false).All(b => b.isBuildable != BuildabilityStatus.BLOCKED))
                    {
                        // Build building
                        if (selectedBuilding.SelectionMode == SelectionMode.Single)
                        {
                            _buildBuildingSingle();
                        }
                        else if ((SelectionMode.Path | SelectionMode.Line).HasFlag(selectedBuilding.SelectionMode))
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
        private PackedScene GetSceneFromBlueprint(BuildingBlueprintBase buildingBlueprint)
        {
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
            else if (buildingBlueprint is BridgeBlueprint)
            {
                scene = this.BridgeBuilding;
            }
            else
            {
                throw new System.Exception($"Unknown building type {buildingBlueprint}");
            }

            return scene;
        }

        public bool BuildBuilding(Vector2I cell, BuildingBlueprintBase buildingBlueprint, bool isBase = true, bool isDestination = true, float baseHeight = default)
        {
            if (MapManager.GetCell(cell).CellType != CellType.GROUND)
            {
                GD.Print("Tried to place building on terrain different than ground, should this be allowed? for now do nothing");
                //return false;
            }
            // Instantiate the scene
            PackedScene scene = GetSceneFromBlueprint(buildingBlueprint);

            BuildingBase buildingInstance = scene.Instantiate<BuildingBase>();

            if (buildingInstance == null)
            {
                GD.PrintErr("Failed to instantiate building scene");
                return false;
            }

            //bool isBase = false, isDestination = false;
            var constraints = buildingBlueprint.GetConstraints(0, 0, isBase, isDestination);
            var cellHeight = MapManager.GetCell(cell).Height;
            if (baseHeight == default)
            {
                baseHeight = cellHeight;
            }
            var buildingHeight = constraints.CalculateHeight(cellHeight, baseHeight);

            buildingInstance.Cell = cell;
            buildingInstance.Blueprint = buildingBlueprint;
            buildingInstance.GameManager = GameManager;
            Vector3 worldPosition = MapManager.CellToWorld(cell, height: buildingHeight + _buildingHeight, centered: false);

            var buildingDataObject = new BuildingDataObject(dummyPlayer, buildingInstance);

            // when partially buildable
            var buildabilities = CheckBuildingBuildable(cell, buildingBlueprint, visualiseHover: false, isBase, isDestination);

            var resourcesToRemoveBeforeBuilding = new List<NaturalResourceGatherTask>();

            foreach(var buildabilitypair  in buildabilities)
            {
                var buildabilityStatus = buildabilitypair.isBuildable;

                if(buildabilityStatus == BuildabilityStatus.PARTIAL)
                {
                    var resource = GameManager.NaturalResourceManager
                        .GetResourcesAtCell(buildabilitypair.cell)
                        .FirstOrDefault();
                    //NOTE: if we want to support multiple resources at the same cell, we need to not do firstOrDefault
                    if(resource != default)
                    {
                        resourcesToRemoveBeforeBuilding.Add(new NaturalResourceGatherTask(resource));
                    }
                }
            }

            buildingDataObject.OccupiedCells = CalculateOccupiedCellsByBuilding(buildingBlueprint, cell);

            foreach(var occupiedCell in buildingDataObject.OccupiedCells)
            {
                //TODO: this overwrites a potential other building in case something goes wrong in the code. Better would be to have a seperate function with build in checks
                this.OccupiedCells[occupiedCell.X, occupiedCell.Y] = new BuildingTypeIdPair(buildingDataObject.Id, buildingBlueprint.Type, buildingInstance);
            }

            this.buildingData[dummyPlayer]
                .Add(buildingDataObject);

            // Add the building to the scene
            AddChild(buildingInstance);

            buildingInstance.GlobalPosition = worldPosition;
            buildingInstance.Scale = MapManager.CellSize;

            if (buildingBlueprint is StockpileBlueprint sb)
            {
                GD.Print("building stockpile");
                if (sb.InitialResourceStored > 0)
                {
                    var stockpile = buildingInstance as Stockpile;
                    GD.Print("Storing resource");
                    var leftover = stockpile.StoredResources.AddResource(sb.InitialResourceStored, sb.InitialResourceAmount);
                    
                    if (leftover > 0)
                    {
                        GD.Print($"leftover resource which couldnt be stored: {leftover} ");
                    }
                }
            }

            buildingInstance.RotateBuilding(buildingBlueprint.Rotation);

            if (!buildingBlueprint.RequiresBuilding)
            {
                buildingDataObject.Instance.CompleteBuilding();
                //NOTE: in this case resources do not get removed, so we need to be sure theres no resources below, or we should remove the resources.
            }
            else
            {
                //enqueue a task for workers
                var buildBuildingTask = new BuildBuildingTask(buildingDataObject);

                UnitTask taskToExecute;
                if (resourcesToRemoveBeforeBuilding.Count > 0)
                {
                    var preTasks = resourcesToRemoveBeforeBuilding.Select(t => t as UnitTask).ToList();
                    taskToExecute = new CollectionTask(preTasks) { FollowUpTasks = [buildBuildingTask] };
                }
                else
                {
                    taskToExecute = buildBuildingTask;
                    
                }

                GameManager.TaskManager.EnqueueTask(taskToExecute);
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
        private List<Vector2I> CalculateOccupiedCellsByBuilding(BuildingBlueprintBase building, Vector2I cell)
        {
            //// KEEP TRACK OF Building
            var occupiedCellsByBuilding = new List<Vector2I>();
            for (var occX = 0; occX < building.CellConstraints.GetLength(0); occX++)
            {
                for (var occY = 0; occY < building.CellConstraints.GetLength(1); occY++)
                {
                    var offset = new Vector2I(occX, occY);
                    offset = offset.Rotate((int)building.Rotation); //TODO: take into account rotation

                    var cellPos = cell + offset;
                    //var occXMap = cell.X + occX;
                    //var occYMap = cell.Y + occY;
                    //

                    occupiedCellsByBuilding.Add(cellPos);
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
