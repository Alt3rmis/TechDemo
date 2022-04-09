using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MapGenerator
{
    public class MapGenerator : MonoBehaviour
    {
        public int mapWidth;
        public int mapHeight;
        public float noiseScale;

        public bool autoUpdate;

        public void GenerateMap() {
            float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth,mapHeight,noiseScale);

            // process this noise map to generate terrain map

            MapDisplay display = FindObjectOfType<MapDisplay> ();
            display.DrawNoiseMap(noiseMap);
        }
    }
}

