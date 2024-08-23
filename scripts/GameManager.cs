using Godot;
using SacaSimulationGame.scripts.building;
using SacaSimulationGame.scripts.buildings;
using SacaSimulationGame.scripts.buildings.dataStructures.blueprints;
using SacaSimulationGame.scripts.map;
using SacaSimulationGame.scripts.naturalResources;
using SacaSimulationGame.scripts.units;
using SacaSimulationGame.scripts.units.dataObjects;
using SacaSimulationGame.scripts.units.professions.misc;
using SacaSimulationGame.scripts.units.tasks;
using System;
using System.Collections.Generic;
using System.Linq;


namespace SacaSimulationGame.scripts.managers
{
    public partial class GameManager : Node3D
    {
        public IUnitManager UnitManager { get; set; }
        public IWorldMapManager MapManager { get; set; }
        public IBuildingManager BuildingManager { get; set; }
        public ITaskManager TaskManager { get; set; }
        public INaturalResourceManager NaturalResourceManager { get; set; }
        public Node3D SpawnLocation { get; private set; }
        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
            UnitManager = GetNode<UnitManager>("UnitManager");
            MapManager = GetNode<WorldMapManager>("MapManager");
            BuildingManager = GetNode<BuildingManager>("BuildingManager");
            TaskManager = GetNode<TaskManager>("TaskManager");
            NaturalResourceManager = GetNode<NaturalResourceManager>("NaturalResourceManager");
            this.SpawnLocation = GetNode<Node3D>("SpawnLocation");
            SpawnInitialVillage();
            SpawnRandomlyDistributedNaturalResources();
        }

        private Vector2I FindSuitableStartLocation()
        {
            return MapManager.WorldToCell(SpawnLocation.GlobalPosition);
        }

        private void SpawnInitialVillage()
        {
            GD.Print("spawning initial village");
            //MapManager.MapData
            var rand = new Random();


            bool spawnSucces = false;
            int spawnRetryCount = 0;
            int spawnRetryCoundCap = 100;
            Vector2I spawnCell = FindSuitableStartLocation();


            while (!spawnSucces && spawnRetryCount < spawnRetryCoundCap)
            {
                spawnRetryCount++;

                //var randI = rand.Next(0, MapManager.GetCellCount());
                //spawnCell = MapManager.MapData.ElementAt(randI).Key;
                //TODO: give randomness in the spawn location, now the loop doesnt do anything
                var building = new HouseBlueprint { 
                    Rotation = BuildingRotation.Right,
                    RequiresBuilding = false
                };

                spawnSucces = BuildingManager.BuildBuilding(spawnCell, building,true, true, default);
            }

            var woodstockpile = new StockpileBlueprint
            {
                Rotation = BuildingRotation.Right,
                RequiresBuilding = false,
                InitialResourceStored = ResourceType.Wood,
                InitialResourceAmount = 100
            };
            BuildingManager.BuildBuilding(spawnCell + new Vector2I(1,-1), woodstockpile, true, true, default);
            BuildingManager.BuildBuilding(spawnCell + new Vector2I(0, -1), woodstockpile, true, true, default);

            var stonestockpile = new StockpileBlueprint
            {
                Rotation = BuildingRotation.Right,
                RequiresBuilding = false,
                InitialResourceStored = ResourceType.Stone,
                InitialResourceAmount = 100
            };
            BuildingManager.BuildBuilding(spawnCell + new Vector2I(1, 2), stonestockpile, true, true, default);
            BuildingManager.BuildBuilding(spawnCell + new Vector2I(0, 2), stonestockpile, true, true, default);

            var roadBlueprint = new RoadBlueprint
            {
                Rotation = BuildingRotation.Right,
                RequiresBuilding = false
            };
            BuildingManager.BuildBuilding(spawnCell + new Vector2I(2, -1), roadBlueprint, true, true, default);
            BuildingManager.BuildBuilding(spawnCell + new Vector2I(2, 0), roadBlueprint, true, true, default);
            BuildingManager.BuildBuilding(spawnCell + new Vector2I(2, 1), roadBlueprint, true, true, default);
            BuildingManager.BuildBuilding(spawnCell + new Vector2I(2, 2), roadBlueprint, true, true, default);

            (UnitDataObject unit, Vector2I offset)[] unitsToSpawn = new (UnitDataObject unit, Vector2I offset)[]
            {
                (new UnitDataObject(UnitGender.FEMALE, ProfessionType.Builder), new Vector2I(2, -2)),
                (new UnitDataObject(UnitGender.MALE, ProfessionType.Worker), new Vector2I(2, -3))
            };
            foreach (var (unit, offset) in unitsToSpawn)
            {
                spawnSucces = false;
                while (!spawnSucces && spawnRetryCount < spawnRetryCoundCap)
                {
                    spawnRetryCount++;

                    var unitCell = spawnCell + offset;
                    var height = MapManager.GetCell(unitCell).Height + 0.1f;
                    Vector3 spawnCoordinate = MapManager.CellToWorld(unitCell, height);

                    //TODO: give randomness in the spawn location, now the loop doesnt do anything
                    spawnSucces = UnitManager.SpawnUnit(spawnCoordinate, unit);
                }
            }

            GD.Print($"initial village spawned");
        }

        private void SpawnRandomlyDistributedNaturalResources()
        {
            const float cellOccupationPercentage = 0.15f;
            var rand = new Random();

            var numberOfCellsToFill = MapManager.MapWidth * MapManager.MapLength * cellOccupationPercentage;

            bool[,] checkedCells = new bool[MapManager.MapWidth, MapManager.MapLength];

            GD.Print($"number of cells to fill: {numberOfCellsToFill}, based on width: {MapManager.MapWidth} and length {MapManager.MapLength}");

            int i = 0;
            while(i < numberOfCellsToFill)
            {
                var vec = new Vector2I(rand.Next(0, MapManager.MapWidth-1), rand.Next(0, MapManager.MapLength-1 )); //TODO: this -1 shouldnt be here, just temp to test

                if (checkedCells[vec.X, vec.Y]) continue;
                checkedCells[vec.X, vec.Y] = true;

                var occupation = MapManager.GetCellOccupation(vec);
                if (occupation.IsOccupied) continue;


                var randomInterpolatedX = (float)rand.NextDouble() * 0.5f + 0.25f;
                var randomInterpolatedY = (float)rand.NextDouble() * 0.5f + 0.25f;

                var mapdata = MapManager.GetCell(vec);

                if (mapdata.Slope > 15) continue;
                if ((mapdata.CellType & (CellType.WATER | CellType.HILL)) > 0) continue;

                var interpolatedCell = MapManager.CellToWorldInterpolated(new Vector2(vec.X + randomInterpolatedX, vec.Y + randomInterpolatedY), height: mapdata.Height);

                var resourcetype = i % 2 == 0 ? NaturalResourceType.Tree : NaturalResourceType.Stone;

                if (NaturalResourceManager.AddResource(interpolatedCell, resourcetype))
                {
                    i++;
                }
                else
                {
                    GD.PushWarning("Could not place resource, why? any reasons?");
                }
            }

        }


        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
        }
    }

}