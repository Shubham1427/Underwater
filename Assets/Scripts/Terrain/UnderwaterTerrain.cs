using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnderwaterTerrain : MonoBehaviour
{
    public Material terrainMaterial;
    public int chunkSize, terrainHeight, terrainSizeInChunks;
    public int minHeight, maxHeight;
    public AnimationCurve cavesHeightCurve, heightMapCurve, cavesDensityCurve;
    public Vector3Int terrainCentre;
    public Player player;


    // Start is called before the first frame update
    void Start()
    {
        for (int x = -terrainSizeInChunks / 2; x <= terrainSizeInChunks / 2; x++)
        {
            for (int z = -terrainSizeInChunks / 2; z <= terrainSizeInChunks / 2; z++)
            {
                TerrainChunk chunk = new GameObject("Chunk (" + x + ", " + z + ")").AddComponent<TerrainChunk>();
                chunk.Init(this, new Vector3Int(x, 0, z));
            }
        }
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
