using GameTemplate.scripts.map;
using Godot;
using Microsoft.VisualBasic;
using SacaSimulationGame.scripts;
using SacaSimulationGame.scripts.buildings;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

public partial class BuildingManager : Node3D
{
    // Called when the node enters the scene tree for the first time.
    private WorldMapManager MapManager { get; set; }
    private Camera3D Camera { get; set; }

    public override void _Ready()
    {
        this.MapManager = GetParent().GetNode<WorldMapManager>("MapManager");
        this.Camera = GetViewport().GetCamera3D();
        // 1. 
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        GetHoveredCell();
    }

    private void GetHoveredCell()
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
                    if (colliderNode.Name == MapManager.Terrain.Name)
                    {
                        var cell = MapManager.WorldToCell(position);
                        var cellWorldPos = MapManager.CellToWorld(cell);
                        if (MapManager.MapData.TryGetValue(cell, out MapDataItem data))
                        {
                            var building = new Road();

                            Color color;
                            if (data.CellType == CellType.WATER)
                            {
                                color = new Color(0, 0, 1);
                            }
                            else if (data.Slope <= building.MaxSlopeAngle) color = new Color(0, 1, 0);
                            else color = new Color(1, 0, 0);

                            var worldPos3d = new Vector3(cellWorldPos.X, data.Height+0.2f, cellWorldPos.Y);
                            VisualiseHover(worldPos3d, color, shape: building.Shape, this.MapManager.CellSize);
                        }
                    }
                }
            }       
        }

    }
    private void VisualiseHover(Vector3 worldPos, Color color, Vector2 shape, Vector2 cellSize)
    {
        var meshInstance = new MeshInstance3D();
        var planeMesh = new PlaneMesh
        {
            Size = new Vector2(cellSize.X * shape.X, cellSize.Y * shape.X)
        };
        meshInstance.Mesh = planeMesh;

        var material = new StandardMaterial3D
        {
            AlbedoColor = color,
            Transparency = BaseMaterial3D.TransparencyEnum.Alpha,
            ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded,
            CullMode = BaseMaterial3D.CullModeEnum.Disabled
        };
        meshInstance.SetSurfaceOverrideMaterial(0, material);

        AddChild(meshInstance);
        meshInstance.Position = worldPos;
    }
}
