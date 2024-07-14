using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SacaSimulationGame.scripts
{
    public class Player
    {
        public Guid Id { get; }
        public Player() {
            Id = new Guid();
        }

        //public override bool Equals(object obj)
        //{
        //    return Id.Equals(obj);
        //}
        //public override int GetHashCode()
        //{
        //    return Id.GetHashCode();
        //}
    }
}
