using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{

    public enum DrawMode {NoiseMap, ColourMap, Mesh};
    public DrawMode drawMode;

    // public const int mapChunkSize = 241;
    public const int mapChunkSize = 241;
    [Range(0, 6)]
    public int levelOfDetail;
    public float noiseScale;

    public int octaves;
    [Range(0,1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public bool crossover;
    public bool smooth;
    public int crossoverChunk;

    public float waterPercentage;
    public float landPercentage;
    public float mountainPercentage;
    public float snowPercentage;

    public int generations = 100;
    public int parentNumber = 30;

    public float meshHeightMultiplier;
    public AnimationCurve meshHeightCurve;

    public bool autoUpdate;

    public TerrainType[] regions;

    void Start()
    {
        GenerateMap();
    }
    public void GenerateMap()
    {
        // Get noise map
        // Replace noise map with evolution algorithm generator
        // float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset);
        /*float[,] noiseMap = Evolution.OnePointCrossover(Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset),
        Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed+1, noiseScale, octaves, persistance, lacunarity, offset));
        */
        float[,] noiseMap;
        if(crossover){
            noiseMap = Evolution.SquareCrossover(Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset),
        Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed+1, noiseScale, octaves, persistance, lacunarity, offset), crossoverChunk, smooth)[1];
            //noiseMap = Evolution.Evolve(generations, parentNumber, mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset, waterPercentage, landPercentage, mountainPercentage, snowPercentage, smooth);
        } else {
            noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octaves, persistance, lacunarity, offset);
        }

        
        // Get colour map
        Color[] colourMap = new Color[mapChunkSize*mapChunkSize];
        for (int y=0; y<mapChunkSize; y++){
            for (int x=0; x<mapChunkSize; x++){
                float currentHeight = noiseMap [x,y];
                for (int i=0; i < regions.Length; i++){
                    if (currentHeight <= regions[i].height){
                        colourMap [y*mapChunkSize+x] = regions[i].colour;
                        break;
                    }
                }
            }
        }
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode==DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        }
        else if (drawMode == DrawMode.ColourMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapChunkSize, mapChunkSize));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail), TextureGenerator.TextureFromColourMap(colourMap, mapChunkSize, mapChunkSize));
        }
        
    }

    void OnValidate() {
        // if (mapChunkSize < 1) {
        //     mapChunkSize = 1;
        // }
        // if (mapChunkSize < 1) {
        //     mapChunkSize = 1;
        // }
        if (lacunarity < 1) {
            lacunarity = 1;
        }
        if (octaves < 0){
            octaves = 0;
        }
    }
}

[System.Serializable]
public struct  TerrainType
{
    public string name;
    public float height;
    public Color colour;
}
