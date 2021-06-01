using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using CoreScript.Utility;
using System;

namespace CoreScript.AStar
{
    public class PathFinding : MonoBehaviour
    {
        NodeGrid grid;

        Stopwatch sw = new Stopwatch();
        private void Awake()
        {
            grid = GetComponent<NodeGrid>();
        }

        public void FindPath(PathRequest pathRequest, Action<PathResult> callback)
        {
            sw.Start();
            Vector3[] waypoints = new Vector3[0];
            bool success = false;

            Node startNode = grid.NodeFromWorldPoint(pathRequest.pathStart);
            Node endNode = grid.NodeFromWorldPoint(pathRequest.pathEnd);

            if (startNode.isWalkable && endNode.isWalkable)
            {
                Heap<Node> openSets = new Heap<Node>(grid.MaxSize);
                HashSet<Node> closedSet = new HashSet<Node>();
                openSets.Add(startNode);

                while (openSets.Count > 0)
                {
                    Node currentNode = openSets.RemoveFirst();
                    closedSet.Add(currentNode);

                    if (currentNode == endNode)
                    {
                        sw.Stop();
                        print("Path found: " + sw.ElapsedMilliseconds + "ms");
                        success = true;
                        break;
                    }

                    foreach (var item in grid.GetNeighbours(currentNode))
                    {
                        if (closedSet.Contains(item) || !item.isWalkable || (pathRequest.needLand && !item.gotLand))
                            continue;

                        int newCost = currentNode.gCost + GetDistance(currentNode, item) + item.weights;
                        if (newCost < item.gCost || !openSets.Contains(item))
                        {
                            item.gCost = newCost;
                            item.hCost = GetDistance(item, endNode);
                            item.Parent = currentNode;

                            if (!openSets.Contains(item))
                                openSets.Add(item);
                            else
                                openSets.UpdateItem(item);
                        }
                    }
                }
            }

            if (success)
            {
                waypoints = RetracePath(pathRequest, startNode, endNode);
                success = waypoints.Length > 0;
            }

            callback(new PathResult(waypoints, success, pathRequest.callback));
        }

        Vector3[] RetracePath(PathRequest request, Node startNode, Node endNode)
        {
            List<Node> path = new List<Node>();
            Node currentNode = endNode;
            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.Parent;
            }
            Vector3[] waypoints = SimplifyPath(path);
            Array.Reverse(waypoints);
            return waypoints;
        }

        Vector3[] SimplifyPath(List<Node> path)
        {
            List<Vector3> waypoints = new List<Vector3>();
            Vector3 dirOld = Vector3.zero;

            for (int i = 1; i < path.Count; ++i)
            {
                Vector3 dirNew = path[i - 1].grid - path[i].grid;
                if (dirNew != dirOld)
                    waypoints.Add(path[i].worldPos);

                dirOld = dirNew;
            }

            return waypoints.ToArray();
        }

        int GetDistance(Node nodeA, Node nodeB)
        {
            int dstX = Mathf.Abs(nodeB.grid.x - nodeA.grid.x);
            int dstY = Mathf.Abs(nodeB.grid.y - nodeA.grid.y);
            int dstZ = Mathf.Abs(nodeB.grid.z - nodeA.grid.z);

            //if (dstX > dstY)
            //    return 14 * dstY + 10 * (dstX - dstY);

            return Mathf.RoundToInt(Mathf.Sqrt(dstX * dstX + dstY * dstY + dstZ * dstZ)); //14 * dstX + 10 * (dstY - dstX);
        }
    }
}