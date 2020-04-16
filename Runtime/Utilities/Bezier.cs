using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreScript.Utility
{
    public static class Bezier
    {
        public static Vector2 QuadraticLerp(Vector2 a, Vector2 b, Vector2 c, float t)
        {
            float tSqr = t * t;
            float ts = 2 * t;
            return tSqr * a - 2 * tSqr * b + tSqr * c - ts * a + ts * b + a;
        }

        public static Vector2 CubicLerp(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
        {
            float omt = 1f - t;
            float omtSqr = omt * omt;
            float tSqr = t * t;
            return a * (omtSqr * omt) + b * (3f * omtSqr * t) + c * (3f * omt * tSqr) + d * (tSqr * t);
        }

        public static Vector2 CubicLerp(Vector2[] points, float t)
        {
            return CubicLerp(points[0], points[1], points[2], points[3], t);
        }
    }
}