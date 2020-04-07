﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomScript.Utility
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

    }
}
