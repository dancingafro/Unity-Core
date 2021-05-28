using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreScript.Utility;

namespace CoreScript.AStar
{
    public class Node : IHeapItem<Node>
    {
        public bool isWalkable;
        public Vector3 worldPos;

        public Node Parent;

        public int gridX, gridY;
        public int gCost, hCost;
        public int weights;
        int heapIndex;
        public int fCost
        {
            get { return gCost + hCost; }
        }

        public Node(bool isWalkable, Vector3 worldPos, int gridX, int gridY, int weights)
        {
            this.isWalkable = isWalkable;
            this.worldPos = worldPos;
            this.gridX = gridX;
            this.gridY = gridY;
            this.weights = weights;
        }

        public int HeapIndex
        {
            get { return heapIndex; }
            set { heapIndex = value; }
        }

        public int CompareTo(Node other)
        {
            int compare = fCost.CompareTo(other.fCost);
            if (compare == 0)
                compare = hCost.CompareTo(other.hCost);

            return -compare;
        }
    }
}