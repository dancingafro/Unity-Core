using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using UnityEngine;
using CoreScript.Utility;

namespace CoreScript.Procidual
{
    public class MapGenerator : MonoBehaviour
    {
        public enum Drawmode
        {
            NoiseMap,
            ColorMap,
            Mesh
        }

        public Drawmode drawmode;

        public const int mapChunkSize = 241;

        [Range(0, 6)]
        public int levelOfDetail;

        public float noiseScale = 0.3f;

        public int octaves = 0;
        [Range(0, 1)]
        public float persistance = 0;
        public float lacunarity = 0;

        public int seed = 0;
        public Vector2 offset = Vector2.zero;

        public float meshHeightMultiplier = 1;
        public AnimationCurve heightCurve;

        public bool autoUpdate = false;

        public CustomGradient regions;

        Queue<ThreadInfo<MapData>> mapThreadInfoQueue = new Queue<ThreadInfo<MapData>>();
        Queue<ThreadInfo<MeshData>> meshThreadInfoQueue = new Queue<ThreadInfo<MeshData>>();

        public void DrawMapInEditor()
        {
            MapData mapData = GenerateMapData();
            MapDisplay display = FindObjectOfType<MapDisplay>();
            Texture2D texture;

            switch (drawmode)
            {
                case Drawmode.ColorMap:
                    texture = UtilityCode.TextureFromColors(mapData.colourMap, mapChunkSize, mapChunkSize);
                    display.DrawTexture(texture);
                    break;
                case Drawmode.Mesh:
                    display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, heightCurve, levelOfDetail), UtilityCode.TextureFromColors(mapData.colourMap, mapChunkSize, mapChunkSize));
                    break;
                default:
                    texture = UtilityCode.TextureFromHeight(mapData.heightMap);
                    display.DrawTexture(texture);
                    break;
            }
        }

        public void RequestMapData(Action<MapData> callBack)
        {
            new Thread((ThreadStart)delegate
            {
                MapDataThread(callBack);
            }).Start();
        }

        void MapDataThread(Action<MapData> callBack)
        {
            MapData mapData = GenerateMapData();
            lock (mapThreadInfoQueue)
                mapThreadInfoQueue.Enqueue(new ThreadInfo<MapData>(callBack, mapData));
        }

        public void RequestMeshData(MapData mapData, Action<MeshData> callBack)
        {
            new Thread((ThreadStart)delegate
            {
                MeshDataThread(mapData, callBack);
            }).Start();
        }

        void MeshDataThread(MapData mapData, Action<MeshData> callBack)
        {
            MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heightMap, meshHeightMultiplier, heightCurve, levelOfDetail);
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

        MapData GenerateMapData()
        {
            float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset);

            Color[] colors = new Color[mapChunkSize * mapChunkSize];

            for (int x = 0; x < mapChunkSize; ++x)
                for (int y = 0; y < mapChunkSize; ++y)
                    colors[y * mapChunkSize + x] = regions.Evaluate(Mathf.InverseLerp(0, 1, noiseMap[x, y]));
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