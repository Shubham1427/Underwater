using UnityEngine;

public static class Utils
{
    public static float ScaleTo_Minus1_1 (float minValue, float maxValue, float value)
    {
        return ScaleTo_0_1(minValue, maxValue, value) * 2f - 1f;
    }

    public static float ScaleTo_0_1 (float minValue, float maxValue, float value)
    {
        return (value - minValue) / (maxValue - minValue);
    }

    public static float Get3DNoise (Vector3 samplePoint, int layers=1)
    {
        float frequency = 1f, amplitude = 1f, noise = 0f;
        float maxNoise = 0f;
        for (int i=0; i<layers; i++)
        {
            noise += Mathf.Clamp(GetFastNoise(samplePoint * frequency), -1f, 1f) * amplitude;
            maxNoise += amplitude;
            amplitude /= 2f;
            frequency *= 2f;
        }
        noise /= maxNoise;
        return noise;
    }

    public static float GetFastNoise (Vector3 samplePoint)
    {
        return UnderwaterTerrain.fastNoiseGenerator.GetNoise(samplePoint.x, samplePoint.y, samplePoint.z);
    }
}