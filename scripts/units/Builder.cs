using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using BehaviourTree;
using BehaviourTree.FluentBuilder;
using Godot;
using Newtonsoft.Json;
using SacaSimulationGame.scripts.buildings;
using SacaSimulationGame.scripts.buildings.dataObjects;
using SacaSimulationGame.scripts.units.dataObjects;

namespace SacaSimulationGame.scripts.units
{
    

    public partial class Builder : Unit
    {
        public override void _Ready()
        {
            base._Ready();
            GD.Print("builder ready");
        }
        public override void _Process(double delta)
        {
            base._Process(delta);

        }


    }
}
