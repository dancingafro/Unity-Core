using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreScript.PathCreation;
using TMPro;

namespace CoreScript.Utility
{
    public static class UtilityClass
    {
        static Dictionary<PositionSpace, Vector3> positionSpaceValue;
        public static Dictionary<PositionSpace, Vector3> PositionSpaceValue
        {
            get
            {
                if (positionSpaceValue == null)
                    positionSpaceValue = new Dictionary<PositionSpace, Vector3>
                    {
                        { PositionSpace.xyz, new Vector3(1, 1, 1) },
                        { PositionSpace.xy, new Vector3(1, 1, 0) },
                        { PositionSpace.xz, new Vector3(1, 0, 1) },
                        { PositionSpace.yz, new Vector3(0, 1, 1) }
                    };

                return positionSpaceValue;
            }
        }

        public static void CombineList<T>(ref List<T> existingList, List<T> newList)
        {
            foreach (var item in newList)
            {
                if (existingList.Contains(item))
                    continue;

                existingList.Add(item);
            }
        }

        public static List<T> CombineList<T>(List<T> existingList, List<T> newList)
        {
            List<T> list = new List<T>(existingList);
            foreach (var item in newList)
            {
                if (list.Contains(item))
                    continue;

                list.Add(item);
            }

            return list;
        }

        public static Vector3 MousePosition(Camera camera)
        {
            return new Vector3(Input.mousePosition.x, Input.mousePosition.y, camera.transform.position.z);
        }

        public static Vector3 ScreenToWorldPos(Camera camera, Vector3 screenPos)
        {
            return camera.ScreenToWorldPoint(screenPos);
        }

        public static TextMeshPro CreateWorldText(string Text, Color color, Transform parent = null, Vector3 localPosition = default, int fontSize = 40, TextAlignmentOptions textAlignment = TextAlignmentOptions.Left, int SortingOrfer = 0)
        {
            if (color == null) color = Color.white;
            return CreateWorldText(color, Text, parent, localPosition, fontSize, textAlignment, SortingOrfer);
        }

        public static TextMeshPro CreateWorldText(Color color, string Text, Transform parent, Vector3 localPosition, int fontSize, TextAlignmentOptions textAlignment, int sortingOrder)
        {
            GameObject gameObject = new GameObject("World_Text", typeof(TextMeshPro), typeof(Billboard));

            Transform transform = gameObject.transform;
            transform.SetParent(parent);
            transform.localPosition = localPosition;

            TextMeshPro textMesh = transform.GetComponent<TextMeshPro>();
            textMesh.alignment = textAlignment;
            textMesh.text = Text;
            textMesh.fontSize = fontSize;
            textMesh.color = color;
            textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;

            return textMesh;
        }

        public static Texture2D TextureFromColors(Color[] colors, int width, int height)
        {
            Texture2D texture = new Texture2D(width, height)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixels(colors);
            texture.Apply();
            return texture;
        }

        public static Texture2D TextureFromHeight(float[,] heightMap)
        {
            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);

            Color[] colors = new Color[width * height];

            for (int x = 0; x < width; ++x)
                for (int y = 0; y < height; ++y)
                    colors[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);

