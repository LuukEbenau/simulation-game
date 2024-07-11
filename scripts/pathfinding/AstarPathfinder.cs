//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Godot;
//using SacaSimulationGame.scripts.map;

//namespace SacaSimulationGame.scripts.pathfinding
//{
//    using System;
//    using System.Collections.Generic;
//    using Godot;

//    namespace SacaSimulationGame.scripts.pathfinding
//    {
//        public class AStarPathfinder
//        {
//            private readonly WorldMapManager _mapManager;
//            private readonly CellType[,] _mapGrid;
//            private readonly int _mapWidth;
//            private readonly int _mapHeight;
//            private readonly PriorityQueue<Node, float> _openSet;
//            private readonly HashSet<Vector2I> _closedSet;
//            private readonly Dictionary<Vector2I, Node> _allNodes;
//            private readonly Vector2I[] _neighbors;

//            public AStarPathfinder(WorldMapManager mapManager)
//            {
//                _mapManager = mapManager;
//                _mapWidth = mapManager.MapWidth;
//                _mapHeight = mapManager.MapHeight;
//                _mapGrid = new CellType[_mapWidth, _mapHeight];
//                _openSet = new PriorityQueue<Node, float>();
//                _closedSet = new HashSet<Vector2I>();
//                _allNodes = new Dictionary<Vector2I, Node>();
//                _neighbors = new Vector2I[4];

//                // Populate the map grid
//                for (int x = 0; x < _mapWidth; x++)
//                {
//                    for (int y = 0; y < _mapHeight; y++)
//                    {
//                        _mapGrid[x, y] = mapManager.GetCell(new Vector2I(x, y)).CellType;
//                    }
//                }
//            }

//            public List<Vector2I> FindPath(Vector2I start, Vector2I goal, CellType traversableTerrainType = CellType.GROUND, int maxIterationCount = 500)
//            {
//                _openSet.Clear();
//                _closedSet.Clear();
//                _allNodes.Clear();

//                var startNode = new Node(start, default, 0, Heuristic(start, goal));
//                _openSet.Enqueue(startNode, startNode.FCost);
//                _allNodes[start] = startNode;

//                int iterationCount = 0;
//                while (_openSet.Count > 0 && iterationCount < maxIterationCount)
//                {
//                    iterationCount++;
//                    var current = _openSet.Dequeue();

//                    if (current.Position == goal)
//                        return ReconstructPath(current);

//                    _closedSet.Add(current.Position);

//                    foreach (var neighbor in GetNeighbors(current.Position))
//                    {
//                        if (!IsValidPosition(neighbor) || _mapGrid[neighbor.X, neighbor.Y] != traversableTerrainType || _closedSet.Contains(neighbor))
//                            continue;

//                        float newGCost = current.GCost + Distance(current.Position, neighbor);

//                        if (!_allNodes.TryGetValue(neighbor, out var neighborNode))
//                        {
//                            neighborNode = new Node(neighbor, current, newGCost, Heuristic(neighbor, goal));
//                            _allNodes[neighbor] = neighborNode;
//                            _openSet.Enqueue(neighborNode, neighborNode.FCost);
//                        }
//                        else if (newGCost < neighborNode.GCost)
//                        {
//                            neighborNode.Parent = current;
//                            neighborNode.GCost = newGCost;
//                            neighborNode.FCost = newGCost + neighborNode.HCost;
//                            // Re-enqueue to update position in priority queue
//                            _openSet.Enqueue(neighborNode, neighborNode.FCost);
//                        }
//                    }
//                }

//                return new List<Vector2I>(); // Return empty list if no path found
//            }

//            private List<Vector2I> ReconstructPath(Node endNode)
//            {
//                var path = new List<Vector2I>();
//                var current = endNode;
//                while (current != default)
//                {
//                    path.Add(current.Position);
//                    current = current.Parent;
//                }
//                path.Reverse();
//                return path;
//            }

//            private float Heuristic(Vector2I a, Vector2I b)
//            {
//                float dx = Math.Abs(a.X - b.X);
//                float dy = Math.Abs(a.Y - b.Y);
//                return 1.0f * (dx + dy) + (1.414f - 2 * 1.0f) * Math.Min(dx, dy); // Octile distance
//            }

//            private float Distance(Vector2I a, Vector2I b)
//            {
//                return a.DistanceTo(b);
//            }

//            private IEnumerable<Vector2I> GetNeighbors(Vector2I node)
//            {
//                _neighbors[0] = new Vector2I(node.X + 1, node.Y);
//                _neighbors[1] = new Vector2I(node.X - 1, node.Y);
//                _neighbors[2] = new Vector2I(node.X, node.Y + 1);
//                _neighbors[3] = new Vector2I(node.X, node.Y - 1);
//                return _neighbors;
//            }

//            private bool IsValidPosition(Vector2I position)
//            {
//                return position.X >= 0 && position.X < _mapWidth && position.Y >= 0 && position.Y < _mapHeight;
//            }

//            private struct Node
//            {
//                public Vector2I Position;
//                public Node Parent;
//                public float GCost;
//                public float HCost;
//                public float FCost;

//                public Node(Vector2I position, Node parent, float gCost, float hCost)
//                {
//                    Position = position;
//                    Parent = parent;
//                    GCost = gCost;
//                    HCost = hCost;
//                    FCost = gCost + hCost;
//                }
//            }
//        }
//    }
//}
