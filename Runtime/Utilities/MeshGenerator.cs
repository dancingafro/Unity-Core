using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreScript.Utility
{
    public static class MeshGenerator
    {
        public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve _heightCurve, int levelOfDetail)
        {
            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);

            int meshSimplificationIncrement = (levelOfDetail == 0 ? 1 : levelOfDetail * 2);
            int verticesPerLine = (width - 1) / meshSimplificationIncrement + 1;

            Vector2 topLeft = new Vector2((width - 1) * .5f, (height - 1) * .5f);

            MeshData meshData = new MeshData(verticesPerLine, verticesPerLine);

            AnimationCurve heightCurve = new AnimationCurve(_heightCurve.keys);

            int vertexIndex = 0;
            for (int x = 0; x < width; x += meshSimplificationIncrement)
            {
                for (int y = 0; y < height; y += meshSimplificationIncrement)
                {
                    meshData.vertices[vertexIndex] = new Vector3(topLeft.x - x, heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier, topLeft.y - y);
                    meshData.uv[vertexIndex] = new Vector2(x / (float)width, y / (float)height);

                    if (x < width - 1 && y < height - 1)
                    {
                        meshData.AddTriangles(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
                        meshData.AddTriangles(vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);
                    }

                    ++vertexIndex;
                }
            }

            return meshData;
        }
    }

    public class MeshData
    {
        public Vector3[] vertices;
        public int[] triangles;
        public Vector2[] uv;

        int triangleIndex;

        public MeshData(int width, int height)
        {
            vertices = new Vector3[width * height];
            uv = new Vector2[width * height];
            triangles = new int[(width - 1) * (height - 1) * 6];
            triangleIndex = 0;
        }

        public void AddTriangles(int a, int b, int c)
        {
            triangles[triangleIndex] = a;
            triangles[triangleIndex + 1] = b;
            triangles[triangleIndex + 2] = c;
            triangleIndex += 3;
        }
        public Mesh CreateMesh()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;
            mesh.RecalculateNormals();
            return mesh;
        }
    }
}