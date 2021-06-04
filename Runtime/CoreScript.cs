using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreScript
{
    public enum PositionSpace { xyz, xy, xz, yz }
    public enum PathSpace { xyz, xy, xz };
    public enum BlendMode { Linear, Discrete }

    public class MinMax4D
    {
        public Vector4 Min { get; private set; }
        public Vector4 Max { get; private set; }

        public MinMax4D()
        {
            Min = Vector4.one * float.MaxValue;
            Max = Vector4.one * float.MinValue;
        }

        public void AddValue(Vector4 v)
        {
            Min = new Vector4(Mathf.Min(Min.x, v.x), Mathf.Min(Min.y, v.y), Mathf.Min(Min.z, v.z), Mathf.Min(Min.w, v.w));
            Max = new Vector4(Mathf.Max(Max.x, v.x), Mathf.Max(Max.y, v.y), Mathf.Max(Max.z, v.z), Mathf.Max(Max.w, v.w));
        }
    }

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

    public class MinMax3DInt
    {
        public Vector3Int Min { get; private set; }
        public Vector3Int Max { get; private set; }

        public MinMax3DInt()
        {
            Min = Vector3Int.one * int.MaxValue;
            Max = Vector3Int.one * int.MinValue;
        }

        public void AddValue(Vector3Int v)
        {
            Min = new Vector3Int(Mathf.Min(Min.x, v.x), Mathf.Min(Min.y, v.y), Mathf.Min(Min.z, v.z));
            Max = new Vector3Int(Mathf.Max(Max.x, v.x), Mathf.Max(Max.y, v.y), Mathf.Max(Max.z, v.z));
        }
    }

    public class MinMax2DInt
    {
        public Vector2Int Min { get; private set; }
        public Vector2Int Max { get; private set; }

        public MinMax2DInt()
        {
            Min = Vector2Int.one * int.MaxValue;
            Max = Vector2Int.one * int.MinValue;
        }

        public void AddValue(Vector2Int v)
        {
            Min = new Vector2Int(Mathf.Min(Min.x, v.x), Mathf.Min(Min.y, v.y));
            Max = new Vector2Int(Mathf.Max(Max.x, v.x), Mathf.Max(Max.y, v.y));
        }
    }

    public class MinMaxInt
    {
        public int Min { get; private set; }
        public int Max { get; private set; }

        public MinMaxInt()
        {
            Min = int.MaxValue;
            Max = int.MinValue;
        }

        public void AddValue(int v)
        {
            Min = Mathf.Min(Min, v);
            Max = Mathf.Max(Max, v);
        }
    }
}