using Godot;
using SacaSimulationGame.scripts.buildings;
using SacaSimulationGame.scripts.buildings.dataStructures.blueprints;
using SacaSimulationGame.scripts.map;
using SacaSimulationGame.scripts.pathfinding;
using System;
using System.Collections.Generic;
using System.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SacaSimulationGame.scripts.managers
{
    public partial class BuildingManager : Node3D
    {
        private bool buildingSelectionStarted = false;
        private double timeElapsedSinceLastHoverUpdate = 0;
        private double hoverIndicatorUpdateInterval = 1 / 60;
        private Vector2I lastHoveredCell = default;


        private void _HandlePathHover(double delta)
        {
            if (SelectionPathStart != Vector2I.MaxValue)
            {
                //Find path with astar from start to destination, in found use this path to visualise
                if (SelectionPathStart == lastHoveredCell)
                {
                    SelectionPath = [new PathfindingNodeGrid(lastHoveredCell, 1.0f)]; // also allow for just building a single cell
                }
                else
                {
                    var cellTypesAllowed = selectedBuilding.GetConstraints(0, 0).CellTypes;//.CellConstraints[0, 0].CellTypes;
                    //TODO: to prevent future problems it would be better to also implement the base and destionation cell constraints here


                    //TODO: make this dynamic, so that for example the base can be different celltype
                    SelectionPath = this.MapManager.Pathfinder.FindPath(SelectionPathStart, lastHoveredCell,
                        traversableTerrainType: cellTypesAllowed,
                        obstacleBuildings: BuildingType.ObstacleBuildings | BuildingType.Stockpile,
                        maxIterationCount: 400);
                }
                var cellsToVisualise = new List<(Vector2I cell, MapDataItem cellData, BuildabilityStatus isBuildable)>();
                for(int i = 0; i < SelectionPath.Count;i++)
                //foreach (var pathCell in SelectionPath)
                {
                    var pathCell = SelectionPath[i];
                    bool isBase = false, isDestination = false;
                    if (i==0)
                    {
                        isBase = true;
                    }
                    if (i==SelectionPath.Count-1)
                    {
                        isDestination = true;
                    }
                    // If type of building is the same, and selectionType is not Single, instead of coloring red we are just going to skip the building
                    //if(pathCell.)
                    var building = GetBuildingAtCell(pathCell.Cell);
                    if (building != null)
                    {
                        if (building.Instance.Type == selectedBuilding.Type)
                        {
                            //same type, we can skip it
                            continue;
                        }
                    }
                    cellsToVisualise.AddRange(CheckBuildingBuildable(pathCell.Cell, selectedBuilding, visualiseHover: true, isBase:isBase, isDestination:isDestination));
                }

                VisualiseBuildableCells(cellsToVisualise);

            }
            else
            {
                VisualiseBuildableCells(CheckBuildingBuildable(lastHoveredCell, selectedBuilding, visualiseHover: true));
            }
        }
        private void _HandleLineHover(double delta)
        {
            if (SelectionPathStart != Vector2I.MaxValue)
            {
                //Find path with astar from start to destination, in found use this path to visualise
                if (SelectionPathStart == lastHoveredCell)
                {
                    SelectionPath = [new PathfindingNodeGrid(lastHoveredCell, 1.0f)]; // also allow for just building a single cell
                }
                else
                {
                    //var cellTypesAllowed = selectedBuilding.GetConstraints(0, 0).CellTypes;//.CellConstraints[0, 0].CellTypes;

                    var diff = lastHoveredCell - SelectionPathStart;

                    Vector2I lineEnd;
                    bool xLarger;
                    if(Mathf.Abs(diff.X) > Mathf.Abs(diff.Y))
                    {
                        // x is largest, so draw line to X
                        lineEnd = SelectionPathStart + new Vector2I(diff.X, 0);
                        xLarger = true;
                    }
                    else
                    {
                        //y is largest, so draw line to y
                        lineEnd = SelectionPathStart + new Vector2I(0, diff.Y);
                        xLarger = false;
                    }

                    List<Vector2I> line = new List<Vector2I>();
                    Vector2I current = SelectionPathStart;
                    Vector2I step = new Vector2I(
                        Math.Sign(lineEnd.X - SelectionPathStart.X),
                        Math.Sign(lineEnd.Y - SelectionPathStart.Y)
                    );

                    while (current != lineEnd)
                    {
                        line.Add(current);
                        if (xLarger)
                        {
                            current.X += step.X;
                            if (current.X == lineEnd.X)
                            {
                                while (current.Y != lineEnd.Y)
                                {
                                    current.Y += step.Y;
                                    line.Add(current);
                                }
                            }
                        }
                        else
                        {
                            current.Y += step.Y;
                            if (current.Y == lineEnd.Y)
                            {
                                while (current.X != lineEnd.X)
                                {
                                    current.X += step.X;
                                    line.Add(current);
                                }
                            }
                        }
                    }
                    line.Add(lineEnd);

                    // Convert Vector2I to PathfindingNodeGrid if needed
                    SelectionPath = line.Select(pos => new PathfindingNodeGrid(pos,1.0f)).ToList();
                }
                
                var cellsToVisualise = new List<(Vector2I cell, MapDataItem cellData, BuildabilityStatus isBuildable)>();
                for(int i = 0; i< SelectionPath.Count; i++)
                //foreach (var pathCell in SelectionPath)
                {
                    var pathCell = SelectionPath[i];
                    bool isBase = false, isDestination = false;
                    if (i == 0)
                    {
                        isBase = true;
                    }
                    if (i == SelectionPath.Count - 1)
                    {
                        isDestination = true;
                    }
                    // If type of building is the same, and selectionType is not Single, instead of coloring red we are just going to skip the building
                    //if(pathCell.)
                    var building = GetBuildingAtCell(pathCell.Cell);
                    if (building != null)
                    {
                        if (building.Instance.Type == selectedBuilding.Type)
                        {
                            //same type, we can skip it
                            continue;
                        }
                    }
                    cellsToVisualise.AddRange(CheckBuildingBuildable(pathCell.Cell, selectedBuilding, visualiseHover: true, isBase: isBase, isDestination: isDestination));
                }

                VisualiseBuildableCells(cellsToVisualise);

            }
            else
            {
                VisualiseBuildableCells(CheckBuildingBuildable(lastHoveredCell, selectedBuilding, visualiseHover: true));
            }
        }



        private void HandleHoverBehaviour(double delta)
        {
            if (selectedBuilding != null && !GuiIsHovered)
            {
                timeElapsedSinceLastHoverUpdate += delta;
                if (timeElapsedSinceLastHoverUpdate > hoverIndicatorUpdateInterval)
                {
                    timeElapsedSinceLastHoverUpdate = 0;
                    var cell = GetHoveredCell();
                    if (cell == _defaultVec)
                    {
                        ClearHoverIndicator();
                    }
                    else if (lastHoveredCell == cell)
                    {
                        // nothing changes, hover stays
                    }
                    else
                    {
                        lastHoveredCell = cell;
                        ClearHoverIndicator();

                        if (selectedBuilding.SelectionMode == SelectionMode.Path)
                        {
                            _HandlePathHover(delta);
                        }
                        else if( selectedBuilding.SelectionMode == SelectionMode.Line)
                        {
                            _HandleLineHover(delta);
                        }
                        else if (selectedBuilding.SelectionMode == SelectionMode.Single)
                        {
                            VisualizeBuildingBlueprint();
                        }
                        else
                        {
                            throw new Exception($"Selectionmode {selectedBuilding.SelectionMode} not implemented");
                        }
                    }
                }
            }
        }

        private void VisualizeBuildingBlueprint()
        {
            ClearHoverIndicator();
            VisualiseBuildableCells(CheckBuildingBuildable(lastHoveredCell, selectedBuilding, visualiseHover: true));
        }

        private void VisualiseBuildableCells(List<(Vector2I cell, MapDataItem cellData, BuildabilityStatus isBuildable)> celldata)
        {
            for(int i = 0; i < celldata.Count;i++)
            {
                var c = celldata[i];
                VisualizeCell(i, c.cell, c.cellData, c.isBuildable);
            }
        }

        private List<(Vector2I cell, MapDataItem cellData, BuildabilityStatus isBuildable)> CheckBuildingBuildable(Vector2I cell, BuildingBlueprintBase blueprint, bool visualiseHover, bool isBase = true, bool isDestination = true)
        {
            bool buildingBuildable = true;
            int shapeWidth = blueprint.CellConstraints.GetLength(0);
            int shapeLength = blueprint.CellConstraints.GetLength(1);

            List<(Vector2I cell, MapDataItem cellData, BuildabilityStatus isBuildable)> celldata = [];

            MapDataItem baseCellData = null;
            for (int x = 0; x < shapeWidth; x++)
            {
                for (int y = 0; y < shapeLength; y++)
                {
                    var vecRotated = new Vector2I(x, y).Rotate((int)blueprint.Rotation);
                    var cellShifted = cell + vecRotated;
                    if (MapManager.TryGetCell(cellShifted, out MapDataItem data))
                    {
                        
                        if(x==0 && y == 0)
                        {
                            baseCellData = data;
                        }

                        BuildabilityStatus cellBuildable = IsCellBuildable(data, blueprint, x, y, cellShifted, baseCellData, isBase, isDestination);
                        buildingBuildable &= cellBuildable != BuildabilityStatus.BLOCKED;

                        celldata.Add((cellShifted,data,cellBuildable));

                        if (visualiseHover) {
                            var meshInstanceIndex = y + x * shapeLength; //get the index based on the x+y
                            VisualizeCell(meshInstanceIndex, cellShifted, data, cellBuildable); 
                        }
                    }
                }
            }
            return celldata;
        }

        private enum BuildabilityStatus
        {
            FREE = 1,
            PARTIAL = 2,
            BLOCKED = 3
        }

        private BuildabilityStatus IsCellBuildable(MapDataItem data, BuildingBlueprintBase blueprint, int shapeX, int shapeY, Vector2I cell, MapDataItem baseCellData, bool isBase, bool isDestination)
        {
            var buildingContraints = blueprint.GetConstraints(shapeX, shapeY, isBase, isDestination);

            // check for obstacles
            var cellOccupation = MapManager.GetCellOccupation(cell);    
            if (cellOccupation.Building)
            {
                return BuildabilityStatus.BLOCKED;
            }

            //check for slope constraint
            if (buildingContraints.MaxSlope != default)
            {
                if(data.Slope > buildingContraints.MaxSlope)
                {
                    return BuildabilityStatus.BLOCKED;
                }
            }
            // check for terrain type constraint
            if(buildingContraints.CellTypes != default)
            {
                if (!buildingContraints.CellTypes.HasFlag(data.CellType))
                {
                    return BuildabilityStatus.BLOCKED;
                }
            }

            if (buildingContraints.ElevationConstraint != default)
            {
                if(!buildingContraints.ElevationConstraint(baseCellData.Height, data.Height))
                {
                    return BuildabilityStatus.BLOCKED;
                }
            }

            // If natural resource, its partially free
            if (cellOccupation.NaturalResource)
            {
                return BuildabilityStatus.PARTIAL;
            }

            return BuildabilityStatus.FREE;
        }
        private Vector2I GetHoveredCell()
        {
            var mousePos = GetViewport().GetMousePosition();
            var from = this.Camera.ProjectRayOrigin(mousePos);
            var to = from + Camera.ProjectRayNormal(mousePos) * 500;

            var spaceState = GetWorld3D().DirectSpaceState;

            var query = new PhysicsRayQueryParameters3D
            {
                From = from,
                To = to,
                CollideWithAreas = true,
                CollideWithBodies = true,
                CollisionMask = Globals.CollisionTypeMap[CollisionType.BUILDING]
            };

            var result = spaceState.IntersectRay(query);

            if (result != null && result.Count > 0)
            {
                if (!result.TryGetValue("position", out var _position))
                {
                    GD.Print("position not found on raycast result");
                }
                Vector3 position = _position.AsVector3();

                if (!result.TryGetValue("collider", out var collider))
                {
                    GD.Print("collider not found on raycast result");
                }

                if (collider.AsGodotObject() != null)
                {
                    if (collider.AsGodotObject() is Node3D colliderNode)
                    {
                        var cell = MapManager.WorldToCell(position);

                        return cell;

                    }
                }
            }
            return _defaultVec;
        }

        #region visualisation of hover
        private readonly List<MeshInstance3D> hoverVisualiseMesh = [];
        private MeshInstance3D GetHoverIndicator(int indicatorIndex, Vector3I cellSize)
        {
            if (indicatorIndex < this.hoverVisualiseMesh.Count)
            {
                return hoverVisualiseMesh[indicatorIndex];
            }

            var meshInstance = new MeshInstance3D();
            var planeMesh = new PlaneMesh
            {
                Size = cellSize.ToVec2World()
            };
            meshInstance.Mesh = planeMesh;

            AddChild(meshInstance);

            hoverVisualiseMesh.Add(meshInstance);

            return meshInstance;
        }

        private void ClearHoverIndicator()
        {
            foreach (var indicator in this.hoverVisualiseMesh)
            {
                indicator.Visible = false;
            }
        }
        private void VisualizeCell(int meshInstanceIndex, Vector2I cellShifted, MapDataItem data, BuildabilityStatus cellBuildable)
        {
            Color color = cellBuildable switch
            {
                BuildabilityStatus.FREE => new Color(0, 1, 0),
                BuildabilityStatus.PARTIAL => new Color(1, 1, 0),
                BuildabilityStatus.BLOCKED => new Color(1, 0, 0),
                _ => new Color(1, 1, 1)
            };

            Vector3 worldPos3d = MapManager.CellToWorld(cellShifted, data.Height + 0.4f, centered:true);

            var hoverIndicatorMesh = GetHoverIndicator(meshInstanceIndex, MapManager.CellSize);
            VisualiseHover(hoverIndicatorMesh, worldPos3d, color);
        }

        private void VisualiseHover(MeshInstance3D meshInstance, Vector3 worldPos, Color color)
        {
            meshInstance.Visible = true;
            var material = new StandardMaterial3D
            {
                AlbedoColor = color,
                Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
                ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
                CullMode = BaseMaterial3D.CullModeEnum.Disabled
            };
            meshInstance.SetSurfaceOverrideMaterial(0, material);
            meshInstance.GlobalPosition = worldPos;
        }
        #endregion
    }
}
