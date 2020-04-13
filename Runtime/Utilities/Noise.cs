﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreScript.Utility
{
    public static class Noise
    {
        public static float[,] GenerateNoiseMap(int width, int height, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
        {
            float[,] noiseMap = new float[width, height];

            System.Random prng = new System.Random(seed);
            Vector2[] octavesOffsets = new Vector2[octaves];

            for (int i = 0; i < octaves; i++)
                octavesOffsets[i] = new Vector2(prng.Next(-100000, 100000), prng.Next(-100000, 100000)) + offset;


            if (scale <= 0)
                scale = 0.0001f;

            float maxNoiseHeight = float.MinValue, minNoiseHeight = float.MaxValue;

            float halfWidth = width * .5f;
            float halfHeight = height * .5f;

            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    float amplitude = 1, frequency = 1, noiseHeight = 0;

                    for (int i = 0; i < octaves; i++)
                    {
                        float sampleX = (x - halfWidth) / scale * frequency + octavesOffsets[i].x;
                        float sampleY = (y - halfHeight) / scale * frequency + octavesOffsets[i].y;

                        float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                        noiseHeight += perlinValue * amplitude;

                        amplitude *= persistance;
                        frequency *= lacunarity;
                    }

                    if (minNoiseHeight > noiseHeight)
                        minNoiseHeight = noiseHeight;
                    else if (maxNoiseHeight < noiseHeight)
                        maxNoiseHeight = noiseHeight;

                    noiseMap[x, y] = noiseHeight;
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
                }
            }

            return noiseMap;
        }
    }
}