using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaunaSpawner : MonoBehaviour
{
    UnderwaterTerrain terrain;
    Ocean ocean;
    List<Creature> creatures;
    Player player;
    int[] creatureCounts;


    public GameObject[] creaturePrefabs;
    public CreatureSpawnData[] creatureSpawnDatas;

    IEnumerator FaunaSpawnHandler ()
    {
        creatureCounts = new int [creaturePrefabs.Length];
        for (int i=0; i<creatureCounts.Length; i++)
            creatureCounts[i] = 0;

        creatures = new List<Creature>();

        while (true)
        {
            if (terrain == null || ocean == null || player == null)
            {
                yield return new WaitForSeconds (0.2f);
                continue;
            }

            // Check if any creature needs despawning
            for (int i=0; i<creatures.Count; i++)
            {
                Creature creature = creatures[i];
                TerrainChunk faunaChunk = terrain.GetChunkFromCoords(terrain.GetChunkCoordsFromWorldPos(creature.transform.position));
                if (faunaChunk == null)
                {
                    creatures.RemoveAt(i);
                    creatureCounts[creature.id]--;
                    Destroy(creature.gameObject);
                    i--;
                }
                yield return new WaitForSeconds (0.2f);
            }

            // Pickup a random chunk
            int maxChunkCoord = terrain.viewDistanceInChunks/2;
            Vector3Int randomChunkCoords = new Vector3Int (Random.Range(-maxChunkCoord, maxChunkCoord+1), 0, Random.Range(-maxChunkCoord, maxChunkCoord+1));
            randomChunkCoords += terrain.playerChunkCoords;

            // Pickup a random point
            TerrainChunk chunk = terrain.GetChunkFromCoords(randomChunkCoords);
            if (chunk == null)
            {
                yield return new WaitForSeconds (0.2f);
                continue;
            }
            Vector3 spawnPoint = chunk.position;
            spawnPoint += new Vector3 (Random.Range (0, terrain.chunkSize), Random.Range (1, terrain.terrainHeight), Random.Range (0, terrain.chunkSize));

            // Check if spawn point is not solid
            if (chunk.SampleDensityMap(spawnPoint) <= 0f)
            {
                yield return new WaitForSeconds (0.2f);
                continue;
            }

            // Check if spawn point is underwater
            if (spawnPoint.y >= ocean.oceanLevel)
            {
                yield return new WaitForSeconds (0.2f);
                continue;
            }

            // Loop for each creature and find the one that can be spawned here
            for (int i=0; i<creaturePrefabs.Length; i++)
            {
                //Check if height is valid
                if (spawnPoint.y < creatureSpawnDatas[i].minSpawnHeight || spawnPoint.y > creatureSpawnDatas[i].maxSpawnHeight)
                {
                    yield return new WaitForSeconds (0.2f);
                    continue;
                }

                // Check if biome is valid
                if (!creatureSpawnDatas[i].IsNativeBiome(chunk, spawnPoint))
                {
                    yield return new WaitForSeconds (0.2f);
                    continue;
                }

                // Check if creature cap is reached
                if (creatureCounts[i] >= creatureSpawnDatas[i].maxCount)
                {
                    yield return new WaitForSeconds (0.2f);
                    continue;
                }


                // Spawn Creature
                Creature creature = Instantiate(creaturePrefabs[i]).GetComponent<Creature>();
                creature.transform.position = spawnPoint;
                creature.transform.parent = transform;
                creature.Init(terrain, this);
                creatureCounts[i]++;
                creatures.Add(creature);        

                break;
            }

            yield return new WaitForSeconds (0.2f);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        terrain = FindObjectOfType<UnderwaterTerrain>();
        ocean = FindObjectOfType<Ocean>();
        player = FindObjectOfType<Player>();
        StartCoroutine(FaunaSpawnHandler());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
