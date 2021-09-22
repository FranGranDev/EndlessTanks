using UnityEngine;

public static class Noise
{
    public static float[,] GenerateNoiseMap(int xSize, int ySize, float Scale, float xOffset, float yOffset, int Octaves, float Persistance, float Lacuranty)
    {
        float[,] Map = new float[xSize, ySize];
        float MaxNoise = float.MinValue;
        float MinNoise = float.MaxValue;
        float HalfxSize = xSize * 0.5f;
        float HalfySize = ySize * 0.5f;

        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                float NoiseHeight = 0;
                float Amplitude = 1;
                float Frequency = 1;

                for (int i = 0; i < Octaves; i++)
                {
                    float xSample = (x - HalfxSize) / xSize * Frequency * Scale + xOffset;
                    float zSample = (y - HalfySize) / ySize * Frequency * Scale + yOffset;
                    float Noise = Mathf.PerlinNoise(xSample, zSample) * 2f - 1f;
                    NoiseHeight += Noise * Amplitude;

                    Frequency *= Lacuranty;
                    Amplitude *= Persistance;
                }
                if (NoiseHeight > MaxNoise)
                {
                    MaxNoise = NoiseHeight;
                }
                else if (NoiseHeight < MinNoise)
                {
                    MinNoise = NoiseHeight;
                }

                Map[x, y] = NoiseHeight;
            }
        }
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                Map[x, y] = Mathf.InverseLerp(MinNoise, MaxNoise, Map[x, y]);
            }
        }
        return Map;
    }

}
