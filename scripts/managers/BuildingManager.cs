using Godot;
using SacaSimulationGame.scripts;
using SacaSimulationGame.scripts.buildings;
using SacaSimulationGame.scripts.map;
using System.Collections.Generic;
namespace SacaSimulationGame.scripts.managers
{
    public partial class BuildingManager : Node3D
    {
        // Called when the node enters the scene tree for the first time.
        private WorldMapManager MapManager { get; set; }
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

            // Apply rotation
            ApplyBuildingRotation(buildingInstance, rotation);

            // Add the building to the scene
            AddChild(buildingInstance);
            buildingInstance.GlobalPosition = worldPosition;
            buildingInstance.Scale = MapManager.CellSize;
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




    }
}
