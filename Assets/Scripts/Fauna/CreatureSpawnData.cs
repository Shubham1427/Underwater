using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CreatureSpawnData
{
    public int id;
    public int maxCount;
    public int minSpawnHeight, maxSpawnHeight;
    public int[] nativeBiomes;

    public bool IsNativeBiome (TerrainChunk chunk, Vector3 pos)
    {
        if (chunk == null)
            return false;

        int biome = chunk.SampleBiomeMap(pos);

        for (int i=0; i<nativeBiomes.Length; i++)
        {
            if (biome == nativeBiomes[i])
                return true;
        }
        return false;
    }

}
