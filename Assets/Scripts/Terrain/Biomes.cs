using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Biomes : MonoBehaviour
{
    public Biome[] biomes;
    public int biomeTextureSize;
    Noise noiseGenerator = new Noise(3);

    public string CalculateBiomeName(Vector3 pos, float scaledHeight)
    {
        float[] biomesValue = new float[biomes.Length];
        
        return biomes[CalculateBiome(pos, scaledHeight)].name;
    }

    public int CalculateBiome (Vector3 pos, float scaledHeight, AnimationCurve[] biomeHeightMask = null)
    {
        float maxValue = 0f;
        int biomeIndex = -1;

        for (int i = 0; i < biomes.Length; i++)
        {
            // Biome evaluation based on height map
            float biomeValue;

            if (biomeHeightMask == null)
                biomeValue = biomes[i].biomeHeightMask.Evaluate(scaledHeight);
            else
                biomeValue = biomeHeightMask[i].Evaluate(scaledHeight);

            // Biome noise calculation
            float noise = 0f;
            Vector3 samplePoint = new Vector3((pos.x + 232.243f), i+234.23f, (pos.z + 2.2348f)) * biomes[i].noiseFrequency;
            noise = Utils.Get3DNoise(samplePoint, 1) * biomes[i].noiseAmplitude;
            biomeValue += noise;

            if (biomeValue > maxValue)
            {
                maxValue = biomeValue;
                biomeIndex = i;
            }
        }

        return biomeIndex;
    }

    void SetTerrainShaderBiomeTextures ()
    {
        Texture2DArray biomeTexturesArray = new Texture2DArray (biomeTextureSize, biomeTextureSize, biomes.Length, UnityEngine.Experimental.Rendering.DefaultFormat.LDR, UnityEngine.Experimental.Rendering.TextureCreationFlags.None);

        for (int i=0; i<biomes.Length; i++)
        {
            biomeTexturesArray.SetPixels(biomes[i].biomeGroundTexture.GetPixels(), i);
        }
        
        biomeTexturesArray.Apply();
        GetComponent<UnderwaterTerrain>().terrainMaterial.SetTexture("_BiomesTextureArray", biomeTexturesArray);
    }

    // Start is called before the first frame update
    void Awake()
    {
        SetTerrainShaderBiomeTextures();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
