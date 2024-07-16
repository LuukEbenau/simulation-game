using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BehaviourTree.FluentBuilder;
using BehaviourTree;
using Godot;

namespace SacaSimulationGame.scripts.units.professions
{
    public class WorkerProfession(Unit unit) : Profession(unit)
    {
        private readonly Random _rnd = new();

        protected override IBehaviour<UnitBTContext> GetBehaviourTree()
        {
            return FluentBuilder.Create<UnitBTContext>()
                .Sequence("root")
                    .Do("Find Delivery Target", this.FindDeliveryTarget)
                    .Do("Find path to target", this.FindPathToDestination)
                    //.Condition("Target found?", c => c.Destination != default)
                    //.Sequence("Deliver")
                    .Do("Move To target", this.MoveToDestination)
                .End()
                .Build();
        }


        public BehaviourStatus FindDeliveryTarget(UnitBTContext context)
        {
            Vector3 destination = new(_rnd.Next(-50, 50), Unit.GlobalPosition.Y, _rnd.Next(-50, 50));
            context.Destination = Unit.GlobalPosition + destination;

            return BehaviourStatus.Succeeded;
        }
    }
}
