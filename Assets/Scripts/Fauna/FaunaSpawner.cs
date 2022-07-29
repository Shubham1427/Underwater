using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaunaSpawner : MonoBehaviour
{
    UnderwaterTerrain terrain;
    Ocean ocean;
    List<Creature> greatWhiteSharks;
    Player player;

    public GameObject sharkPrefab;

    IEnumerator FaunaSpawnHandler ()
    {
        FaunaInfo.numberOfsharks = 0;
        greatWhiteSharks = new List<Creature>();
        while (true)
        {
            if (terrain == null || ocean == null || player == null)
            {
                yield return new WaitForSeconds (0.2f);
                continue;
            }

            // Check if any shark needs despawning
            for (int i=0; i<greatWhiteSharks.Count; i++)
            {
                Creature fauna = greatWhiteSharks[i];
                TerrainChunk faunaChunk = terrain.GetChunkFromCoords(terrain.GetChunkCoordsFromWorldPos(fauna.transform.position));
                // Debug.Log(terrain.GetChunkCoordsFromWorldPos(fauna.transform.position));
                if (faunaChunk == null)
                {
                    greatWhiteSharks.RemoveAt(i);
                    Destroy(fauna.gameObject);
                    FaunaInfo.numberOfsharks--;
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
            if (spawnPoint.y >= 30f)
            {
                yield return new WaitForSeconds (0.2f);
                continue;
            }

            // Check if biome is deeps
            if (chunk.SampleBiomeMap(spawnPoint) != 3)
            {
                yield return new WaitForSeconds (0.2f);
                continue;
            }

            // Check if sharks can be spawned
            if (FaunaInfo.numberOfsharks >= 5)
            {
                yield return new WaitForSeconds (0.2f);
                continue;
            }

            // Spawn Shark
            Creature shark = Instantiate(sharkPrefab).GetComponent<Creature>();
            shark.transform.position = spawnPoint;
            shark.transform.parent = transform;
            shark.Init(terrain);
            FaunaInfo.numberOfsharks++;
            greatWhiteSharks.Add(shark);        

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
