using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreScript.Utility
{
    [RequireComponent(typeof(PathCreator))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class RoadMeshCreator : MonoBehaviour
    {
        [Range(.05f, 1.5f)]
        public float spacing = 1f;
        public float scale = 1;

        public bool autoUpdate = false;

        public float tiling = 1;

        PathCreator pathCreator;
        MeshFilter meshFilter;
        MeshRenderer meshRenderer;

        PathCreator PathCreator
        {
            get
            {
                if (pathCreator == null)
                    pathCreator = GetComponent<PathCreator>();
                return pathCreator;
            }
        }

        MeshFilter MeshFilter
        {
            get
            {
                if (meshFilter == null)
                    meshFilter = GetComponent<MeshFilter>();
                return meshFilter;
            }
        }

        MeshRenderer MeshRenderer
        {
            get
            {
                if (meshRenderer == null)
                    meshRenderer = GetComponent<MeshRenderer>();
                return meshRenderer;
            }
        }

        public void UpdateRoad()
        {
            Path path = PathCreator.path;
            OrientedPoint[] orientedPoints = path.CalculateEvenlySpacedPoints(spacing);
            MeshFilter.mesh = CreateRoadMesh(orientedPoints, path.IsClosed);
            int textureRepeat = Mathf.RoundToInt(tiling * orientedPoints.Length * spacing * .05f);
            MeshRenderer.sharedMaterial.mainTextureScale = new Vector2(1, textureRepeat);
        }

        Mesh CreateRoadMesh(OrientedPoint[] points, bool isClosed)
        {
            Vector3[] verts = new Vector3[points.Length * 2];
            Vector2[] uvs = new Vector2[verts.Length];
            int numTris = 2 * (points.Length - 1) + (isClosed ? 2 : 0);
            int[] tris = new int[numTris * 3];
            int vertIndex = 0;
            int triIndex = 0;

            for (int i = 0; i < points.Length; ++i)
            {
                Vector3 forward = Vector3.zero;

                if (i < points.Length - 1 || isClosed)
                    forward += points[(i + 1) % points.Length].Position - points[i].Position;
                if (i > 0)
                    forward += points[i].Position - points[(i - 1 + points.Length) % points.Length].Position;

                forward.Normalize();
                Vector3 left = new Vector2(-forward.y, forward.x);

                verts[vertIndex] = points[i].Position + left * scale * .5f;
                verts[vertIndex + 1] = points[i].Position - left * scale * .5f;

                float completionPercent = i / (float)(points.Length - 1);
                float v = 1 - Mathf.Abs(2 * completionPercent - 1);
                uvs[vertIndex] = new Vector2(0, v);
                uvs[vertIndex + 1] = new Vector2(1, v);

                if (i < points.Length - 1 || isClosed)
                {
                    tris[triIndex] = vertIndex;
                    tris[triIndex + 1] = (vertIndex + 2) % verts.Length;
                    tris[triIndex + 2] = vertIndex + 1;
                    tris[triIndex + 3] = vertIndex + 1;
                    tris[triIndex + 4] = (vertIndex + 2) % verts.Length;
                    tris[triIndex + 5] = (vertIndex + 3) % verts.Length;
                }

                vertIndex += 2;
                triIndex += 6;
            }

            return new Mesh
            {
                vertices = verts,
                triangles = tris,
                uv = uvs
            };
        }

        public struct ExtrudeShape
        {
            public Vector2[] verts;
            public Vector2[] normals;
            public Vector2[] uvs;
        }
    }
}