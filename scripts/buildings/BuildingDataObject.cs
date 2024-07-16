﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using SacaSimulationGame.scripts.units;

namespace SacaSimulationGame.scripts.buildings
{
    public class BuildingDataObject
    {
        private static int buildingIndexCounter = 1; //reserve id 0 for empty tiles
        public Building Building { get; set; }
        public int Id { get; }
        public List<Vector2I> OccupiedCells { get; set; }
        public Player Player { get; set; }
        public BuildingDataObject(Player player, Building building)
        {
            this.Id = buildingIndexCounter++;
            this.Player = player;
            this.Building = building;
            this.AssignedUnits = [];
        }
        private List<Unit> AssignedUnits { get; set; }

        /// <summary>
        /// This counts the amount of times the building was unreachable for a unit, used to filter out unreachable buildings
        /// </summary>
        public int IsUnreachableCounter { get; set; } = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="unit"></param>
        /// <returns>assigning succes or not</returns>
        public bool AssignUnit(Unit unit)
        {
            if(AssignedUnits.Count >= Building.MaxBuilders)
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
