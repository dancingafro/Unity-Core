using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreScript.Utility
{
    public static class UtilityCode
    {
        public enum Pos2D
        {
            XY,
            XZ,
            YZ
        }

        public static Vector3 ScreenToWorld3DPos(Camera camera, Vector3 screenPos)
        {
            return camera.ScreenToWorldPoint(screenPos);
        }

        public static Vector3 ScreenToWorld2DPos(Camera camera, Vector3 screenPos, Pos2D pos2D = Pos2D.XY)
        {
            Vector3 worldPos = ScreenToWorld3DPos(camera, screenPos);

            switch (pos2D)
            {
                case Pos2D.YZ:
                    worldPos.x = 0;
                    break;
                case Pos2D.XZ:
                    worldPos.y = 0;
                    break;
                default:
                    worldPos.z = 0;
                    break;
            }

            return worldPos;
        }

        public static TextMesh CreateWorldText(string Text, Color color, Transform parent = null, Vector3 localPosition = default, int fontSize = 40, TextAnchor textAnchor = TextAnchor.LowerLeft, TextAlignment textAlignment = TextAlignment.Left, int SortingOrfer = 0)
        {
            if (color == null) color = Color.white;
            return CreateWorldText(color, Text, parent, localPosition, fontSize, textAnchor, textAlignment, SortingOrfer);
        }

        public static TextMesh CreateWorldText(Color color, string Text, Transform parent, Vector3 localPosition, int fontSize, TextAnchor textAnchor, TextAlignment textAlignment, int sortingOrder)
        {
            GameObject gameObject = new GameObject("World_Text", typeof(TextMesh));

            Transform transform = gameObject.transform;
            transform.parent = parent;
            transform.localPosition = localPosition;

            TextMesh textMesh = transform.GetComponent<TextMesh>();
            textMesh.anchor = textAnchor;
            textMesh.alignment = textAlignment;
            textMesh.text = Text;
            textMesh.fontSize = fontSize;
            textMesh.color = color;
            textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;

            return textMesh;
        }

        public static Vector3 QuadraticBezier2D(Vector3 a, Vector3 b, Vector3 c, float t)
        {
            Vector3 quadraticBezier = QuadraticBezier(a, b, c, t);
            quadraticBezier.z = 0;
            return quadraticBezier;
        }

        public static Vector3 QuadraticBezier2D(Vector3[] pts, float t)
        {
            return QuadraticBezier2D(pts[0], pts[1], pts[2], t);
        }

        public static Vector3 CubicBezier2D(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
        {
            Vector3 cubicBezier = CubicBezier(a, b, c, d, t);
            cubicBezier.z = 0;
            return cubicBezier;
        }

        public static Vector3 CubicBezier2D(Vector3[] pts, float t)
        {
            return CubicBezier2D(pts[0], pts[1], pts[2], pts[3], t);
        }

        public static Vector3 CubicTangent2D(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
        {
            Vector3 tangent = CubicTangent(a, b, c, d, t);
            return new Vector3(tangent.x, tangent.y);
        }

        public static Vector3 CubicTangent2D(Vector3[] pts, float t)
        {
            return CubicTangent2D(pts[0], pts[1], pts[2], pts[3], t);
        }

        public static Vector3 CubicNormal2D(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
        {
            Vector3 tangent = CubicTangent2D(a, b, c, d, t);
            return new Vector3(-tangent.y, tangent.x);
        }

        public static Vector3 CubicNormal2D(Vector3[] pts, float t)
        {
            return CubicNormal2D(pts[0], pts[1], pts[2], pts[3], t);
        }

        public static Quaternion CubicOrientation2D(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
        {
            Vector3 tangent = CubicTangent2D(a, b, c, d, t);
            Vector3 normal = CubicNormal2D(a, b, c, d, t);
            return Quaternion.LookRotation(tangent, normal);
        }

        public static Quaternion CubicOrientation2D(Vector3[] pts, float t)
        {
            return CubicOrientation2D(pts[0], pts[1], pts[2], pts[3], t);
        }

        public static Vector3 QuadraticBezier(Vector3 a, Vector3 b, Vector3 c, float t)
        {
            float tSqr = t * t;
            float ts = 2 * t;
            return tSqr * a - 2 * tSqr * b + tSqr * c - ts * a + ts * b + a;
        }

        public static Vector3 QuadraticBezier(Vector3[] pts, float t)
        {
            return QuadraticBezier(pts[0], pts[1], pts[2], t);
        }

        public static Vector3 CubicBezier(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
        {
            float omt = 1f - t;
            float omtSqr = omt * omt;
            float tSqr = t * t;
            return a * (omtSqr * omt) + b * (3f * omtSqr * t) + c * (3f * omt * tSqr) + d * (tSqr * t);
        }

        public static Vector3 CubicBezier(Vector3[] pts, float t)
        {
            return CubicBezier(pts[0], pts[1], pts[2], pts[3], t);
        }

        public static Vector3 CubicTangent(Vector3 a, Vector3 b, Vector3 c, Vector3 d, float t)
        {
            float omt = 1f - t;
            float omtSqr = omt * omt;
            float tSqr = t * t;

            Vector3 tangent = a * (-omtSqr) +
                             b * (3f * omtSqr - 2 * omt) +
                             c * (-3f * tSqr + 2 * t) +
                             d * tSqr;

            return tangent.normalized;
        }

        public static Vector3 CubicTangent(Vector3[] pts, float t)
        {
            return CubicTangent(pts[0], pts[1], pts[2], pts[3], t);
        }

        public static Vector3 CubicNormal(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 up, float t)
        {
            Vector3 tangent = CubicTangent(a, b, c, d, t);
            Vector3 biNormal = Vector3.Cross(up, tangent).normalized;
            return Vector3.Cross(tangent, biNormal);
        }

        public static Vector3 CubicNormal(Vector3[] pts, Vector3 up, float t)
        {
            return CubicNormal(pts[0], pts[1], pts[2], pts[3], up, t);
        }

        public static Quaternion CubicOrientation(Vector3 a, Vector3 b, Vector3 c, Vector3 d, Vector3 up, float t)
        {
            Vector3 tangent = CubicTangent(a, b, c, d, t);
            Vector3 normal = CubicNormal(a, b, c, d, up, t);
            return Quaternion.LookRotation(tangent, normal);
        }

        public static Quaternion CubicOrientation(Vector3[] pts, Vector3 up, float t)
        {
            return CubicOrientation(pts[0], pts[1], pts[2], pts[3], up, t);
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
    }
}