            return TextureFromColors(colors, width, height);
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            T c = a;
            a = b;
            b = c;
        }
    }

    public static class MathUtility
    {
        public static Vector3 QuadraticBezier2D(Vector3[] pts, float t) { return QuadraticBezier2D(pts[0], pts[1], pts[2], t); }
        public static Vector3 QuadraticBezier2D(Vector3 a, Vector3 b, Vector3 c, float t)
        {
            Vector3 quadraticBezier = QuadraticBezier(a, b, c, t);
            quadraticBezier.z = 0;
            return quadraticBezier;
        }

        public static Vector3 CubicBezier2D(Vector3[] pts, float t) { return CubicBezier2D(pts[0], pts[1], pts[2], pts[3], t); }
        public static Vector3 CubicBezier2D(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
        {
            Vector3 cubicBezier = CubicBezier(a, b, c, d, t);
            cubicBezier.z = 0;
            return cubicBezier;
        }

        public static Vector3 CubicTangent2D(Vector3[] pts, float t) { return CubicTangent2D(pts[0], pts[1], pts[2], pts[3], t); }
        public static Vector3 CubicTangent2D(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
        {
            Vector3 tangent = CubicTangent(a, b, c, d, t);
            return new Vector3(tangent.x, tangent.y);
        }

        public static Vector3 CubicNormal2D(Vector3[] pts, float t) { return CubicNormal2D(pts[0], pts[1], pts[2], pts[3], t); }
        public static Vector3 CubicNormal2D(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
        {
            Vector3 tangent = CubicTangent2D(a, b, c, d, t);
            return new Vector3(-tangent.y, tangent.x);
        }

        public static Quaternion CubicOrientation2D(Vector3[] pts, float t) { return CubicOrientation2D(pts[0], pts[1], pts[2], pts[3], t); }
        public static Quaternion CubicOrientation2D(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
        {
            Vector3 tangent = CubicTangent2D(a, b, c, d, t);
            Vector3 normal = CubicNormal2D(a, b, c, d, t);
            return Quaternion.LookRotation(tangent, normal);
        }

        public static Vector3 QuadraticBezier(Vector3[] pts, float t) { return QuadraticBezier(pts[0], pts[1], pts[2], t); }
        public static Vector3 QuadraticBezier(Vector3 a, Vector3 b, Vector3 c, float t)
        {
            t = Mathf.Clamp01(t);
            float tSqr = t * t;
            float ts = 2 * t;
            return tSqr * a - 2 * tSqr * b + tSqr * c - ts * a + ts * b + a;
        }

        public static Vector3 CubicBezier(Vector3[] pts, float t) { return CubicBezier(pts[0], pts[1], pts[2], pts[3], t); }
        public static Vector3 CubicBezier(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
        {
            t = Mathf.Clamp01(t);
            float omt = 1f - t;
            float omtSqr = omt * omt;
            float tSqr = t * t;
            return a * (omtSqr * omt) + b * (3f * omtSqr * t) + c * (3f * omt * tSqr) + d * (tSqr * t);
        }

        public static Vector3 CubicTangent(Vector3[] pts, float t) { return CubicTangent(pts[0], pts[1], pts[2], pts[3], t); }
        public static Vector3 CubicTangent(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
        {
            t = Mathf.Clamp01(t);
            float omt = 1f - t;
            float omtSqr = omt * omt;
            float tSqr = t * t;

            Vector3 tangent = a * (-omtSqr) +
                             b * (3f * omtSqr - 2 * omt) +
                             c * (-3f * tSqr + 2 * t) +
                             d * tSqr;

            return tangent.normalized;
        }

        public static Vector3 CubicNormal(Vector3[] pts, Vector3 up, float t) { return CubicNormal(pts[0], pts[1], pts[2], pts[3], up, t); }
        public static Vector3 CubicNormal(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 up, float t)
        {
            Vector3 tangent = CubicTangent(a, b, c, d, t);
            Vector3 biNormal = Vector3.Cross(up, tangent).normalized;
            return Vector3.Cross(tangent, biNormal);
        }

        public static Quaternion CubicOrientation(Vector3[] pts, Vector3 up, float t) { return CubicOrientation(pts[0], pts[1], pts[2], pts[3], up, t); }
        public static Quaternion CubicOrientation(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 up, float t)
        {
            Vector3 tangent = CubicTangent(a, b, c, d, t);
            Vector3 normal = CubicNormal(a, b, c, d, up, t);
            return Quaternion.LookRotation(tangent, normal);
        }

        /// Calculates the derivative of the curve at time 't'
        /// This is the vector tangent to the curve at that point
        public static Vector3 EvaluateCurveDerivative(Vector3[] pts, float t) { return EvaluateCurveDerivative(pts[0], pts[1], pts[2], pts[3], t); }
        public static Vector3 EvaluateCurveDerivative(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
        {
            t = Mathf.Clamp01(t);
            float omt = 1f - t;
            float omtSqr = omt * omt;
            float tSqr = t * t;

            return 3 * omtSqr * (b - a) + 6 * omt * t * (c - b) + 3 * tSqr * (d - c);
        }

        /// Returns the second derivative of the curve at time 't'
        public static Vector3 EvaluateCurveSecondDerivative(Vector3[] points, float t) { return EvaluateCurveSecondDerivative(points[0], points[1], points[2], points[3], t); }
        ///Returns the second derivative of the curve at time 't'
        public static Vector3 EvaluateCurveSecondDerivative(Vector3 a1, Vector3 c1, Vector3 c2, Vector3 a2, float t)
        {
            t = Mathf.Clamp01(t);
            float omt = 1f - t;
            return 6 * omt * (c2 - 2 * c1 + a1) + 6 * t * (a2 - 2 * c2 + c1);
        }

        // Transform point from local to world space
        public static Vector3 TransformPoint(Vector3 p, Transform t, PathSpace space)
        {
            // path only works correctly for uniform scales, so average out xyz global scale
            float scale = Vector3.Dot(t.lossyScale, Vector3.one) / 3;
            Vector3 constrainedPos = t.position;
            Quaternion constrainedRot = t.rotation;
            ConstrainPosRot(ref constrainedPos, ref constrainedRot, space);
            return constrainedRot * p * scale + constrainedPos;
        }

        // Transform point from world to local space
        public static Vector3 InverseTransformPoint(Vector3 p, Transform t, PathSpace space)
        {
            Vector3 constrainedPos = t.position;
            Quaternion constrainedRot = t.rotation;
            ConstrainPosRot(ref constrainedPos, ref constrainedRot, space);

            // path only works correctly for uniform scales, so average out xyz global scale
            float scale = Vector3.Dot(t.lossyScale, Vector3.one) / 3;
            var offset = p - constrainedPos;

            return Quaternion.Inverse(constrainedRot) * offset / scale;
        }

        // Transform vector from local to world space (affected by rotation and scale, but not position)
        public static Vector3 TransformVector(Vector3 p, Transform t, PathSpace space)
        {
            // path only works correctly for uniform scales, so average out xyz global scale
            float scale = Vector3.Dot(t.lossyScale, Vector3.one) / 3;
            Quaternion constrainedRot = t.rotation;
            ConstrainRot(ref constrainedRot, space);
            return constrainedRot * p * scale;
        }

        // Transform vector from world to local space (affected by rotation and scale, but not position)
        public static Vector3 InverseTransformVector(Vector3 p, Transform t, PathSpace space)
        {
            Quaternion constrainedRot = t.rotation;
            ConstrainRot(ref constrainedRot, space);
            // path only works correctly for uniform scales, so average out xyz global scale
            float scale = Vector3.Dot(t.lossyScale, Vector3.one) / 3;
            return Quaternion.Inverse(constrainedRot) * p / scale;
        }

        // Transform vector from local to world space (affected by rotation, but not position or scale)
        public static Vector3 TransformDirection(Vector3 p, Transform t, PathSpace space)
        {
            Quaternion constrainedRot = t.rotation;
            ConstrainRot(ref constrainedRot, space);
            return constrainedRot * p;
        }

        // Transform vector from world to local space (affected by rotation, but not position or scale)
        public static Vector3 InverseTransformDirection(Vector3 p, Transform t, PathSpace space)
        {
            Quaternion constrainedRot = t.rotation;
            ConstrainRot(ref constrainedRot, space);
            return Quaternion.Inverse(constrainedRot) * p;
        }

        public static bool LineSegmentsIntersect(Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2)
        {
            float d = (b2.x - b1.x) * (a1.y - a2.y) - (a1.x - a2.x) * (b2.y - b1.y);
            if (d == 0)
                return false;
            float t = ((b1.y - b2.y) * (a1.x - b1.x) + (b2.x - b1.x) * (a1.y - b1.y)) / d;
            float u = ((a1.y - a2.y) * (a1.x - b1.x) + (a2.x - a1.x) * (a1.y - b1.y)) / d;

            return t >= 0 && t <= 1 && u >= 0 && u <= 1;
        }

        public static bool LinesIntersect(Vector2 a1, Vector2 a2, Vector2 a3, Vector2 a4)
        {
            return (a1.x - a2.x) * (a3.y - a4.y) - (a1.y - a2.y) * (a3.x - a4.x) != 0;
        }

        public static Vector2 PointOfLineLineIntersection(Vector2 a1, Vector2 a2, Vector2 a3, Vector2 a4)
        {
            float d = (a1.x - a2.x) * (a3.y - a4.y) - (a1.y - a2.y) * (a3.x - a4.x);
            if (d == 0)
            {
                Debug.LogError("Lines are parallel, please check that this is not the case before calling line intersection method");
                return Vector2.zero;
            }

            float n = (a1.x - a3.x) * (a3.y - a4.y) - (a1.y - a3.y) * (a3.x - a4.x);
            float t = n / d;
            return a1 + (a2 - a1) * t;
        }

        public static Vector2 ClosestPointOnLineSegment(Vector2 p, Vector2 a, Vector2 b)
        {
            Vector2 aB = b - a;
            Vector2 aP = p - a;
            float sqrLenAB = aB.sqrMagnitude;

            if (sqrLenAB == 0)
                return a;

            float t = Mathf.Clamp01(Vector2.Dot(aP, aB) / sqrLenAB);
            return a + aB * t;
        }

        public static Vector3 ClosestPointOnLineSegment(Vector3 p, Vector3 a, Vector3 b)
        {
            Vector3 aB = b - a;
            Vector3 aP = p - a;
            float sqrLenAB = aB.sqrMagnitude;

            if (sqrLenAB == 0)
                return a;

            float t = Mathf.Clamp01(Vector3.Dot(aP, aB) / sqrLenAB);
            return a + aB * t;
        }

        public static int SideOfLine(Vector2 a, Vector2 b, Vector2 c)
        {
            return (int)Mathf.Sign((c.x - a.x) * (-b.y + a.y) + (c.y - a.y) * (b.x - a.x));
        }

        /// returns the smallest angle between ABC. Never greater than 180
        public static float MinAngle(Vector3 a, Vector3 b, Vector3 c)
        {
            return Vector3.Angle((a - b), (c - b));
        }

        // Crude, but fast estimation of curve length.
        public static float EstimateCurveLength(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float controlNetLength = (p0 - p1).magnitude + (p1 - p2).magnitude + (p2 - p3).magnitude;
            float estimatedCurveLength = (p0 - p3).magnitude + controlNetLength * .5f;
            return estimatedCurveLength;
        }

        public static bool PointInTriangle(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
        {
            float area = 0.5f * (-b.y * c.x + a.y * (-b.x + c.x) + a.x * (b.y - c.y) + b.x * c.y);
            float s = 1 / (2 * area) * (a.y * c.x - a.x * c.y + (c.y - a.y) * p.x + (a.x - c.x) * p.y);
            float t = 1 / (2 * area) * (a.x * b.y - a.y * b.x + (a.y - b.y) * p.x + (b.x - a.x) * p.y);
            return s >= 0 && t >= 0 && (s + t) <= 1;
        }

        public static bool PointsAreClockwise(Vector2[] points)
        {
            float signedArea = 0;
            for (int i = 0; i < points.Length; i++)
            {
                int nextIndex = (i + 1) % points.Length;
                signedArea += (points[nextIndex].x - points[i].x) * (points[nextIndex].y + points[i].y);
            }

            return signedArea >= 0;
        }

        static void ConstrainPosRot(ref Vector3 pos, ref Quaternion rot, PathSpace space)
        {
            Vector3 eulerAngles;
            switch (space)
            {
                case PathSpace.xy:
                    eulerAngles = rot.eulerAngles;
                    if (eulerAngles.x != 0 || eulerAngles.y != 0)
                        rot = Quaternion.AngleAxis(eulerAngles.z, Vector3.forward);

                    pos = new Vector3(pos.x, pos.y, 0);
                    break;
                case PathSpace.xz:
                    eulerAngles = rot.eulerAngles;
                    if (eulerAngles.x != 0 || eulerAngles.z != 0)
                        rot = Quaternion.AngleAxis(eulerAngles.y, Vector3.up);

                    pos = new Vector3(pos.x, 0, pos.z);
                    break;
            }
        }

        static void ConstrainRot(ref Quaternion rot, PathSpace space)
        {
            Vector3 eulerAngles;
            switch (space)
            {
                case PathSpace.xy:
                    eulerAngles = rot.eulerAngles;
                    if (eulerAngles.x != 0 || eulerAngles.y != 0)
                        rot = Quaternion.AngleAxis(eulerAngles.z, Vector3.forward);
                    break;
                case PathSpace.xz:
                    eulerAngles = rot.eulerAngles;
                    if (eulerAngles.x != 0 || eulerAngles.z != 0)
                        rot = Quaternion.AngleAxis(eulerAngles.y, Vector3.up);
                    break;
            }
        }
    }

    public static class PathUtility
    {
        public static PathSplitData SplitBezierPathEvenly(BezierPath bezierPath, float spacing, float accuracy)
        {
            PathSplitData splitData = new PathSplitData();

            splitData.vertices.Add(bezierPath[0]);
            splitData.tangents.Add(MathUtility.EvaluateCurveDerivative(bezierPath.GetPointsInSegment(0), 0).normalized);
            splitData.cumulativeLength.Add(0);
            splitData.anchorVertexMap.Add(0);
            splitData.minMax.AddValue(bezierPath[0]);

            Vector3 prevPointOnPath = bezierPath[0];
            Vector3 lastAddedPoint = bezierPath[0];

            float currentPathLength = 0;
            float dstSinceLastVertex = 0;

            // Go through all segments and split up into vertices
            for (int segmentIndex = 0; segmentIndex < bezierPath.NumSegments; segmentIndex++)
            {
                Vector3[] segmentPoints = bezierPath.GetPointsInSegment(segmentIndex);
                float estimatedSegmentLength = MathUtility.EstimateCurveLength(segmentPoints[0], segmentPoints[1], segmentPoints[2], segmentPoints[3]);
                int divisions = Mathf.CeilToInt(estimatedSegmentLength * accuracy);
                float increment = 1f / divisions;

                for (float t = increment; t <= 1; t += increment)
                {
                    bool isLastPointOnPath = (t + increment > 1 && segmentIndex == bezierPath.NumSegments - 1);
                    if (isLastPointOnPath)
                    {
                        t = 1;
                    }
                    Vector3 pointOnPath = MathUtility.CubicBezier(segmentPoints, t);
                    dstSinceLastVertex += (pointOnPath - prevPointOnPath).magnitude;

                    // If vertices are now too far apart, go back by amount we overshot by
                    if (dstSinceLastVertex > spacing)
                    {
                        float overshootDst = dstSinceLastVertex - spacing;
                        pointOnPath += (prevPointOnPath - pointOnPath).normalized * overshootDst;
                        t -= increment;
                    }

                    if (dstSinceLastVertex >= spacing || isLastPointOnPath)
                    {
                        currentPathLength += (lastAddedPoint - pointOnPath).magnitude;
                        splitData.cumulativeLength.Add(currentPathLength);
                        splitData.vertices.Add(pointOnPath);
                        splitData.tangents.Add(MathUtility.EvaluateCurveDerivative(segmentPoints, t).normalized);
                        splitData.minMax.AddValue(pointOnPath);
                        dstSinceLastVertex = 0;
                        lastAddedPoint = pointOnPath;
                    }
                    prevPointOnPath = pointOnPath;
                }
                splitData.anchorVertexMap.Add(splitData.vertices.Count - 1);
            }
            return splitData;
        }

        public static PathSplitData SplitBezierPathByAngleError(BezierPath bezierPath, float maxAngleError, float minVertexDst, float accuracy)
        {
            PathSplitData splitData = new PathSplitData();

            splitData.vertices.Add(bezierPath[0]);
            splitData.tangents.Add(MathUtility.EvaluateCurveDerivative(bezierPath.GetPointsInSegment(0), 0).normalized);
            splitData.cumulativeLength.Add(0);
            splitData.anchorVertexMap.Add(0);
            splitData.minMax.AddValue(bezierPath[0]);

            Vector3 prevPointOnPath = bezierPath[0];
            Vector3 lastAddedPoint = bezierPath[0];

            float currentPathLength = 0;
            float dstSinceLastVertex = 0;

            // Go through all segments and split up into vertices
            for (int segmentIndex = 0; segmentIndex < bezierPath.NumSegments; segmentIndex++)
            {
                Vector3[] segmentPoints = bezierPath.GetPointsInSegment(segmentIndex);
                float estimatedSegmentLength = MathUtility.EstimateCurveLength(segmentPoints[0], segmentPoints[1], segmentPoints[2], segmentPoints[3]);
                int divisions = Mathf.CeilToInt(estimatedSegmentLength * accuracy);
                float increment = 1f / divisions;

                for (float t = increment; t <= 1; t += increment)
                {
                    bool isLastPointOnPath = (t + increment > 1 && segmentIndex == bezierPath.NumSegments - 1);
                    if (isLastPointOnPath)
                        t = 1;
                    Vector3 pointOnPath = MathUtility.CubicBezier(segmentPoints, t);
                    Vector3 nextPointOnPath = MathUtility.CubicBezier(segmentPoints, t + increment);

                    // angle at current point on path
                    float localAngle = 180 - MathUtility.MinAngle(prevPointOnPath, pointOnPath, nextPointOnPath);
                    // angle between the last added vertex, the current point on the path, and the next point on the path
                    float angleFromPrevVertex = 180 - MathUtility.MinAngle(lastAddedPoint, pointOnPath, nextPointOnPath);
                    float angleError = Mathf.Max(localAngle, angleFromPrevVertex);


                    if ((angleError > maxAngleError && dstSinceLastVertex >= minVertexDst) || isLastPointOnPath)
                    {
                        currentPathLength += (lastAddedPoint - pointOnPath).magnitude;
                        splitData.cumulativeLength.Add(currentPathLength);
                        splitData.vertices.Add(pointOnPath);
                        splitData.tangents.Add(MathUtility.EvaluateCurveDerivative(segmentPoints, t).normalized);
                        splitData.minMax.AddValue(pointOnPath);
                        dstSinceLastVertex = 0;
                        lastAddedPoint = pointOnPath;
                    }
                    else
                        dstSinceLastVertex += (pointOnPath - prevPointOnPath).magnitude;

                    prevPointOnPath = pointOnPath;
                }
                splitData.anchorVertexMap.Add(splitData.vertices.Count - 1);
            }
            return splitData;
        }

        /// Splits curve into two curves at time t. Returns 2 arrays of 4 points.
        public static Vector3[][] SplitCurve(Vector3[] points, float t)
        {
            Vector3 a1 = Vector3.Lerp(points[0], points[1], t);
            Vector3 a2 = Vector3.Lerp(points[1], points[2], t);
            Vector3 a3 = Vector3.Lerp(points[2], points[3], t);
            Vector3 b1 = Vector3.Lerp(a1, a2, t);
            Vector3 b2 = Vector3.Lerp(a2, a3, t);
            Vector3 pointOnCurve = Vector3.Lerp(b1, b2, t);

            return new Vector3[][] {
                new Vector3[] { points[0], a1, b1, pointOnCurve },
                    new Vector3[] { pointOnCurve, b2, a3, points[3] }
            };
        }

        public static Bounds CalculateSegmentBounds(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            MinMax3D minMax = new MinMax3D();
            minMax.AddValue(p0);
            minMax.AddValue(p3);

            List<float> extremePointTimes = ExtremePointTimes(p0, p1, p2, p3);
            foreach (float t in extremePointTimes)
                minMax.AddValue(MathUtility.CubicBezier(p0, p1, p2, p3, t));

            return new Bounds((minMax.Min + minMax.Max) * .5f, minMax.Max - minMax.Min);
        }

        /// Times of stationary points on curve (points where derivative is zero on any axis)
        public static List<float> ExtremePointTimes(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            // coefficients of derivative function
            Vector3 a = 3 * (-p0 + 3 * p1 - 3 * p2 + p3);
            Vector3 b = 6 * (p0 - 2 * p1 + p2);
            Vector3 c = 3 * (p1 - p0);

            List<float> times = new List<float>();
            times.AddRange(StationaryPointTimes(a.x, b.x, c.x));
            times.AddRange(StationaryPointTimes(a.y, b.y, c.y));
            times.AddRange(StationaryPointTimes(a.z, b.z, c.z));
            return times;
        }

        // Finds times of stationary points on curve defined by ax^2 + bx + c.
        // Only times between 0 and 1 are considered as Bezier only uses values in that range
        static IEnumerable<float> StationaryPointTimes(float a, float b, float c)
        {
            List<float> times = new List<float>();

            // from quadratic equation: y = [-b +- sqrt(b^2 - 4ac)]/2a
            if (a == 0)
                return times;

            float discriminant = b * b - 4 * a * c;
            if (discriminant < 0)
                return times;

            float s = Mathf.Sqrt(discriminant);
            float t1 = (-b + s) / (2 * a);
            if (t1 >= 0 && t1 <= 1)
                times.Add(t1);

            if (discriminant != 0)
            {
                float t2 = (-b - s) / (2 * a);
                if (t2 >= 0 && t2 <= 1)
                    times.Add(t2);
            }

            return times;
        }

        public class PathSplitData
        {
            public List<Vector3> vertices = new List<Vector3>();
            public List<Vector3> tangents = new List<Vector3>();
            public List<float> cumulativeLength = new List<float>();
            public List<int> anchorVertexMap = new List<int>();
            public MinMax3D minMax = new MinMax3D();
        }
    }
}
