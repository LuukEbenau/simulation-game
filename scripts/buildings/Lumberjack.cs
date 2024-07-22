﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SacaSimulationGame.scripts.units.dataObjects;

namespace SacaSimulationGame.scripts.buildings
{
    public partial class Lumberjack:StorageBuildingBase
    {
        public override int MaxBuilders => 2;
        public override double TotalBuildingProgressNeeded => 5;
        public override bool IsResourceStorage => false;
    
        public override BuildingType Type => BuildingType.Lumberjack;
        public override void _Ready()
        {
            base._Ready();
            OnBuildingCompleted += House_OnBuildingCompleted;
        }


        private void House_OnBuildingCompleted()
        {
            GameManager.UnitManager.SpawnUnit(GlobalPosition, new UnitDataObject(UnitGender.MALE, units.professions.ProfessionType.Lumberjack));
        }


        // Called every frame. 'delta' is the elapsed time since the previous frame.
        public override void _Process(double delta)
        {
            base._Process(delta);

            if (this.BuildingCompleted)
            {
                //temporary: passive income
                var currentResource = ResourceType.Wood;

                StoredResources.AddResource(currentResource, (float)delta);
            }
        }

        protected override void UpdateVisualBasedOnResources()
        {
            
        }
    }
}
