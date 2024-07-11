using Godot;
using SacaSimulationGame.scripts;
using SacaSimulationGame.scripts.buildings;
using SacaSimulationGame.scripts.map;
using System.Collections.Generic;
namespace SacaSimulationGame.scripts.map
{
    public partial class BuildingManager : Node3D
    {
        // Called when the node enters the scene tree for the first time.
        private IWorldMapManager MapManager { get; set; }
        private Camera3D Camera { get; set; }

        private readonly Vector2I _defaultVec = new(int.MinValue, int.MinValue);


        [Export]
        public PackedScene HouseBuilding { get; set; }
        [Export]
        public PackedScene RoadBuilding { get; set; }

        public override void _Ready()
        {
            this.MapManager = GetParent().GetNode<WorldMapManager>("MapManager");
            this.Camera = GetViewport().GetCamera3D();
        }

        private Building selectedBuilding = null;
        // Called every frame. 'delta' is the elapsed time since the previous frame.
        private Vector2I lastHoveredCell = default;
        private BuildingRotation buildingRotation = BuildingRotation.Top;

        private double timeElapsedSinceLastHoverUpdate = 0;
        private double hoverIndicatorUpdateInterval = 1 / 60;

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

        public override void _Process(double delta)
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

        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("Building Slot 1"))
            {
                selectedBuilding = new House();
            }
            else if (@event.IsActionPressed("Building Slot 2"))
            {
                selectedBuilding = new Road();
            }
            else if (@event.IsActionPressed("Building Slot 3"))
            {
                selectedBuilding = new FishingPost();
            }
            else if (@event.IsActionPressed("Building Slot 4"))
            {
                selectedBuilding = new House();
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
                    GD.Print("check building buildable");
                    if (CheckBuildingBuildable(lastHoveredCell, selectedBuilding))
                    {
                        // Build building
                        GD.Print("start building");
                        if(BuildBuilding(lastHoveredCell, selectedBuilding, buildingRotation))
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

        public bool BuildBuilding(Vector2I cell, Building building, BuildingRotation rotation)
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
            Node3D buildingInstance = scene.Instantiate<Node3D>();

            if (buildingInstance == null)
            {
                GD.PrintErr("Failed to instantiate building scene");
                return false;
            }

            // Set the position
            Vector3 worldPosition = MapManager.CellToWorld(cell, height: MapManager.GetCell(cell).Height + 0.15f, centered: true);

            buildingInstance.GlobalPosition = worldPosition;
            buildingInstance.Scale = MapManager.CellSize;

            // Apply rotation
            ApplyBuildingRotation(buildingInstance, rotation);

            // Add the building to the scene
            AddChild(buildingInstance);

            // You might want to store the building instance in a data structure for later reference
            // For example: buildings[cell] = buildingInstance;
            GD.Print($"Building building at coordinate {worldPosition}");
            return true;
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

        private bool CheckBuildingBuildable(Vector2I cell, Building building, bool visualiseHover = false)
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

                        if(visualiseHover) VisualizeCell(x, y, cellShifted, data, cellBuildable, shapeHeight);
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
            Vector3 worldPos3d = MapManager.CellToWorld(cellShifted, data.Height + 2.6f);

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
            meshInstance.Position = worldPos;
        }
        #endregion
    }
}
