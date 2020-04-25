using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using UnityEngine;
using CoreScript.Factories;
using CoreScript.Utility;

namespace CoreScript.Procidual
{
    public class MapGenerator : MonoBehaviour
    {
        public enum Drawmode
        {
            NoiseMap,
            ColorMap,
            Mesh,
            FalloffMap
        }
        public Drawmode drawmode;
        public Noise.NormalizeMode normalizeMode;

        public const int mapChunkSize = 239;

        [Range(0, 6)]
        public int editorPreviewLOD;

        public float noiseScale = 0.3f;

        public int octaves = 0;
        [Range(0, 1)]
        public float persistance = 0;
        public float lacunarity = 0;

        public int seed = 0;
        public Vector2 offset = Vector2.zero;

        public bool useFalloffMap = false;

        public float meshHeightMultiplier = 1;
        public AnimationCurve heightCurve;

        public bool autoUpdate = false;

        public CustomColourGradient regions;

        float[,] falloffMap;

        Queue<ThreadInfo<MapData>> mapThreadInfoQueue = new Queue<ThreadInfo<MapData>>();
        Queue<ThreadInfo<MeshData>> meshThreadInfoQueue = new Queue<ThreadInfo<MeshData>>();

        void Awake()
        {
            falloffMap = Noise.GenerateFalloffMap(mapChunkSize + 2);
        }

        public void DrawMapInEditor()
        {
            MapData mapData = GenerateMapData(Vector2.zero);
            MapDisplay display = FindObjectOfType<MapDisplay>();

            switch (drawmode)
            {
                case Drawmode.ColorMap:
                    display.DrawTexture(UtilityClass.TextureFromColors(mapData.colourMap, mapChunkSize, mapChunkSize));
                    break;
                case Drawmode.Mesh:
                    display.DrawMesh(Factory.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, heightCurve, editorPreviewLOD), UtilityClass.TextureFromColors(mapData.colourMap, mapChunkSize, mapChunkSize));
                    break;
                case Drawmode.FalloffMap:
                    display.DrawTexture(UtilityClass.TextureFromHeight(Noise.GenerateFalloffMap(mapChunkSize + 2)));
                    break;
                default:
                    display.DrawTexture(UtilityClass.TextureFromHeight(mapData.heightMap));
                    break;
            }
        }

        public void RequestMapData(Vector2 centre, Action<MapData> callBack)
        {
            new Thread((ThreadStart)delegate
            {
                MapDataThread(centre, callBack);
            }).Start();
        }

        void MapDataThread(Vector2 centre, Action<MapData> callBack)
        {
            MapData mapData = GenerateMapData(centre);
            lock (mapThreadInfoQueue)
                mapThreadInfoQueue.Enqueue(new ThreadInfo<MapData>(callBack, mapData));
        }

        public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callBack)
        {
            new Thread((ThreadStart)delegate
            {
                MeshDataThread(mapData, lod, callBack);
            }).Start();
        }

        void MeshDataThread(MapData mapData, int lod, Action<MeshData> callBack)
        {
            MeshData meshData = Factory.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, heightCurve, lod);
            lock (meshThreadInfoQueue)
                meshThreadInfoQueue.Enqueue(new ThreadInfo<MeshData>(callBack, meshData));
        }

        void Update()
        {
            while (mapThreadInfoQueue.Count > 0)
                mapThreadInfoQueue.Dequeue().Invoke();

            while (meshThreadInfoQueue.Count > 0)
                meshThreadInfoQueue.Dequeue().Invoke();
        }

        MapData GenerateMapData(Vector2 centre)
        {
            float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize + 2, mapChunkSize + 2, seed, noiseScale, octaves, persistance, lacunarity, centre + offset, normalizeMode);

            Color[] colors = new Color[mapChunkSize * mapChunkSize];

            for (int x = 0; x < mapChunkSize; ++x)
            {
                for (int y = 0; y < mapChunkSize; ++y)
                {
                    if (useFalloffMap)
                        noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - falloffMap[x, y]);

                    colors[y * mapChunkSize + x] = regions.Evaluate(Mathf.InverseLerp(0, 1, noiseMap[x, y]));
                }
            }
            return new MapData(noiseMap, colors);
        }

        private void OnValidate()
        {
            if (lacunarity < 1)
                lacunarity = 1;
            if (meshHeightMultiplier < 1)
                meshHeightMultiplier = 1;
            if (octaves < 0)
                octaves = 0;
            if (useFalloffMap)
                falloffMap = Noise.GenerateFalloffMap(mapChunkSize + 2);
        }

        struct ThreadInfo<T>
        {
            public readonly Action<T> callBack;
            public readonly T Parameter;

            public ThreadInfo(Action<T> callBack, T parameter)
            {
                this.callBack = callBack;
                Parameter = parameter;
            }

            public void Invoke()
            {
                callBack(Parameter);
            }
        }
    }

    public struct MapData
    {
        public readonly float[,] heightMap;
        public readonly Color[] colourMap;

        public MapData(float[,] heightMap, Color[] colourMap)
        {
            this.heightMap = heightMap;
            this.colourMap = colourMap;
        }

    }
}