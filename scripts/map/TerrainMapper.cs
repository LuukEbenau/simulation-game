using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using GameTemplate.scripts.map;
using SacaSimulationGame.scripts.map;
using SacaSimulationGame.scripts;
using SacaSimulationGame.scripts.buildings;
using System.Reflection.Metadata.Ecma335;


public partial class TerrainMapper : Node3D
{
    private string currentMap = "scene1";
    private string MAPDATA_FILE;
    private float raycastStartHeight = 50.0f;
    private float raycastLength = 100.0f;
    public Vector2I[] directions = [
        new(1, 0), new(-1, 0), new(0, 1), new(0, -1)
    ];

    private WorldMapManager MapManager;
    public override void _Ready()
    {
        MAPDATA_FILE = $"user://{currentMap}_data.json";
        this.MapManager = GetParent<WorldMapManager>();
        
    }

    public Dictionary<Vector2I, MapDataItem> LoadMapdata(Node3D terrain, Vector2I cellSize)
    {
        if (LoadMapdataFromFile(out Dictionary<Vector2I, MapDataItem> mapData))
        {
            GD.Print("Loaded terrain gradients from file");
        }
        else
        {
            GD.Print("Calculating terrain gradients...");
            mapData = MapTerrain(terrain, cellSize);
            PrintTerrainInfo(mapData, cellSize);
            //mapData = CalculateTerrainGradients(grid, cellSize);
            SaveMapdata(mapData);
            GD.Print("Terrain gradients calculated and saved");
        }
        return mapData;
    }

    private void SaveMapdata(Dictionary<Vector2I, MapDataItem> mapData)
    {
        using var file = FileAccess.Open(MAPDATA_FILE, FileAccess.ModeFlags.Write);
        if (file != null)
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new Vector2IConverter());
            GD.Print("Converter added");
            var jsonString = JsonConvert.SerializeObject(mapData, settings);

