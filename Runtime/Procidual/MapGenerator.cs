using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreScript.Utility;

namespace CoreScript.Procidual
{
    public class MapGenerator : MonoBehaviour
    {
        public int width = 0, height = 0;
        public float noiseScale = 0.0001f;
        public bool autoUpdate = false;

        public void GenerateMap()
        {
            float[,] noiseMap = Noise.GenerateNoiseMap(width, height, noiseScale);

            MapDisplay mapDisplay = FindObjectOfType<MapDisplay>();
            mapDisplay.DrawNoiseMap(noiseMap);

        }
    }
}