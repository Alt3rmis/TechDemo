using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Evolution
{
    public static float[,] MapCrossover(float[,] map1, float[,] map2) {
        int map1Width = map1.GetLength(0);
        int map1Height = map1.GetLength(1);
        int map2Width = map2.GetLength(0);
        int map2Height = map2.GetLength(1);

        float[] flattenMap1 = MapFlatter(map1);
        float[] flattenMap2 = MapFlatter(map2);

        float[] flattenMap = new float[map1Width*map1Height];

        int crossoverPoint = (map1Width * map1Height)/2;
        for(int y=0; y<map1Height; y++){
            for(int x=0; x<map2Width; x++){
                if(y*map1Width + x < crossoverPoint){
                    flattenMap[y*map1Width + x] = flattenMap1[y*map1Width + x];
                } else {
                    flattenMap[y*map1Width + x] = flattenMap2[y*map1Width + x];
                }
            }
        }
        return MapReconstructor(flattenMap, map1Width, map1Height);
    }

    private static float[] MapFlatter(float[,] map){
        int width = map.GetLength(0);
        int height = map.GetLength(1);
        float[] flattenMap = new float[width*height];
        for(int y=0; y<height; y++){
            for(int x=0; x<width; x++){
                flattenMap[y*width+x] = map[x,y];
            }
        }
        return flattenMap;
    }

    private static float[,] MapReconstructor(float[] flattenMap, int width, int height) {
        float[,] map = new float[width, height];
        for(int y=0; y<height; y++){
            for(int x=0; x<width; x++){
                map[x,y] = flattenMap[y*width+x];
            }
        }
        return map;
    }
}
