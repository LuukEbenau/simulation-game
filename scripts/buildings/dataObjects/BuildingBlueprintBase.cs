﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using SacaSimulationGame.scripts.map;

namespace SacaSimulationGame.scripts.buildings.dataObjects
{
    public abstract class BuildingBlueprintBase
    {
        public abstract BuildingType Type { get; }
        public BuildingRotation Rotation { get; set; } = BuildingRotation.Bottom;
        public abstract CellType[,] Shape { get; }
        public abstract float MaxSlopeAngle { get; }
        public bool RequiresBuilding = true;
        public abstract SelectionMode SelectionMode { get; }
    }
}
