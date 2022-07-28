using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnderwaterTerrain : MonoBehaviour
{
    public Material terrainMaterial;
    public int chunkSize, terrainHeight, viewDistanceInChunks;
    public int minHeight, maxHeight;
    public AnimationCurve heightMapCurve;

    [Header("Caves Noise Settings")]
    public AnimationCurve cavesHeightCurve;
    public AnimationCurve cavesDensityCurve;

    [Header("Overhangs Noise Settings")]
    public AnimationCurve overhangsHeightCurve;
    public AnimationCurve overhangsDensityCurve;

    public Vector3Int terrainCentre;
    public Player player;
    public Vector3Int playerChunkCoords
    {
        get
        {
            return new Vector3Int(Mathf.FloorToInt((player.transform.position.x - chunkSize / 2f) / chunkSize) + 1, 0, Mathf.FloorToInt((player.transform.position.z - chunkSize / 2f) / chunkSize) + 1);
        }
    }
    Dictionary<Vector3Int, TerrainChunk> activeChunks;
    Vector3Int lastPlayerChunkCoords;
    bool updatingChunks = false;

    public TerrainChunk GetChunkFromCoords (Vector3Int coords)
    {
        if (activeChunks.ContainsKey(coords))
            return activeChunks[coords];
        
        return null;
    }

    IEnumerator UpdateActiveChunks()
    {
        while (true)
        {
            //Player has moved to a different chunk
            if (playerChunkCoords != lastPlayerChunkCoords && !updatingChunks)
            {
                updatingChunks = true;

                //Destroy chunks that are too far away
                List<Vector3Int> chunksToDestroy = new List<Vector3Int>();
                foreach (Vector3Int chunkCoords in activeChunks.Keys)
                {
                    int relativeX = Mathf.Abs(chunkCoords.x - playerChunkCoords.x);
                    int relativeZ = Mathf.Abs(chunkCoords.z - playerChunkCoords.z);

                    if (relativeX > viewDistanceInChunks / 2 || relativeZ > viewDistanceInChunks / 2)
                    {
                        TerrainChunk chunk = activeChunks[chunkCoords];
                        chunksToDestroy.Add(chunkCoords);
                        Destroy(chunk.gameObject);
                    }
                    yield return new WaitForSeconds (0.02f);
                }
                foreach (Vector3Int chunkCoord in chunksToDestroy)
                {
                    activeChunks.Remove(chunkCoord);
                    yield return new WaitForSeconds (0.02f);
                }

                //Create new chunks that are now closer to player
                for (int x = playerChunkCoords.x - viewDistanceInChunks / 2; x <= playerChunkCoords.x + viewDistanceInChunks / 2; x++)
                {
                    for (int z = playerChunkCoords.z - viewDistanceInChunks / 2; z <= playerChunkCoords.z + viewDistanceInChunks / 2; z++)
                    {
                        Vector3Int coords = new Vector3Int(x, 0, z);
                        if (activeChunks.ContainsKey(coords))
                            continue;
                        TerrainChunk chunk = new GameObject("Chunk (" + x + ", " + z + ")").AddComponent<TerrainChunk>();
                        chunk.Init(this, coords);
                        activeChunks.Add(coords, chunk);
                        yield return new WaitForSeconds (0.03f);
                    }
                }
                
                // Set player's last chunk to this chunk
                lastPlayerChunkCoords = playerChunkCoords;
                updatingChunks = false;
                
                chunksToDestroy.Clear();
            }
            yield return new WaitForSeconds (0.1f);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        activeChunks = new Dictionary<Vector3Int, TerrainChunk>();
        for (int x = -viewDistanceInChunks / 2; x <= viewDistanceInChunks / 2; x++)
        {
            for (int z = -viewDistanceInChunks / 2; z <= viewDistanceInChunks / 2; z++)
            {
                TerrainChunk chunk = new GameObject("Chunk (" + x + ", " + z + ")").AddComponent<TerrainChunk>();
                Vector3Int coords = new Vector3Int(x, 0, z);
                chunk.Init(this, coords);
                activeChunks.Add(coords, chunk);
            }
        }
        lastPlayerChunkCoords = new Vector3Int(0, 0, 0);
        StartCoroutine(UpdateActiveChunks());
    }

    // Update is called once per frame
    void Update()
    {
        if (player.isUnderwater)
            terrainMaterial.SetInt("_IsPlayerUnderwater", 1);
        else
            terrainMaterial.SetInt("_IsPlayerUnderwater", 0);
    }
}
