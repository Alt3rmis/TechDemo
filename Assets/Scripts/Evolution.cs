using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Evolution
{
    public static float[,] OnePointCrossover(float[,] map1, float[,] map2) {
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

    public static float[,] SquareCrossover(float[,] map1, float[,] map2, int squareNum, bool smooth)
    {
        int mapWidth = map1.GetLength(0);
        int mapHeight = map2.GetLength(1);

        float[,] map = new float[mapWidth, mapHeight];

        int squareWidth = (int)(mapWidth/squareNum);
        int squareHeight = (int)(mapHeight/squareNum);

        for(int y=0; y<mapHeight; y++){
            for(int x=0; x<mapWidth; x++){
                int bY = (int)(y/squareHeight);
                int bX = (int)(x/squareWidth);
                if((bX+bY)%2 == 0)
                    map[x, y] = map1[x,y];
                else
                    map[x, y] = map2[x, y];
            }
        }
        if(smooth){
            map = Convolve(map, GenerateKernel(10, 10));
            map = Normalize(map);
        }
        return map;
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

    public static float[,] MapReconstructor(float[] flattenMap, int width, int height) {
        float[,] map = new float[width, height];
        for(int y=0; y<height; y++){
            for(int x=0; x<width; x++){
                map[x,y] = flattenMap[y*width+x];
            }
        }
        return map;
    }
    
    public static float[,] GenerateKernel(int length, float weight) {
        float[,] kernel = new float[length, length];
        for(int y=0; y<length; y++){
            for(int x=0; x<length; x++){
                kernel[x,y] = Gaussian(x, y, length, weight);
            }
        }
        return kernel;
    }

    public static float Gaussian(int x, int y, int length, float weight) {
        float x2 = x - length/2;
        float y2 = y - length/2;
        return Mathf.Exp(-((x2*x2 + y2*y2)/(2*weight*weight)));
    }

    public static float[,] Convolve(float[,] map, float[,] kernel){
        int kernelWidth = kernel.GetLength(0);
        int kernelHeight = kernel.GetLength(1);
        int mapWidth = map.GetLength(0);
        int mapHeight = map.GetLength(1);

        float[,] convolvedMap = new float[mapWidth, mapHeight];

        for(int y=0; y<mapHeight; y++){
            for(int x=0; x<mapWidth; x++){
                float sum = 0;
                for(int kY=0; kY<kernelHeight; kY++){
                    for(int kX=0; kX<kernelWidth; kX++){
                        int newX = x + kX - kernelWidth/2;
                        int newY = y + kY - kernelHeight/2;
                        if(newX >= 0 && newX < mapWidth && newY >= 0 && newY < mapHeight){
                            sum += map[newX, newY] * kernel[kX, kY];
                        }
                    }
                }
                convolvedMap[x, y] = sum;
            }
        }
        return convolvedMap;
    }

    public static float[,] Normalize(float[,] map)
    {
        int width = map.GetLength(0);
        int height = map.GetLength(1);

        float max = float.MinValue;
        float min = float.MaxValue;
        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                if(map[x,y] > max)
                    max = map[x,y];
                if(map[x,y] < min)
                    min = map[x,y];
            }
        }

        for(int y=0; y<height; y++){
            for(int x = 0; x < width; x++){
                map[x,y] = (map[x,y] - min)/(max-min);
            }
        }
        return map;
    }
}
