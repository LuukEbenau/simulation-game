using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using SacaSimulationGame.scripts.units;

namespace SacaSimulationGame.scripts.buildings.DO
{
    public class BuildingDataObject
    {
        private static int buildingIndexCounter = 1; //reserve id 0 for empty tiles
        public BuildingBase Instance { get; set; }
        public int Id { get; }
        public List<Vector2I> OccupiedCells { get; set; }
        public Player Player { get; set; }
        public BuildingDataObject(Player player, BuildingBase building)
        {
            Id = buildingIndexCounter++;
            Player = player;
            Instance = building;
        }


        private List<Unit> AssignedUnits { get; set; } = new() { };

        /// <summary>
        /// This counts the amount of times the building was unreachable for a unit, used to filter out unreachable buildings
        /// </summary>
        public int IsUnreachableCounter { get; set; } = 0;

        public int GetNrOfAssignedUnits => AssignedUnits.Count;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unit"></param>
        /// <returns>assigning succes or not</returns>
        public bool AssignUnit(Unit unit)
        {
            if (AssignedUnits.Count >= Instance.MaxBuilders)
            {
                return false;
            }
            if (!AssignedUnits.Contains(unit))
            {
                AssignedUnits.Add(unit);
            }
            return true;
        }
        public void ClearAssignedUnits()
        {
            AssignedUnits.Clear();
        }
        public void UnassignUnit(Unit unit)
        {
            var result = AssignedUnits.Remove(unit);
            if (!result)
            {
                GD.Print($"unit was not assigned to begin with");
            }
        }
    }
}
