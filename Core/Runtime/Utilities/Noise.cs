using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreScript.Utility
{
    public static class Noise
    {
        public enum NormalizeMode
        {
            Local,
            Global
        }

        public static float[,] GenerateNoiseMap(int width, int height, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, NormalizeMode normalizeMode)
        {
            float[,] noiseMap = new float[width, height];

            System.Random prng = new System.Random(seed);
            Vector2[] octavesOffsets = new Vector2[octaves];

            float maxPossibleHeight = 0, amplitude = 1, frequency = 1;

            for (int i = 0; i < octaves; i++)
            {
                octavesOffsets[i] = new Vector2(prng.Next(-100000, 100000), prng.Next(-100000, 100000)) - offset;
                maxPossibleHeight += amplitude;
                amplitude *= persistance;
            }

            if (scale <= 0)
                scale = 0.0001f;

            float maxLocalNoiseHeight = float.MinValue, minLocalNoiseHeight = float.MaxValue;

            float halfWidth = width * .5f;
            float halfHeight = height * .5f;

            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    amplitude = 1;
                    frequency = 1;
                    float noiseHeight = 0;

                    for (int i = 0; i < octaves; i++)
                    {
                        float sampleX = (x - halfWidth + octavesOffsets[i].x) / scale * frequency;
                        float sampleY = (y - halfHeight + octavesOffsets[i].y) / scale * frequency;

                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                        noiseHeight += perlinValue * amplitude;

                        amplitude *= persistance;
                        frequency *= lacunarity;
                    }

                    if (minLocalNoiseHeight > noiseHeight)
                        minLocalNoiseHeight = noiseHeight;
                    else if (maxLocalNoiseHeight < noiseHeight)
                        maxLocalNoiseHeight = noiseHeight;

                    noiseMap[x, y] = noiseHeight;
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    switch (normalizeMode)
                    {
                        case NormalizeMode.Global:
                            noiseMap[x, y] = Mathf.Clamp((noiseMap[x, y] + 1) / maxPossibleHeight, 0, int.MaxValue);
                            break;
                        default:
                            noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                            break;
                    }
                }
            }

            return noiseMap;
        }

        public static float[,] GenerateFalloffMap(int size)
        {
            float[,] map = new float[size, size];

            for (int i = 0; i < size; ++i)
                for (int j = 0; j < size; ++j)
                    map[i, j] = Evaluate(Mathf.Max(Mathf.Abs(i / (float)size * 2 - 1), Mathf.Abs(j / (float)size * 2 - 1)));

            return map;
        }

        public static float[,] GenerateCircularFilterMap(int size, float radius, float falloffOffset = .01f)
        {
            float[,] map = new float[size, size];

            Vector3 center = new Vector3(size * .5f, size * .5f);
            float radiusSqr = radius * radius;
            float maxDisSqr = (new Vector3(size, size) - center).sqrMagnitude;
            for (int i = 0; i < size; ++i)
            {
                for (int j = 0; j < size; ++j)
                {
                    Vector3 relative = new Vector3(i, j) - center;
                    map[i, j] = Mathf.Clamp01((Mathf.Max((Mathf.Max(relative.sqrMagnitude - radiusSqr, 0) / maxDisSqr) / falloffOffset, 0) - 1f) * -1f);
                }
            }

            return map;
        }

        static float Evaluate(float value)
        {
            float a = 3, b = 2.2f;
            return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
        }
    }
}