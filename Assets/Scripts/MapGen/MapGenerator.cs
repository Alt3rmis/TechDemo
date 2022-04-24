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

    Vector3 interestingPoints;
    bool hasGenerated = false;
    WaypointsGenerator wg;
    void Start()
    {
        
        GenerateMap();
    }
    public void GenerateMap()
    {
        wg = GetComponent<WaypointsGenerator>();
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
        float maxHeight = float.MinValue;
        Color[] colourMap = new Color[mapChunkSize*mapChunkSize];
        for (int y=0; y<mapChunkSize; y++){
            for (int x=0; x<mapChunkSize; x++){
                float currentHeight = noiseMap [x,y];
                for (int i=0; i < regions.Length; i++){
                    if (currentHeight > maxHeight){
                        maxHeight = currentHeight;
                        interestingPoints = new Vector3(x,currentHeight,y);
                    }
                    if (currentHeight <= regions[i].height){
                        colourMap [y*mapChunkSize+x] = regions[i].colour;
                        break;
                    }
                }
            }
        }
        hasGenerated = true;
        // Debug.Log("interesting points: " + interestingPoints);
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
        wg.GenerateWaypoints();
    }

    public Vector3 GetInterestingPoints()
    {
        if(hasGenerated)
        {
            return interestingPoints;
        }
        else
        {
            return new Vector2(0f,0f);
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
