using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloraSpawner : MonoBehaviour
{
    UnderwaterTerrain terrain;
    TerrainChunk chunk;
    Biomes biomesHandler;
    Noise noiseGenerator = new Noise(5);


    public void Init(TerrainChunk c, UnderwaterTerrain t, Biomes b)
    {
        chunk = c;
        terrain = t;
        biomesHandler = b;
    }

    public IEnumerator SpawnFlora()
    {
        for (int x = 0; x < terrain.chunkSize; x++)
        {
            for (int z = 0; z < terrain.chunkSize; z++)
            {
                // Raycast from top of the world to find where the ground is
                Vector3 spawnPos = new Vector3(chunk.position.x + x, terrain.terrainHeight, chunk.position.z + z);
                RaycastHit hit;

                if (!Physics.Raycast(spawnPos, Vector3.down, out hit, terrain.terrainHeight, LayerMask.GetMask("Terrain")))
                    continue;

                // Check if the ground is not too steep for the plants to grow
                if (hit.normal.y < 0.7f)
                    continue;

                // Find out the biomes at the point
                spawnPos.y = hit.point.y;
                int biome = chunk.SampleBiomeMap(new Vector3(x, Mathf.RoundToInt(spawnPos.y), z), false);

                // Check if the biomes supports growing plants
                if (biomesHandler.biomes[biome].plantPrefab == null)
                    continue;

                // Calculate noise for large chunks of plants
                float noise = 0f;
                Vector3 samplePoint = new Vector3((chunk.position.x + x + 2.243f), spawnPos.y + 34.23f, (chunk.position.z + z + 2.2348f)) * biomesHandler.biomes[biome].floraDensityFrequency;
                noise = Utils.Get3DNoise(noiseGenerator, samplePoint, 1);

                if (noise < -0.2f)
                    continue;

                // Calculate noise for individual plant
                samplePoint = new Vector3((chunk.position.x + x + 624.35f), spawnPos.y + 245.567f, (chunk.position.z + z + 45.456f)) * biomesHandler.biomes[biome].plantFrequency;
                noise = Utils.Get3DNoise(noiseGenerator, samplePoint, 1);

                if (noise < biomesHandler.biomes[biome].plantThreshold)
                    continue;

                // Create plant
                GameObject plant = Instantiate(biomesHandler.biomes[biome].plantPrefab);
                plant.transform.position = spawnPos;
                plant.transform.parent = transform;

                // Randomize height if it is a custom plant
                PlantGenerator pg = plant.GetComponentInChildren<PlantGenerator>();
                if (pg != null)
                {
                    samplePoint = new Vector3((chunk.position.x + x + 6.743f), spawnPos.y + 4.456f, (chunk.position.z + z + 7.456f)) * 0.8f;
                    noise = Utils.Get3DNoise(noiseGenerator, samplePoint, 1) * 10f;

                    pg.plantHeight += noise;
                }

                yield return new WaitForSeconds(0.2f);
            }
        }
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
