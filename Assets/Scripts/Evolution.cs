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

    public static List<float[,]> SquareCrossover(float[,] map1, float[,] map2, int squareNum, bool smooth)
    {
        int mapWidth = map1.GetLength(0);
        int mapHeight = map2.GetLength(1);

        List<float[,]> result = new List<float[,]>();
        float[,] resMap1 = new float[mapWidth, mapHeight];
        float[,] resMap2 = new float[mapWidth, mapHeight];

        int squareWidth = (int)(mapWidth/squareNum);
        int squareHeight = (int)(mapHeight/squareNum);

        for(int y=0; y<mapHeight; y++){
            for(int x=0; x<mapWidth; x++){
                int bY = (int)(y/squareHeight);
                int bX = (int)(x/squareWidth);
                if((bX+bY)%2 == 0){
                    resMap1[x, y] = map1[x, y];
                    resMap2[x, y] = map2[x, y];
                } else {
                    resMap1[x, y] = map2[x, y];
                    resMap2[x, y] = map1[x, y];
                }
            }
        }
        if(smooth){
            resMap1 = Convolve(resMap1, GenerateKernel(10, 10));
            resMap1 = Normalize(resMap1);
            resMap2 = Convolve(resMap2, GenerateKernel(10, 10));
            resMap2 = Normalize(resMap2);
        }
        result.Add(resMap1);
        result.Add(resMap2);
        return result;
    }

    public static float[,] ScoreSquareCrossover(float[,] map1, float[,] map2, int squareNum, bool smooth, float waterPercentage, float landPercentage, float mountainPercentage, float snowPercentage){
        int mapWidth = map1.GetLength(0);
        int mapHeight = map2.GetLength(1);

        float[,] map = new float[mapWidth, mapHeight];

        
        int squareWidth = (int)(mapWidth/squareNum);
        int squareHeight = (int)(mapHeight/squareNum);
        

        for(int y = 0; y < squareNum; y++){
            for(int x = 0; x < squareNum; x++){
                float[,] tempMap1 = new float[squareWidth, squareHeight];
                float[,] tempMap2 = new float[squareWidth, squareHeight];
                int tY = y * squareHeight;
                int tX = x * squareWidth;
                int bY = (y+1)*squareHeight;
                int bX = (x+1)*squareWidth;
                for(int i = tY; i < bY; i++){
                    for(int j = tX; j < bX; j++){
                        tempMap1[j-tX, i-tY] = map1[j, i];
                        tempMap2[j-tX, i-tY] = map2[j, i];
                    }
                }
                float score1 = ScoreMap(tempMap1, waterPercentage, landPercentage, mountainPercentage, snowPercentage);
                float score2 = ScoreMap(tempMap2, waterPercentage, landPercentage, mountainPercentage, snowPercentage);
                if(score1 > score2){
                    for(int i = tY; i < bY; i++){
                        for(int j = tX; j < bX; j++){
                            map[j, i] = tempMap1[j-tX, i-tY];
                        }
                    }
                } else {
                    for(int i = tY; i < bY; i++){
                        for(int j = tX; j < bX; j++){
                            map[j, i] = tempMap2[j-tX, i-tY];
                        }
                    }
                }
            }
        }
        if(smooth){
            map = Convolve(map, GenerateKernel(10, 10));
            map = Normalize(map);
        }
        return map;
    }

    public static float[,] Evolve(int generations, int populationSize, int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, float waterPercentage, float landPercentage, float mountainPercentage, float snowPercentage, bool smooth){
        List<float[,]> maps = new List<float[,]>();
        for(int i=0; i<populationSize; i++){
            maps.Add(Noise.GenerateNoiseMap(mapWidth, mapHeight, seed+i, scale, octaves, persistance, lacunarity, offset));
        }
        for(int i=0; i<generations; i++){
            maps = GetChildren(maps);
            // Debug.Log("Generation: " + i);
            maps = FallChildren(maps, waterPercentage, landPercentage, mountainPercentage, snowPercentage);
        }
        float[,] chosenMap = Choose(maps, waterPercentage, landPercentage, mountainPercentage, snowPercentage);
        ScoreMap(chosenMap, waterPercentage, landPercentage, mountainPercentage, snowPercentage);
        if(smooth){
            chosenMap = Convolve(chosenMap, GenerateKernel(10, 10));
            chosenMap = Normalize(chosenMap);
        }
        return maps[1];
    }

    public static List<float[,]> FallChildren(List<float[,]> maps, float waterPercentage, float landPercentage, float mountainPercentage, float snowPercentage){
        List<float> scores = new List<float>();
        float maxScore = float.MinValue;
        float minScore = float.MaxValue;
        for(int i = 0; i < maps.Count; i++){
            scores.Add(ScoreMap(maps[i], waterPercentage, landPercentage, mountainPercentage, snowPercentage));
            if(scores[i] > maxScore){
                maxScore = scores[i];
            }
            if(scores[i] < minScore){
                minScore = scores[i];
            }
        }
        float targetScore = (maxScore + minScore)/2;
        targetScore = (maxScore + targetScore)/2;
        List<float[,]> result = new List<float[,]>();
        for(int i = 0; i < maps.Count; i++){
            if(scores[i] > targetScore){
                result.Add(maps[i]);
            }
        }
        return result;
    }

