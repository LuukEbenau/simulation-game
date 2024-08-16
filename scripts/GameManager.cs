using Godot;
using SacaSimulationGame.scripts.building;
using SacaSimulationGame.scripts.buildings;
using SacaSimulationGame.scripts.buildings.dataStructures.blueprints;
using SacaSimulationGame.scripts.map;
using SacaSimulationGame.scripts.units;
using SacaSimulationGame.scripts.units.dataObjects;
using SacaSimulationGame.scripts.units.professions.misc;
using SacaSimulationGame.scripts.units.tasks;
using System;
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

            (UnitDataObject unit, Vector2I offset)[] unitsToSpawn = [
                //(new UnitDataObject(UnitGender.MALE,   ProfessionType.Worker ), new Vector2I(2, 2)),
                //(new UnitDataObject(UnitGender.FEMALE, ProfessionType.Builder), new Vector2I(2, 3)),
                (new UnitDataObject(UnitGender.FEMALE, ProfessionType.Builder), new Vector2I(2,-2)),
                (new UnitDataObject(UnitGender.MALE,   ProfessionType.Worker ), new Vector2I(2,-3))
            ];
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

        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
        }
    }

}