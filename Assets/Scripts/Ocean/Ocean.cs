using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ocean : MonoBehaviour
{
    public int oceanSizeInChunks, chunkSize, highestResolution;
    public float oceanLevel;
    public Material oceanUpMaterial, oceanDownMaterial;
    public Player player;
    public float fogEndDistanceAtDeepestPoint;

    Color oceanFogColor;
    float surfaceFogEndDistance;

    // Start is called before the first frame update
    void Start()
    {
        oceanFogColor = RenderSettings.fogColor;
        surfaceFogEndDistance = RenderSettings.fogEndDistance;

        for (int x = -oceanSizeInChunks / 2; x <= oceanSizeInChunks / 2; x++)
        {
            for (int z = -oceanSizeInChunks / 2; z <= oceanSizeInChunks / 2; z++)
            {
                OceanChunk chunkTop = new GameObject("Ocean Chunk (" + x + ", " + z + ") Top").AddComponent<OceanChunk>();
                OceanChunk chunkBottom = new GameObject("Ocean Chunk (" + x + ", " + z + ") Bottom").AddComponent<OceanChunk>();
                Vector2 pos = new Vector2 (x, z);
                // Vector2 playerPos = new Vector2 (player.position.x, player.position.z);
                float distanceFromPlayer = (pos).magnitude;
                if (distanceFromPlayer < 1.5f)
                    distanceFromPlayer = 0f;
                chunkTop.Init(this, new Vector3Int(x, 0, z), player.transform, Mathf.Max(2, highestResolution - (int)(distanceFromPlayer) * 2), true);
                chunkBottom.Init(this, new Vector3Int(x, 0, z), player.transform, Mathf.Max(2, highestResolution - (int)(distanceFromPlayer) * 2), false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (oceanLevel > player.transform.position.y)
        {
            float depth = (oceanLevel - player.transform.position.y)/oceanLevel;
            RenderSettings.fogColor = Color.Lerp(oceanFogColor, Color.black, depth);
            RenderSettings.fogEndDistance = Mathf.Lerp(surfaceFogEndDistance, fogEndDistanceAtDeepestPoint, depth);
            player.playerCam.backgroundColor = RenderSettings.fogColor;
        }
    }
}