            file.StoreString(jsonString);
        }
        else
        {
            GD.Print("Failed to save terrain gradients");
        }
    }

    private bool LoadMapdataFromFile(out Dictionary<Vector2I, MapDataItem> mapData)
    {
        if (FileAccess.FileExists(MAPDATA_FILE))
        {
            using var file = FileAccess.Open(MAPDATA_FILE, FileAccess.ModeFlags.Read);
            if (file != null)
            {
                var jsonString = file.GetAsText();
                try
                {
                    GD.Print("Converter added");

                    var settings = new JsonSerializerSettings();
                    settings.Converters.Add(new Vector2IConverter());

                    var data = JsonConvert.DeserializeObject<Dictionary<string, MapDataItem>>(jsonString, settings);

                    mapData = new();
                    foreach (var kvp in data)
                    {
                        var vector = ParseKey(kvp.Key);
                        mapData[vector] = kvp.Value;
                    }

                    return true;

                }
                catch(Exception ex)
                {
                    GD.Print($"Error happened when trying to parse json map data: {ex.Message}");
                    mapData = null;
                    return false;
                }
            }
        }

        mapData = null;
        return false;
    }

    private Vector2I ParseKey(string s)
    {
        s = s.Trim('(', ')');
        var parts = s.Split(',');
        return new Vector2I(int.Parse(parts[0]), int.Parse(parts[1]));
    }

    private float GetCellSlope(Vector2 pos, Vector2 cellSize)
    {
        var h1 = CheckHeightAtPosition(pos);
        var h2 = CheckHeightAtPosition(pos + new Vector2(cellSize.X, 0));
        var h3 = CheckHeightAtPosition(pos + new Vector2(0, cellSize.Y));
        var h4 = CheckHeightAtPosition(pos + cellSize);

        var slopeX1 = Math.Abs((h2 - h1) / cellSize.X);
        var slopeX2 = Math.Abs((h4 - h3) / cellSize.X);
        var slopeZ1 = Math.Abs((h3 - h1) / cellSize.Y);
        var slopeZ2 = Math.Abs((h4 - h2) / cellSize.Y);
        var slopeDiagonal = Math.Abs((h4 - h1) / Math.Sqrt(cellSize.X * cellSize.X + cellSize.Y * cellSize.Y));

        var maxSlope = new[] { slopeX1, slopeX2, slopeZ1, slopeZ2, slopeDiagonal }.Max();
        float angle = (float)Mathf.RadToDeg(Mathf.Atan(maxSlope));

        return angle;
    }

    private float CheckHeightAtPosition(Vector2 pos)
    {
        var spaceState = GetWorld3D().DirectSpaceState;
        var start = new Vector3(pos.X, raycastStartHeight, pos.Y);
        var end = start + Vector3.Down * raycastLength;
        
        var query = new PhysicsRayQueryParameters3D
        {
            From = start,
            To = end,
            CollisionMask = Globals.CollisionTypeMap[CollisionType.WORLDMAPPING]
        };
        var result = spaceState.IntersectRay(query);
        if (result.Count > 0)
        {
            var intersectionPoint = (Vector3)result["position"];
            var lengthOfRay = start.DistanceTo(intersectionPoint);
            return raycastStartHeight - lengthOfRay;
        }
        return -1;
    }

    private Dictionary<Vector2I, MapDataItem> MapTerrain(Node3D terrain, Vector2I cellSize)
    {
        var toCheck = new Queue<Vector2I>();
        var mappedCells = new Dictionary<Vector2I, MapDataItem>();

        //var center = new Vector2I((int)terrain.GlobalPosition.X, (int)terrain.GlobalPosition.Z);

        var center = new Vector2I(0, 0);


        toCheck.Enqueue(center);

        while (toCheck.Count > 0)
        {
            var currentCell = toCheck.Dequeue();
            var currentGlobalPos = MapManager.CellToWorld(currentCell);

            if (!mappedCells.ContainsKey(currentCell))
            {
                var (cellType, height) = CheckCellTypeAndCellInsideGrid(currentGlobalPos);
                if (cellType != CellType.NONE)
                {
                    var slope = GetCellSlope(currentGlobalPos, cellSize);
                    mappedCells[currentCell] = new MapDataItem {
                        Slope = slope,
                        CellType = cellType,
                        Height = height
                    };

                    foreach (var dir in directions)
                    {
                        toCheck.Enqueue(currentCell + dir);
                    }
                }
            }
        }

        GD.Print($"Mapping complete. Total cells mapped: {mappedCells.Count}");
        return mappedCells;
    }

    private (CellType type, float height) CheckCellTypeAndCellInsideGrid(Vector2 worldPos)
    {
        var spaceState = GetWorld3D().DirectSpaceState;
        var start = new Vector3(worldPos.X, raycastStartHeight, worldPos.Y);
        var end = start + Vector3.Down * raycastLength;

        var query = new PhysicsRayQueryParameters3D
        {
            From = start,
            To = end,
            CollisionMask = Globals.CollisionTypeMap[CollisionType.WORLDMAPPING],
            CollideWithAreas = true,
            CollideWithBodies = true,

        };
        var result = spaceState.IntersectRay(query);

        if(result.Count > 0)
        {
            if (!result.TryGetValue("collider", out var collider))
            {
                GD.Print("collider not found on raycast result");
            }
            if (!result.TryGetValue("position", out var _position))
            {
                GD.Print("position not found on raycast result");
            }
            Vector3 position = _position.AsVector3();

            if (collider.AsGodotObject() != null)
            {
                if (collider.AsGodotObject() is Node3D colliderNode)
                {
                    if (colliderNode.Name == MapManager.Terrain.Name)
                    {
                        return (CellType.GROUND, position.Y);
                    }
                }
                else if(collider.AsGodotObject() is StaticBody3D staticBody)
                {
                    if (staticBody.Name == MapManager.RiverCollider.Name)
                    {
                        GD.Print("river");
                        return (CellType.WATER, position.Y);
                    }

                }
            }
            GD.Print($"unknown collider name: {collider}");
        }
        
        return (CellType.NONE, default);
    }

    private Vector2 GetTerrainSize(Dictionary<Vector2I, MapDataItem> grid, Vector2I cellSize)
    {
        var minX = float.PositiveInfinity;
        var maxX = float.NegativeInfinity;
        var minZ = float.PositiveInfinity;
        var maxZ = float.NegativeInfinity;

        foreach (var cell in grid.Keys)
        {
            minX = Math.Min(minX, cell.X);
            maxX = Math.Max(maxX, cell.X);
            minZ = Math.Min(minZ, cell.Y);
            maxZ = Math.Max(maxZ, cell.Y);
        }

        return new Vector2(maxX - minX, maxZ - minZ) * cellSize;
    }

    private void PrintTerrainInfo(Dictionary<Vector2I, MapDataItem> grid, Vector2I cellSize)
    {
        var size = GetTerrainSize(grid, cellSize);
        GD.Print($"Approximate terrain size: {size}");
        GD.Print($"Total cells: {grid.Count}");
        GD.Print($"Approximate area: {size.X * size.Y} square units");
    }
}