﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using SacaSimulationGame.scripts.buildings.DO;

namespace SacaSimulationGame.scripts.units.tasks
{
    public class BuildBuildingTask(BuildingDataObject building)
     :UnitTask
    {
        public override bool IsFinished => Building.Instance.BuildingCompleted;
        public override Vector3 TaskPosition => Building.Instance.GlobalPosition;
        public BuildingDataObject Building { get; } = building;


        
    }
}
