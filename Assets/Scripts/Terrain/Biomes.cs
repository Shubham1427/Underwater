using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Biomes : MonoBehaviour
{
    public Biome[] biomes;

    public string CalculateBiome (float scaledHeight)
    {
        float[] biomesValue = new float [biomes.Length];
        float maxValue = 0f;
        int biomeIndex = 0;

        for (int i=0; i<biomes.Length; i++)
        {
            biomesValue[i] = biomes[i].biomeHeightMask.Evaluate(scaledHeight);
            if (biomesValue[i] > maxValue)
            {
                maxValue = biomesValue[i];
                biomeIndex = i;
            }
        }
        return biomes[biomeIndex].name;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
