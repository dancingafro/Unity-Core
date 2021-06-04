using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreScript.AStar
{
    public class Destructible : MonoBehaviour
    {
        //public const int destructiblePenalty = 10;
        List<Vector3Int> coord = new List<Vector3Int>();
        MinMax3DInt minMax3DInt = new MinMax3DInt();
        bool started = false;
        private void OnEnable()
        {
            if (!started)
                return;
            UpdateGrid();
        }

        public void AddCoord(Vector3Int newCoord)
        {
            started = true;
            if (coord.Contains(newCoord))
                return;

            minMax3DInt.AddValue(newCoord);
            coord.Add(newCoord);
        }

        void UpdateGrid()
        {
            NodeGrid.ModifyGridNode(minMax3DInt);
        }

        private void OnDisable()
        {
            UpdateGrid();
        }
    }
}