using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreScript.Utility;

namespace CoreScript.AStar
{
    public class Node : IHeapItem<Node>
    {
        public bool isWalkable, gotLand;
        public Vector3 worldPos;

        public Node Parent;

        public Vector3Int grid;
        public int gCost, hCost;
        public int weights;
        int heapIndex;
        public int FCost
        {
            get { return gCost + hCost; }
        }

        public Node(bool isWalkable, bool gotLand, Vector3 worldPos, Vector3Int grid, int weights)
        {
            this.isWalkable = isWalkable;
            this.gotLand = gotLand;
            this.worldPos = worldPos;
            this.grid = grid;
            this.weights = weights;
        }

        public int HeapIndex
        {
            get { return heapIndex; }
            set { heapIndex = value; }
        }

        public int CompareTo(Node other)
        {
            int compare = FCost.CompareTo(other.FCost);
            if (compare == 0)
                compare = hCost.CompareTo(other.hCost);

            return -compare;
        }
    }
}