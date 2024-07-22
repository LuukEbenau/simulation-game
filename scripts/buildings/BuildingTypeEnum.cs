using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SacaSimulationGame.scripts.buildings
{
    [Flags]
    public enum BuildingType
    {
        None = 0,
        Road = 1,
        House = 2,
        FishingPost = 4,
        Stockpile = 8,
        Lumberjack = 16,


        //groups can be defined here
        ObstacleBuildings = House | FishingPost
    }
}
