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
        PathRequestManager requestManager;
        NodeGrid grid;

        private void Awake()
        {
            requestManager = GetComponent<PathRequestManager>();
            grid = GetComponent<NodeGrid>();
        }

        public void StartFindPath(Vector3 startPos, Vector3 endPos)
        {
            StartCoroutine(FindPath(startPos, endPos));
        }

        IEnumerator FindPath(Vector3 startPos, Vector3 endPos)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Vector3[] waypoints = new Vector3[0];
            bool success = false;

            Node startNode = grid.NodeFromWorldPoint(startPos);
            Node endNode = grid.NodeFromWorldPoint(endPos);

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
                        if (closedSet.Contains(item) || !item.isWalkable)
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
            yield return null;
            if (success)
                waypoints = RetracePath(startNode, endNode);

            requestManager.FinishedProcessingPath(waypoints, success);
        }

        Vector3[] RetracePath(Node startNode, Node endNode)
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
            Vector2 dirOld = Vector2.zero;

            for (int i = 1; i < path.Count; ++i)
            {
                Vector2 dirNew = new Vector2(path[i - 1].gridX - path[i].gridX, path[i - 1].gridY - path[i].gridY);
                if (dirNew != dirOld)
                    waypoints.Add(path[i].worldPos);

                dirOld = dirNew;
            }

            return waypoints.ToArray();
        }

        int GetDistance(Node nodeA, Node nodeB)
        {
            int dstX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
            int dstY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

            if (dstX > dstY)
                return 14 * dstY + 10 * (dstX - dstY);

            return 14 * dstX + 10 * (dstY - dstX);
        }
    }
}