using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using SacaSimulationGame.scripts;
using SacaSimulationGame.scripts.managers;
using SacaSimulationGame.scripts.map;

namespace SacaSimulationGame.scripts.pathfinding
{
    public class AstarPathfinder(GameManager gameManager)
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="start">start position</param>
        /// <param name="goal">goal position</param>
        /// <param name="traversableTerrainType">the types of terrain which are traversable by this path</param>
        /// <param name="maxIterationCount">maximum amount of explored cells before canceling</param>
        /// <returns></returns>
        public List<Vector2I> FindPath(Vector2I start, Vector2I goal, CellType traversableTerrainType = CellType.GROUND, int maxIterationCount = 500)
        {
            var openSet = new SortedSet<(float cost, Vector2I node)>(new NodeComparer());
            var cameFrom = new Dictionary<Vector2I, Vector2I>();
            var gScore = new Dictionary<Vector2I, float> { [start] = 0 };
            var fScore = new Dictionary<Vector2I, float> { [start] = Heuristic(start, goal) };

            openSet.Add((fScore[start], start));

            int iCount = 0;
            while (openSet.Count > 0)
            {
                iCount++;
                if(iCount>= maxIterationCount)
                {
                    return [];
                }
                var current = openSet.Min.node;
                if (current == goal)
                    return ReconstructPath(cameFrom, current);

                openSet.Remove(openSet.Min);
                foreach (var neighbor in GetNeighbors(current))
                {
                    if (!gameManager.MapManager.TryGetCell(neighbor, out var neighborData) || neighborData.CellType != traversableTerrainType)
                        continue;
                    //TODO: obstacle check
                    var cellOccupied = gameManager.BuildingManager.OccupiedCells[neighbor.X, neighbor.Y] > 0;
                    if (cellOccupied) continue;

                    float tentativeGScore = gScore[current] + Distance(current, neighbor);
                    if (!gScore.TryGetValue(neighbor, out float value) || tentativeGScore < value)
                    {
                        cameFrom[neighbor] = current;
                        value = tentativeGScore;
                        gScore[neighbor] = value;
                        fScore[neighbor] = tentativeGScore + Heuristic(neighbor, goal);
                        if (!openSet.Any(x => x.node == neighbor))
                            openSet.Add((fScore[neighbor], neighbor));
                    }
                }
            }

            return []; // Return empty list if no path found
        }

        private List<Vector2I> ReconstructPath(Dictionary<Vector2I, Vector2I> cameFrom, Vector2I current)
        {
            var path = new List<Vector2I> { current };
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
            return[
                new(node.X + 1, node.Y),
                new(node.X - 1, node.Y),
                new(node.X, node.Y + 1),
                new(node.X, node.Y - 1)
            ];
        }

        private class NodeComparer : IComparer<(float cost, Vector2I node)>
        {
            public int Compare((float cost, Vector2I node) x, (float cost, Vector2I node) y)
            {
                int result = x.cost.CompareTo(y.cost);
                if (result == 0)
                {
                    result = x.node.Y.CompareTo(y.node.Y);
                    if (result == 0)
                    {
                        result = x.node.X.CompareTo(y.node.X);
                    }
                }
                return result;
            }
        }
    }
}