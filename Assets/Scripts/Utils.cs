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

    public static float Get3DNoise (Noise noiseGenerator, Vector3 samplePoint, int layers=1)
    {
        float frequency = 1f, amplitude = 1f, noise = 0f;
        float maxNoise = 0f;
        for (int i=0; i<layers; i++)
        {
            noise += noiseGenerator.Evaluate(samplePoint * frequency) * amplitude;
            maxNoise += amplitude;
            amplitude /= 2f;
            frequency *= 2f;
        }
        noise /= maxNoise;
        return noise;
    }
}