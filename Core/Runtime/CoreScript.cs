﻿using UnityEngine;

namespace CoreScript
{
    public enum PathSpace { xyz, xy, xz };

    public class MinMax3D
    {

        public Vector3 Min { get; private set; }
        public Vector3 Max { get; private set; }

        public MinMax3D()
        {
            Min = Vector3.one * float.MaxValue;
            Max = Vector3.one * float.MinValue;
        }

        public void AddValue(Vector3 v)
        {
            Min = new Vector3(Mathf.Min(Min.x, v.x), Mathf.Min(Min.y, v.y), Mathf.Min(Min.z, v.z));
            Max = new Vector3(Mathf.Max(Max.x, v.x), Mathf.Max(Max.y, v.y), Mathf.Max(Max.z, v.z));
        }
    }

    public class MinMax2D
    {

        public Vector2 Min { get; private set; }
        public Vector2 Max { get; private set; }

        public MinMax2D()
        {
            Min = Vector2.one * float.MaxValue;
            Max = Vector2.one * float.MinValue;
        }

        public void AddValue(Vector2 v)
        {
            Min = new Vector2(Mathf.Min(Min.x, v.x), Mathf.Min(Min.y, v.y));
            Max = new Vector2(Mathf.Max(Max.x, v.x), Mathf.Max(Max.y, v.y));
        }
    }

    public class MinMax
    {
        public float Min { get; private set; }
        public float Max { get; private set; }

        public MinMax()
        {
            Min = float.MaxValue;
            Max = float.MinValue;
        }

        public void AddValue(float v)
        {
            Min = Mathf.Min(Min, v);
            Max = Mathf.Max(Max, v);
        }
    }
}