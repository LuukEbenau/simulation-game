using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using SacaSimulationGame.scripts.map;

namespace SacaSimulationGame.scripts.buildings.dataStructures.blueprints
{
    public class StoneMineBlueprint : BuildingBlueprintBase
    {
        public override BuildingType Type => BuildingType.Stockpile;
        public override BuildingContraints[,] CellConstraints { get; }
        public override SelectionMode SelectionMode => SelectionMode.Single;
        public StoneMineBlueprint()
        {
            const float elevationHeight = 5f; //TODO: find the right number

            static bool halfSizeConstraint(float buildingBaseHeight, float cellHeight)
            {
                return (buildingBaseHeight - cellHeight) >= elevationHeight / 2f;
            }
            static bool fullSizeConstraint(float buildingBaseHeight, float cellHeight)
            {
                return (buildingBaseHeight - cellHeight) >= elevationHeight;
            }

            CalculateCellHeightDelegate ch = (float cellHeight, float baseHeight) => cellHeight;
            var baseCon = new BuildingContraints { CellTypes = CellType.GROUND, MaxSlope = 15f, CalculateHeight = ch };
            var l1Con = new BuildingContraints { CellTypes = CellType.GROUND, ElevationConstraint = halfSizeConstraint, CalculateHeight = ch };
            var l2Con = new BuildingContraints { CellTypes = CellType.GROUND, ElevationConstraint = fullSizeConstraint, CalculateHeight = ch };

            CellConstraints = new BuildingContraints[4, 2]
            {
                { baseCon, baseCon},
                { l1Con, l1Con},
                { l2Con,l2Con},
                { l2Con, l2Con}
            };
        }
    }
}
