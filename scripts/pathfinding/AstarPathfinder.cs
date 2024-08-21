using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Godot;
using SacaSimulationGame.scripts;
using SacaSimulationGame.scripts.buildings;
using SacaSimulationGame.scripts.managers;
using SacaSimulationGame.scripts.map;

namespace SacaSimulationGame.scripts.pathfinding
{

    public class AstarPathfinder
    {
        public AstarPathfinder(GameManager gameManager)
        {
            this.GameManager = gameManager;
        }
        private readonly float roadSpeedMultiplier = 0.5f;
        private const float obstructedTerrainMultiplier = 1.5f;

        private readonly float heuristicCoefficient = 0.5f;
        private readonly float pathScoreCoefficient = 1f;

        private GameManager GameManager { get; }

        private float GetSpeedAtCell(BuildingManager.BuildingTypeIdPair buildingType)
        {
            if (buildingType.BuildingCompleted)
            {
                //if((buildingType.Type & BuildingType.Stockpile) > 0)
                //{
                //    return obstructedTerrainMultiplier;
                //}
                if ((buildingType.Type & (BuildingType.Road | BuildingType.Bridge)) > 0)
                {
                    return roadSpeedMultiplier;
                }
            }

            return 1.0f;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="start">start position</param>
        /// <param name="goal">goal position</param>
        /// <param name="traversableTerrainType">the types of terrain which are traversable by this path</param>
        /// <param name="maxIterationCount">maximum amount of explored cells before canceling</param>
        /// <returns></returns>
        public List<PathfindingNodeGrid> FindPath(Vector2I start, Vector2I goal, CellType traversableTerrainType = CellType.GROUND, BuildingType obstacleBuildings = BuildingType.ObstacleBuildings, int maxIterationCount = 1000)
        {
            var startCellObstacle = GameManager.BuildingManager.OccupiedCells[start.X, start.Y];
            var startCellSpeed = GetSpeedAtCell(startCellObstacle);
            var startNode = new PathfindingNodeGrid(start, startCellSpeed);

            var goalNode = new PathfindingNodeGrid(start, 1.0f);

            var openSet = new SortedSet<(float cost, PathfindingNodeGrid node)>(new NodeComparer());
            var cameFrom = new Dictionary<PathfindingNodeGrid, PathfindingNodeGrid>();
            var gScore = new Dictionary<Vector2I, float> { [start] = 0 };
            var fScore = new Dictionary<Vector2I, float> { [start] = Heuristic(startNode.Cell, goalNode.Cell) };

            openSet.Add((fScore[start], startNode));

            int iCount = 0;
            while (openSet.Count > 0)
            {
                iCount++;
                if(iCount>= maxIterationCount)
                {
                    return new();
                }
                var current = openSet.Min.node;
                if (current.Cell == goal)
                    return ReconstructPath(cameFrom, current);

                openSet.Remove(openSet.Min);
                foreach (var neighbor in GetNeighbors(current.Cell))
                {
                    var cellExists = GameManager.MapManager.TryGetCell(neighbor, out var neighborData);
                    if (!cellExists) continue;
                    

                    var obstacleAtCell = GameManager.BuildingManager.OccupiedCells[neighbor.X, neighbor.Y];

                    // if neighbor is the destination, we can exit
                    if (neighbor == goal)
                    {
                        //if its the goal, we exit directly. Otherwise, pathfinding will never find a path since the building is considered a obstacle
                        //TODO: we might be able to make a* with a collection of goal cells, so that it stops if it hits any of the building cells
                        //TODO: make building blueprints passable until its actually being build.
                        var neighborNode = new PathfindingNodeGrid(neighbor, 1f / GetSpeedAtCell(obstacleAtCell));
                        cameFrom[neighborNode] = current;
                        return ReconstructPath(cameFrom, neighborNode);
                    }

                    if (!this.GameManager.MapManager.CellIsTraversable(neighbor, traversableTerrainType, obstacleBuildings))
                    {
                        continue;
                    }

                    //if (neighborData.CellType != traversableTerrainType)
                    //{
                    //    if (traversableTerrainType.HasFlag(CellType.GROUND) && obstacleAtCell.BuildingCompleted && obstacleAtCell.Type == BuildingType.Bridge) { 
                    //        // its a bridge, so its traversable
                    //    }
                    //    else continue;
                    //}

                    //if ((obstacleAtCell.Type & obstacleBuildings) > 0)
                    //{
                    //    continue;
                    //}

                    var cellSpeedMultiplier = GetSpeedAtCell(obstacleAtCell);

                    float tentativeGScore = gScore[current.Cell] + (Distance(current.Cell, neighbor) * cellSpeedMultiplier * pathScoreCoefficient);
                    if (!gScore.TryGetValue(neighbor, out float value) || tentativeGScore < value)
                    {
                        var neighborNode = new PathfindingNodeGrid(neighbor, 1f / cellSpeedMultiplier);
                        cameFrom[neighborNode] = current;
                        //value = tentativeGScore;
                        gScore[neighbor] = tentativeGScore; //originally value
                        fScore[neighbor] = tentativeGScore + Heuristic(neighbor, goal) * heuristicCoefficient;
                        if (!openSet.Any(x => x.node.Cell == neighbor))
                        {
                            
                            openSet.Add((fScore[neighbor], neighborNode));
                        }
                    }
                }
            }

            return new(); // Return empty list if no path found
        }

        private List<PathfindingNodeGrid> ReconstructPath(Dictionary<PathfindingNodeGrid, PathfindingNodeGrid> cameFrom, PathfindingNodeGrid current)
        {
            var path = new List<PathfindingNodeGrid> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Add(current);
            }
            path.Reverse();
            return path;
        }

        private float Heuristic(Vector2I a, Vector2I b)
        {
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y); // Manhattan distance for grid
        }

        private float Distance(Vector2I a, Vector2I b)
        {
            return a.DistanceTo(b);
        }

        private IEnumerable<Vector2I> GetNeighbors(Vector2I node)
        {
            return new List<Vector2I> {
                new(node.X + 1, node.Y),
                new(node.X - 1, node.Y),
                new(node.X, node.Y + 1),
                new(node.X, node.Y - 1)
            };
        }

        private class NodeComparer : IComparer<(float cost, PathfindingNodeGrid node)>
        {
            public int Compare((float cost, PathfindingNodeGrid node) x, (float cost, PathfindingNodeGrid node) y)
            {
                int result = x.cost.CompareTo(y.cost);
                if (result == 0)
                {
                    result = x.node.Cell.Y.CompareTo(y.node.Cell.Y);
                    if (result == 0)
                    {
                        result = x.node.Cell.X.CompareTo(y.node.Cell.X);
                    }
                }
                return result;
            }
        }
    }
}