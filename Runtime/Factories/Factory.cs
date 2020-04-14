using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreScript.Factories
{
    public static class Factory
    {
        public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve _heightCurve, int levelOfDetail)
        {
            AnimationCurve heightCurve = new AnimationCurve(_heightCurve.keys);

            int meshSimplificationIncrement = (levelOfDetail == 0 ? 1 : levelOfDetail * 2);

            int borderSize = heightMap.GetLength(0);
            int meshSize = borderSize - 2 * meshSimplificationIncrement;
            int meshSizeUnsimplified = borderSize - 2;

            int verticesPerLine = (meshSize - 1) / meshSimplificationIncrement + 1;

            Vector2 topLeft = new Vector2((meshSizeUnsimplified - 1) * .5f, (meshSizeUnsimplified - 1) * .5f);

            MeshData meshData = new MeshData(verticesPerLine);

            int[,] vertexIndicesMap = new int[borderSize, borderSize];
            int meshVertexIndex = 0;
            int borderVertexIndex = -1;

            for (int x = 0; x < borderSize; x += meshSimplificationIncrement)
            {
                for (int y = 0; y < borderSize; y += meshSimplificationIncrement)
                {
                    if (y == 0 || y == borderSize - 1 || x == 0 || x == borderSize - 1)
                    {
                        vertexIndicesMap[x, y] = borderVertexIndex--;
                        continue;
                    }

                    vertexIndicesMap[x, y] = meshVertexIndex++;
                }
            }

            for (int x = 0; x < borderSize; x += meshSimplificationIncrement)
            {
                for (int y = 0; y < borderSize; y += meshSimplificationIncrement)
                {
                    int vertexIndex = vertexIndicesMap[x, y];
                    Vector2 percent = new Vector2((x - meshSimplificationIncrement) / (float)meshSize, (y - meshSimplificationIncrement) / (float)meshSize);
                    Vector3 vertexPos = new Vector3(topLeft.x - percent.x * meshSizeUnsimplified, heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier, topLeft.y - percent.y * meshSizeUnsimplified);

                    meshData.AddVertex(vertexPos, percent, vertexIndex);

                    if (x < borderSize - 1 && y < borderSize - 1)
                    {
                        int a = vertexIndicesMap[x, y];
                        int b = vertexIndicesMap[x + meshSimplificationIncrement, y];
                        int c = vertexIndicesMap[x, y + meshSimplificationIncrement];
                        int d = vertexIndicesMap[x + meshSimplificationIncrement, y + meshSimplificationIncrement];
                        meshData.AddTriangles(a, c, d);
                        meshData.AddTriangles(b, a, d);
                    }

                    ++vertexIndex;
                }
            }

            return meshData;
        }

        public static float[,] GenerateFalloffMap(int size)
        {
            float[,] map = new float[size, size];

            for (int i = 0; i < size; ++i)
                for (int j = 0; j < size; ++j)
                    map[i, j] = Evaluate(Mathf.Max(Mathf.Abs(i / (float)size * 2 - 1), Mathf.Abs(j / (float)size * 2 - 1)));

            return map;
        }

        static float Evaluate(float value)
        {
            float a = 3;
            float b = 2.2f;

            return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
        }
    }

    public class MeshData
    {
        Vector3[] vertices;
        int[] triangles;
        Vector2[] uv;

        Vector3[] borderVertices;
        int[] borderTriangle;

        int triangleIndex;
        int borderTriangleIndex;

        public MeshData(int verticesPerline)
        {
            vertices = new Vector3[verticesPerline * verticesPerline];
            uv = new Vector2[verticesPerline * verticesPerline];
            triangles = new int[(verticesPerline - 1) * (verticesPerline - 1) * 6];

            borderVertices = new Vector3[verticesPerline * 4 + 4];
            borderTriangle = new int[verticesPerline * 24];

            triangleIndex = 0;
        }

        public void AddVertex(Vector3 vertexPos, Vector2 uv, int vertexIndex)
        {
            if (vertexIndex < 0)
            {
                borderVertices[-vertexIndex - 1] = vertexPos;
                return;
            }

            vertices[vertexIndex] = vertexPos;
            this.uv[vertexIndex] = uv;
        }

        public void AddTriangles(int a, int b, int c)
        {
            if (a < 0 || b < 0 || c < 0)
            {
                borderTriangle[borderTriangleIndex] = a;
                borderTriangle[borderTriangleIndex + 1] = b;
                borderTriangle[borderTriangleIndex + 2] = c;
                borderTriangleIndex += 3;
                return;
            }
            triangles[triangleIndex] = a;
            triangles[triangleIndex + 1] = b;
            triangles[triangleIndex + 2] = c;
            triangleIndex += 3;
        }

        Vector3[] CalculateNormals()
        {
            Vector3[] normals = new Vector3[vertices.Length];

            for (int i = 0; i < triangles.Length; i += 3)
            {
                int vertexIndexA = triangles[i];
                int vertexIndexB = triangles[i + 1];
                int vertexIndexC = triangles[i + 2];

                Vector3 surfaceNormal = SurfaceNormalFromIndices(vertices[vertexIndexA], vertices[vertexIndexB], vertices[vertexIndexC]);

                normals[vertexIndexA] += surfaceNormal;
                normals[vertexIndexB] += surfaceNormal;
                normals[vertexIndexC] += surfaceNormal;
            }

            for (int i = 0; i < borderTriangle.Length; i += 3)
            {
                int vertexIndexA = borderTriangle[i];
                int vertexIndexB = borderTriangle[i + 1];
                int vertexIndexC = borderTriangle[i + 2];

                Vector3 surfaceNormal = SurfaceNormalFromIndices((vertexIndexA < 0) ? borderVertices[-vertexIndexA - 1] : vertices[vertexIndexA],
                                                                 (vertexIndexB < 0) ? borderVertices[-vertexIndexB - 1] : vertices[vertexIndexB],
                                                                 (vertexIndexC < 0) ? borderVertices[-vertexIndexC - 1] : vertices[vertexIndexC]);
                if (vertexIndexA >= 0)
                    normals[vertexIndexA] += surfaceNormal;
                if (vertexIndexB >= 0)
                    normals[vertexIndexB] += surfaceNormal;
                if (vertexIndexC >= 0)
                    normals[vertexIndexC] += surfaceNormal;
            }

            for (int i = 0; i < normals.Length; i++)
                normals[i].Normalize();

            return normals;
        }

        Vector3 SurfaceNormalFromIndices(Vector3 vertexA, Vector3 vertexB, Vector3 vertexC)
        {
            return Vector3.Cross(vertexB - vertexA, vertexC - vertexA).normalized;
        }

        public Mesh CreateMesh()
        {
            Mesh mesh = new Mesh
            {
                vertices = vertices,
                triangles = triangles,
                uv = uv,
                normals = CalculateNormals()
            };

            return mesh;
        }
    }
}