using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using GameTemplate.scripts.map;

//namespace GameTemplate.scripts.map
//{
    public partial class TerrainMapper : Node3D
    {
        private string currentMap = "scene1";
        private string MAPDATA_FILE;
        private float raycastStartHeight = 50.0f;
        private float raycastLength = 100.0f;
        private Vector2I[] directions = new Vector2I[]
        {
        new Vector2I(1, 0), new Vector2I(-1, 0), new Vector2I(0, 1), new Vector2I(0, -1)
        };

        public override void _Ready()
        {
            MAPDATA_FILE = $"user://{currentMap}_data.json";
        }

        public Dictionary<Vector2I, MapDataItem> LoadMapdata(Node3D terrain, Vector2I cellSize)
        {
            Dictionary<Vector2I, MapDataItem> mapData;
            if (LoadMapdataFromFile(out mapData))
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
                var jsonString = JsonConvert.SerializeObject(mapData);

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
                        var data = JsonConvert.DeserializeObject<Dictionary<Vector2I, MapDataItem>>(jsonString);

                        mapData = data;
                        //foreach (var key in data.Keys)
                        //{
                        //    var keyVec = ParseVector2I(key.ToString());
                        //    mapData[keyVec] = data[key];
                        //}
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

        private Dictionary<Vector2I, float> CalculateTerrainGradients(Dictionary<Vector2I, MapDataItem> grid, Vector2I cellSize)
        {
            var slopeGradients = new Dictionary<Vector2I, float>();
            foreach (var cell in grid.Keys)
            {
                var slope = GetCellSlope(cell, cellSize);
                slopeGradients[cell] = slope;
            }
            return slopeGradients;
        }

        private float GetCellSlope(Vector2I pos, Vector2I cellSize)
        {
            var h1 = CheckHeightAtPosition(pos);
            var h2 = CheckHeightAtPosition(pos + new Vector2I(cellSize.X, 0));
            var h3 = CheckHeightAtPosition(pos + new Vector2I(0, cellSize.Y));
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

        private float CheckHeightAtPosition(Vector2I pos)
        {
            var spaceState = GetWorld3D().DirectSpaceState;
            var start = new Vector3(pos.X, raycastStartHeight, pos.Y);
            var end = start + Vector3.Down * raycastLength;
            var query = PhysicsRayQueryParameters3D.Create(start, end);
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

            var center = new Vector2I((int)terrain.GlobalTransform.Origin.X, (int)terrain.GlobalTransform.Origin.Z);
            toCheck.Enqueue(center);

            while (toCheck.Count > 0)
            {
                var current = toCheck.Dequeue();
                if (!mappedCells.ContainsKey(current))
                {
                    if (CheckCellInGrid(current))
                    {
                        var slope = GetCellSlope(current, cellSize);
                        mappedCells[current] = new MapDataItem {
                            Slope=slope
                        };

                        foreach (var dir in directions)
                        {
                            toCheck.Enqueue(current + dir * cellSize);
                        }
                    }
                }
            }

            GD.Print($"Mapping complete. Total cells mapped: {mappedCells.Count}");
            return mappedCells;
        }

        private bool CheckCellInGrid(Vector2I cell)
        {
            var spaceState = GetWorld3D().DirectSpaceState;
            var start = new Vector3(cell.X, raycastStartHeight, cell.Y);
            var end = start + Vector3.Down * raycastLength;

            var query = PhysicsRayQueryParameters3D.Create(start, end);
            var result = spaceState.IntersectRay(query);

            return result.Count > 0;
        }

        private Vector2I ParseVector2I(string key)
        {
            var cleanedKey = key.Replace("(", "").Replace(")", "");
            var components = cleanedKey.Split(',');
            if (components.Length == 2)
            {
                var x = int.Parse(components[0]);
                var y = int.Parse(components[1]);
                return new Vector2I(x, y);
            }
            return new Vector2I();
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
//}
