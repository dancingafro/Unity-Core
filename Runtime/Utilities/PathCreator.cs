using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreScript.Utility
{
    public class PathCreator : MonoBehaviour
    {
        [HideInInspector]
        public Path path;

        public Color anchorCol = Color.red;
        public Color controlCol = Color.white;
        public Color segmentCol = Color.green;
        public Color selectedCol = Color.yellow;
        public float anchorDiameter = .1f;
        public float controlDiameter = .075f;
        public bool displayControlPoints = true;

        public void CreatePath()
        {
            path = new Path(transform.position);
        }

        void Reset()
        {
            CreatePath();
        }
    }
}