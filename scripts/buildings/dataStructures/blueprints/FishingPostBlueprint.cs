using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using SacaSimulationGame.scripts.map;

namespace SacaSimulationGame.scripts.buildings.dataStructures.blueprints
{
    internal class FishingPostBlueprint : BuildingBlueprintBase
    {
        public override BuildingType Type => BuildingType.FishingPost;
        public override SelectionMode SelectionMode => SelectionMode.Single;
        public override BuildingContraints[,] CellConstraints { get; }
        //public override float MaxSlopeAngle { get; }

        public FishingPostBlueprint()
        {

            CellConstraints = new BuildingContraints[5, 2]
            {
                { new BuildingContraints { CellTypes = CellType.GROUND }, new BuildingContraints { CellTypes = CellType.GROUND }},
                { new BuildingContraints { CellTypes = CellType.GROUND | CellType.WATER }, new BuildingContraints { CellTypes = CellType.GROUND | CellType.WATER }},
                { new BuildingContraints { CellTypes = CellType.WATER }, new BuildingContraints { CellTypes = CellType.WATER }},
                { new BuildingContraints { CellTypes = CellType.WATER }, new BuildingContraints { CellTypes = CellType.WATER }},
                { new BuildingContraints { CellTypes = CellType.WATER }, new BuildingContraints { CellTypes = CellType.WATER }},
            };
            //Shape = new CellType[5, 2]
            //{
            //    { CellType.GROUND,CellType.GROUND},
            //    { CellType.WATER | CellType.GROUND, CellType.WATER | CellType.GROUND },
            //    { CellType.WATER, CellType.WATER },
            //    { CellType.WATER, CellType.WATER },
            //    { CellType.WATER, CellType.WATER },
            //};
            //MaxSlopeAngle = 15f;
        }
    }
}
