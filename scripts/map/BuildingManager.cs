using GameTemplate.scripts.map;
using Godot;
using SacaSimulationGame.scripts;
using SacaSimulationGame.scripts.buildings;
using System.Collections.Generic;

public partial class BuildingManager : Node3D
{
    // Called when the node enters the scene tree for the first time.
    private WorldMapManager MapManager { get; set; }
    private Camera3D Camera { get; set; }

    private readonly Vector2I _defaultVec = new(int.MinValue, int.MinValue);

    public override void _Ready()
    {
        this.MapManager = GetParent().GetNode<WorldMapManager>("MapManager");
        this.Camera = GetViewport().GetCamera3D();
    }

    private Building selectedBuilding = null;
    public override void _Input(InputEvent @event)
    {
        if(@event.IsActionPressed("Building Slot 1"))
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
        else if(@event.IsActionPressed("Cancel Selection"))
        {
            selectedBuilding = null;
            ClearHoverIndicator();
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    private Vector2I lastHoveredCell = default;
    private double timeElapsedSinceLastHoverUpdate=0;
    private double hoverIndicatorUpdateInterval = 1/60;
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
                    ClearHoverIndicator();
                    UpdateHoverIndicator(cell, selectedBuilding);
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
        
        var query = new PhysicsRayQueryParameters3D {
            From = from,
            To = to,
            CollideWithAreas = true,
            CollideWithBodies = true,
            CollisionMask = Globals.CollisionTypeMap[CollisionType.BUILDING]
        };

        var result = spaceState.IntersectRay(query);

        if(result != null && result.Count > 0)
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

            if (collider.AsGodotObject() != null) {
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
    private MeshInstance3D GetHoverIndicator(int indicatorIndex, Vector2 cellSize)
    {
        if (indicatorIndex < this.hoverVisualiseMesh.Count)
        {
            return hoverVisualiseMesh[indicatorIndex];
        }

        var meshInstance = new MeshInstance3D();
        var planeMesh = new PlaneMesh
        {
            Size = MapManager.CellSize
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


    private void UpdateHoverIndicator(Vector2I cell, Building building)
    {
        for (int x = 0; x < building.Shape.X; x++)
        {
            for (int y = 0; y < building.Shape.Y; y++)
            {
                var meshInstanceIndex = y + x * building.Shape.Y;

                var cellShifted = cell + new Vector2I(x, y);
                if (MapManager.MapData.TryGetValue(cellShifted, out MapDataItem data))
                {
                    Color color;
                    if (data.CellType == CellType.WATER)
                    {
                        color = new Color(0, 0, 1);
                    }
                    else if (data.Slope <= building.MaxSlopeAngle) color = new Color(0, 1, 0);
                    else color = new Color(1, 0, 0);

                    var cellWorldPos = MapManager.CellToWorld(cellShifted);
                    var worldPos3d = new Vector3(cellWorldPos.X, data.Height + 0.4f, cellWorldPos.Y);

                    var hoverIndicatorMesh = GetHoverIndicator(meshInstanceIndex, MapManager.CellSize);
                    VisualiseHover(hoverIndicatorMesh, worldPos3d, color, this.MapManager.CellSize);
                }
            }
        }
    }

    private void VisualiseHover(MeshInstance3D meshInstance, Vector3 worldPos, Color color, Vector2 cellSize)
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
