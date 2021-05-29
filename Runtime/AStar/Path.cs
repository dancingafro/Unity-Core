using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreScript.AStar
{
    public class Path
    {
        public readonly Vector3[] lookPoints;
        public readonly Line[] turnBoundaries;
        public readonly int finishIndex;
        public readonly int slowDownIndex;

        public Path(Vector3[] waypoints, Vector3 startPos, float turnDst, float stoppingDst)
        {
            lookPoints = waypoints;
            turnBoundaries = new Line[lookPoints.Length];
            finishIndex = turnBoundaries.Length - 1;

            Vector2 previousPoint = V3ToV2(startPos);

            for (int i = 0; i < lookPoints.Length; ++i)
            {
                Vector2 currentPoint = V3ToV2(lookPoints[i]);
                Vector2 dirToCurrentPoint = (currentPoint - previousPoint).normalized;
                Vector2 turnBoundariesPoint = (i == finishIndex) ? currentPoint : currentPoint - dirToCurrentPoint * turnDst;
                turnBoundaries[i] = new Line(turnBoundariesPoint, previousPoint - dirToCurrentPoint * turnDst);
                previousPoint = turnBoundariesPoint;
            }

            float dstFromEnd = 0;

            for (int i = lookPoints.Length - 1; i > 0; --i)
            {
                dstFromEnd += (lookPoints[i - 1] - lookPoints[i]).magnitude;

                if (dstFromEnd < stoppingDst)
                    continue;
                slowDownIndex = i;
                break;
            }
        }

        Vector2 V3ToV2(Vector3 v3)
        {
            return Utility.UtilityClass.V3ToV2(v3, PositionSpace.xz);
        }

        public void DrawWithGizmos()
        {
            Gizmos.color = Color.black;
            foreach (Vector3 p in lookPoints)
                Gizmos.DrawCube(p + Vector3.up, Vector3.one);

            Gizmos.color = Color.white;
            foreach (Line l in turnBoundaries)
                l.DrawWithGizmos(10);
        }
    }
}