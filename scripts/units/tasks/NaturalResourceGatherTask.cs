using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using SacaSimulationGame.scripts.naturalResources;

namespace SacaSimulationGame.scripts.units.tasks
{
    public class NaturalResourceGatherTask(NaturalResource naturalResource) : UnitTask
    {
        public override bool IsFinished => !Resource.IsInsideTree() || this.Resource.GetNrOfResourcesLeft() == 0;
        public NaturalResource Resource { get; private set; } = naturalResource;

        public override Vector3 TaskPosition
        {
            get
            {
                Vector3 pos;
                try
                {
                    if (!Resource.IsInsideTree())
                    {
                        Debug.Print($"Resource is not inside tree currently, What to do?");
                    }
                    pos = Resource.GlobalPosition;
                }
                catch {
                    Debug.Print($"Resourse does not have a globalposition, did it despawn?");
                    pos = default;
                }
                return pos; 
            }
        }
    }
}
