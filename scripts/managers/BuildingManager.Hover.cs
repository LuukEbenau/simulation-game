using Godot;
using SacaSimulationGame.scripts;
using SacaSimulationGame.scripts.buildings.dataObjects;
using SacaSimulationGame.scripts.map;
using System.Collections.Generic;

namespace SacaSimulationGame.scripts.managers
{
    public partial class BuildingManager : Node3D
    {

        private void HandleHoverBehaviour(double delta)
        {
            if (selectedBuilding != null)
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
                        bool buildingIsBuildable = CheckBuildingBuildable(cell, selectedBuilding, visualiseHover: true);
                    }
                }
            }
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
                GD.Print("Hit something");

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

        private bool CheckBuildingBuildable(Vector2I cell, BuildingDO building, bool visualiseHover = false)
        {
            bool buildingBuildable = true;
            int shapeWidth = building.Shape.GetLength(0);
            int shapeHeight = building.Shape.GetLength(1);

            for (int x = 0; x < shapeWidth; x++)
            {
                for (int y = 0; y < shapeHeight; y++)
                {
                    var cellShifted = cell + new Vector2I(x, y);
                    if (MapManager.TryGetCell(cellShifted, out MapDataItem data))
                    {
                        bool cellBuildable = IsCellBuildable(data, building.Shape[x, y], building.MaxSlopeAngle, cellShifted);
                        buildingBuildable &= cellBuildable;

                        if (visualiseHover) VisualizeCell(x, y, cellShifted, data, cellBuildable, shapeHeight);
                    }
                }
            }
            return buildingBuildable;
        }

        private bool IsCellBuildable(MapDataItem data, CellType buildingCellType, float maxSlopeAngle, Vector2I cell)
        {
            if (data.CellType.HasFlag(CellType.WATER))
            {
                return buildingCellType.HasFlag(CellType.WATER);
            }
            else if (data.CellType.HasFlag(CellType.GROUND))
            {
                return buildingCellType.HasFlag(CellType.GROUND) && data.Slope <= maxSlopeAngle;
            }
            else
            {
                GD.Print($"Unknown cell type: {data.CellType} of cell {cell}");
                return false;
            }
        }

        private void VisualizeCell(int x, int y, Vector2I cellShifted, MapDataItem data, bool cellBuildable, int shapeHeight)
        {
            var meshInstanceIndex = y + x * shapeHeight;
            var color = cellBuildable ? new Color(0, 1, 0) : new Color(1, 0, 0);
            Vector3 worldPos3d = MapManager.CellToWorld(cellShifted, data.Height + 3f);

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
