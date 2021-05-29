using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreScript.AStar
{
    public struct Line
    {
        const float verticalLineGradient = 1e5f;

        float gradient;
        float yIntercept;

        Vector2 pointOnLine_1, pointOnLine_2;

        float gradientPerpendicular;
        bool approchSide;

        public Line(Vector2 pointOnLine, Vector2 pointPerpenticularToLine)
        {
            Vector2 delta = pointOnLine - pointPerpenticularToLine;

            if (delta.x == 0)
                gradientPerpendicular = verticalLineGradient;
            else
                gradientPerpendicular = delta.y / delta.x;

            if (gradientPerpendicular == 0)
                gradient = verticalLineGradient;
            else
                gradient = -1 / gradientPerpendicular;

            yIntercept = pointOnLine.y - gradient * pointOnLine.x;
            pointOnLine_1 = pointOnLine;
            pointOnLine_2 = pointOnLine + new Vector2(1, gradient);

            approchSide = false;
            approchSide = GetSide(pointPerpenticularToLine);
        }

        bool GetSide(Vector2 p)
        {
            return (p.x - pointOnLine_1.x) * (pointOnLine_2.y - pointOnLine_1.y) > (p.y - pointOnLine_1.y) * (pointOnLine_2.x - pointOnLine_1.x);
        }

        public bool HasCrossedLine(Vector2 p)
        {
            return GetSide(p) != approchSide;
        }

        public float DistanceFromPoint(Vector2 p)
        {
            float yInterceptPerpendicular = p.y - gradientPerpendicular * p.x;
            float intersectX = (yInterceptPerpendicular - yIntercept) / (gradient - gradientPerpendicular);
            float intersectY = gradient * intersectX + yIntercept;
            return (new Vector2(intersectX, intersectY) - p).magnitude;
        }

        public void DrawWithGizmos(float length)
        {
            Vector3 lineDir = new Vector3(1, 0, gradient).normalized;
            Vector3 lineCenter = new Vector3(pointOnLine_1.x, 1, pointOnLine_1.y);

            Gizmos.DrawLine(lineCenter - lineDir * length * .5f, lineCenter + lineDir * length * .5f);
        }
    }
}