// not totally random
    public static List<float[,]> GetChildren(List<float[,]> maps){
        List<float[,]> children = new List<float[,]>();
        for(int i=0; i+1<maps.Count; i+=1){
            List<float[,]> childrenMaps = SquareCrossover(maps[i], maps[i+1], 3, false);
            children.Add(childrenMaps[0]);
            children.Add(childrenMaps[1]);
        }
        List<float[,]> cMaps = SquareCrossover(maps[0], maps[maps.Count-1], 3, false);
        children.Add(cMaps[0]);
        children.Add(cMaps[1]);
        return children;
    }

    public static float[,] Choose(List<float[,]> maps, float waterPercentage, float landPercentage, float mountainPercentage, float snowPercentage){
        float[,] chosenMap = maps[0];
        float chosenScore = ScoreMap(chosenMap, waterPercentage, landPercentage, mountainPercentage, snowPercentage);
        for(int i=1; i<maps.Count; i++){
            float score = ScoreMap(maps[i], waterPercentage, landPercentage, mountainPercentage, snowPercentage);
            Debug.Log("Map Score: " + score);
            if(score > chosenScore){
                chosenMap = maps[i];
                chosenScore = score;
            }
        }
        Debug.Log("Chosen Map Score" + chosenScore);
        return chosenMap;
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

// score every area and keep high score areas
    public static float ScoreMap(float[,] map, float waterThreshold, float landThreshold, float mountainThreshold, float snowThreshold) {
        int width = map.GetLength(0);
        int height = map.GetLength(1);

        int waterArea = 0;
        int landArea = 0;
        int mountainArea = 0;
        int snowArea = 0;
        for(int y=0; y<height; y++){
            for(int x=0; x<width; x++){
                if(map[x,y] < 0.4f)
                    waterArea++;
                else if(map[x,y] < 0.6f)
                    landArea++;
                else if(map[x,y] < 0.9f)
                    mountainArea++;
                else if(map[x,y] < 1.0f)
                    snowArea++;
            }
        }
        int area = height*width;
        float waterPercent = (float)waterArea/area;
        float landPercent = (float)landArea/area;
        float mountainPercent = (float)mountainArea/area;
        float snowPercent = (float)snowArea/area;
        // Debug.Log("water: " + waterPercent + " land: " + landPercent + " mountain: " + mountainPercent + " snow: " + snowPercent);
        float score = 1/(Mathf.Abs(waterPercent - waterThreshold) + Mathf.Abs(landPercent - landThreshold) + Mathf.Abs(mountainPercent - mountainThreshold) + Mathf.Abs(snowPercent - snowThreshold));
        return score;
    }
}
