using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Linq;

namespace Pathfinding
{
    public class Pathfinder2D : MonoBehaviour
    {

        Grid2D grid;

        void Awake()
        {
            grid = GetComponent<Grid2D>();
        }


        public void FindPath(PathRequest request, Action<PathResult> callback)
        {

            Stopwatch sw = new Stopwatch();
            sw.Start();

            Vector3[] waypoints = new Vector3[0];
            bool pathSuccess = false;

            Node2D startNode = grid.NodeFromWorldPoint(request.pathStart);
            var endNodes = grid.NodesFromWorldBound(request.pathEnd,request.endBounds);
            UnityEngine.Debug.Log("nodes in end node: " + endNodes.Length);
            Node2D[] targetNode = endNodes.Where(n => n.walkable).ToArray();
            UnityEngine.Debug.Log("usable target nodes: " + targetNode.Length);
            Node2D endNode = startNode;
            try
            {
                endNode = targetNode[0];

                startNode.parent = startNode;


                if (startNode.walkable && targetNode != null && targetNode.Length > 0)
                {
                    Heap<Node2D> openSet = new Heap<Node2D>(grid.MaxSize);
                    HashSet<Node2D> closedSet = new HashSet<Node2D>();
                    openSet.Add(startNode);

                    while (openSet.Count > 0)
                    {
                        Node2D currentNode = openSet.RemoveFirst();
                        closedSet.Add(currentNode);

                        if (targetNode.Contains(currentNode))
                        {
                            sw.Stop();
                            UnityEngine.Debug.Log("Path found: " + sw.ElapsedMilliseconds + " ms");
                            pathSuccess = true;
                            endNode = currentNode;
                            break;
                        }

                        foreach (Node2D neighbour in grid.GetNeighbours(currentNode))
                        {
                            if (!neighbour.walkable || closedSet.Contains(neighbour))
                            {
                                continue;
                            }

                            int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour) + neighbour.movementPenalty;
                            if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                            {
                                neighbour.gCost = newMovementCostToNeighbour;
                                neighbour.hCost = GetDistance(neighbour, targetNode);
                                neighbour.parent = currentNode;

                                if (!openSet.Contains(neighbour))
                                    openSet.Add(neighbour);
                                else
                                    openSet.UpdateItem(neighbour);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log("No usable end nodes");
                callback(new PathResult(waypoints, pathSuccess, request.callback));
            }
            if (pathSuccess)
            {
                waypoints = RetracePath(startNode, endNode);
                pathSuccess = waypoints.Length > 0;
            }
            callback(new PathResult(waypoints, pathSuccess, request.callback));

        }


        Vector3[] RetracePath(Node2D startNode, Node2D endNode)
        {
            List<Node2D> path = new List<Node2D>();
            Node2D currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.parent;
            }
            Vector3[] waypoints = SimplifyPath(path);
            Array.Reverse(waypoints);
            return waypoints;

        }

        Vector3[] SimplifyPath(List<Node2D> path)
        {
            List<Vector3> waypoints = new List<Vector3>();
            Vector2 directionOld = Vector2.zero;

            for (int i = 1; i < path.Count; i++)
            {
                Vector2 directionNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
                if (directionNew != directionOld)
                {
                    waypoints.Add(path[i].worldPosition);
                }
                directionOld = directionNew;
            }
            return waypoints.ToArray();
        }

        int GetDistance(Node2D nodeA, Node2D nodeB)
        {
            int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
            int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

            if (dstX > dstY)
                return 14 * dstY + 10 * (dstX - dstY);
            return 14 * dstX + 10 * (dstY - dstX);
        }

        int GetDistance(Node2D nodeA, Node2D[] nodesB)
        {

            return nodesB.Min<Node2D>(o => GetDistance(nodeA,o));
        }


    }
}