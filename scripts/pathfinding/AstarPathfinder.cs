using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using SacaSimulationGame.scripts.map;

namespace SacaSimulationGame.scripts.pathfinding
{
    public class AstarPathfinder
    {
        private AStarGrid2D AStarGrid { get; }
        public WorldMapManager MapManager { get; }

        public AstarPathfinder(WorldMapManager mapManager) { 
            this.AStarGrid = new AStarGrid2D();
            MapManager = mapManager;

            var region = new Rect2I(mapManager.MinX, mapManager.MinY, MapManager.MapWidth, MapManager.MapHeight);
            AStarGrid.Region = region;
            AStarGrid.CellSize = new Vector2I(1, 1);// MapManager.CellSize.ToVec2World();
            //AStarGrid.of
            if(AStarGrid.IsDirty()) AStarGrid.Update();

            AssignCoefficientsToGrid();
        }


        public Vector2[] FindPath(Vector2I startCell, Vector2I goalCell)
        {
            var path = AStarGrid.GetPointPath(startCell, goalCell);

            return path;
        }

        private void AssignCoefficientsToGrid()
        {
            foreach(var p in MapManager.MapData)
            {
                float coefficient = 1;
                bool solid;
                if (p.Value.CellType == CellType.WATER)
                {
                    solid = false;
                }
                else if (p.Value.CellType == CellType.GROUND)
                {
                    solid = true;
                }
                else {
                    throw new Exception($"undefined celltype in pathfinding");
                }
                //TODO: check for roads, and decreace coefficient for those

                //if (!solid)
                //{
                //    AStarGrid.SetPointSolid(p.Key, solid);
                //}
                //else
                //{
                //AStarGrid.SetPointWeightScale(p.Key, coefficient);
                //}
            }
        }
    }
}
