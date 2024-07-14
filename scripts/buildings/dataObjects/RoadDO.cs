﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using SacaSimulationGame.scripts.map;

namespace SacaSimulationGame.scripts.buildings.dataObjects
{
    public class RoadDO : BuildingDO
    {
        public override CellType[,] Shape { get; }
        public override string Name => "Road";
        public override float MaxSlopeAngle { get; }
        private static PackedScene _scene;
        public override PackedScene Scene => _scene ??= GD.Load("res://assets/buildings/road.blend") as PackedScene;
        public RoadDO()
        {
            Shape = new CellType[1, 1]
            {
                { CellType.GROUND}
            };
            MaxSlopeAngle = 20f;
        }

        public override float TotalBuildingProgressNeeded => 3;
    }
}
