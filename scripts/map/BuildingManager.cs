using GameTemplate.scripts.map;
using Godot;
using Microsoft.VisualBasic;
using SacaSimulationGame.scripts.buildings;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

public partial class BuildingManager : Node3D
{
    // Called when the node enters the scene tree for the first time.
    private WorldMapManager MapManager { get; set; }
    private Camera3D Camera { get; set; }

    private uint _collisionMaskLevel = 2;

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
            CollisionMask = _collisionMaskLevel
        };

        var result = spaceState.IntersectRay(query);

        if(result != null && result.Count > 0)
        {
            GD.Print("Hit something");

            if (!result.TryGetValue("collider_id", out var collider_id))
            {
                GD.Print("collider_id not found on raycast result");
            }

            if (!result.TryGetValue("position", out var _position))
            {
                GD.Print("position not found on raycast result");
            }
            Vector3 position = _position.AsVector3();

            if (!result.TryGetValue("normal", out var normal))
            {
                GD.Print("normal not found on raycast result");
            }

            if (!result.TryGetValue("face_index", out var face_index))
            {
                GD.Print("face_index not found on raycast result");
            }

            if (!result.TryGetValue("shape", out var shape))
            {
                GD.Print("shape not found on raycast result");
            }

            if (!result.TryGetValue("rid", out var rid))
            {
                GD.Print("rid not found on raycast result");
            }

            if (!result.TryGetValue("collider", out var collider))
            {
                GD.Print("collider not found on raycast result");
            }

            if (collider.AsGodotObject() != null && collider.AsGodotObject() is Node3D colliderNode) {
                if(colliderNode.Name == MapManager.Terrain.Name)
                {
                    var cell = MapManager.WorldToCell(position);
                    if(MapManager.MapData.TryGetValue(cell, out MapDataItem data))
                    {
                        var building = new House();
                        //var overlayShape = new Rect2(cell, building.Shape);

                        Color color;
                        if (data.Slope <= building.MaxSlopeAngle) color = new Color(0, 1, 0);
                        else color = new Color(1, 0, 0);

                        VisualiseHover(cell, color, building.Shape);
                    }
                }
            }
           
        }

    }
    private void VisualiseHover(Vector2I cell, Color color, Vector2 cellSize)
    {
        var meshInstance = new MeshInstance3D();
        var planeMesh = new PlaneMesh();
        planeMesh.Size = new Vector2(cellSize.X, cellSize.Y);
        meshInstance.Mesh = planeMesh;

        var material = new StandardMaterial3D();
        material.AlbedoColor = color;
        material.Transparency = BaseMaterial3D.TransparencyEnum.Alpha;
        material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;
        material.CullMode = BaseMaterial3D.CullModeEnum.Disabled;
        meshInstance.SetSurfaceOverrideMaterial(0, material);

        meshInstance.Position = new Vector3(cell.X * cellSize.X, 2.2f, cell.Y * cellSize.Y);
        AddChild(meshInstance);
    }
}
