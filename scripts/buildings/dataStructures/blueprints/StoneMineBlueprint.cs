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
        public override BuildingType Type => BuildingType.StoneMine;
        public override BuildingContraints[,] CellConstraints { get; }
        public override SelectionMode SelectionMode => SelectionMode.Single;
        public StoneMineBlueprint()
        {
            const float elevationHeight = 1.5f; //TODO: find the right number

            static bool sizeConstraint(float buildingBaseHeight, float cellHeight, float percent)
            {
                return cellHeight - buildingBaseHeight  >= elevationHeight * percent;
            }

            CalculateCellHeightDelegate ch = (float cellHeight, float baseHeight) => cellHeight;
            var baseCon = new BuildingContraints { CellTypes = CellType.GROUND, MaxSlope = 15f, CalculateHeight = ch };
            var l1Con = new BuildingContraints { CellTypes = CellType.GROUND, ElevationConstraint = (float bh, float ch) => sizeConstraint(bh,ch, 0.33f),  CalculateHeight = ch };
            var l2Con = new BuildingContraints { CellTypes = CellType.GROUND, ElevationConstraint = (float bh, float ch) => sizeConstraint(bh, ch, 0.66f), CalculateHeight = ch };
            var l3Con = new BuildingContraints { CellTypes = CellType.GROUND, ElevationConstraint = (float bh, float ch) => sizeConstraint(bh, ch, 1f),    CalculateHeight = ch };

            CellConstraints = new BuildingContraints[4, 2]
            {
                { baseCon, baseCon},
                { l1Con,   l1Con},
                { l2Con,   l2Con},
                { l2Con,   l2Con}
            };
        }
    }
}
