using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SacaSimulationGame.scripts.map;

namespace SacaSimulationGame.scripts.buildings
{
    public delegate bool ElevationConstraintDelegate(float buildingBaseHeight, float cellHeight);

    public delegate float CalculateCellHeightDelegate(float cellHeight, float baseHeight);

    public class BuildingContraints
    {
        public CellType CellTypes { get; set; }
        /// <summary>
        /// Minimum elevation difference required from the base of the building
        /// </summary>
        public ElevationConstraintDelegate ElevationConstraint { get; set; }
        public float MaxSlope { get; set; }
        public CalculateCellHeightDelegate CalculateHeight { get; set; }

    }
}
