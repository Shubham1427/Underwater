using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DebugInfo : MonoBehaviour
{
    public TMP_Text depthText, biomeText;
    Biomes biomesHandler;
    UnderwaterTerrain terrain;
    Player player;

    IEnumerator UpdateDebugInfo()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);

            int depth = (int)(player.depth * 3f);
            depthText.text = "Depth: " + depth.ToString() + "m";

            TerrainChunk playerChunk = terrain.GetChunkFromCoords(terrain.playerChunkCoords);
            // biomeText.text = "Biome: "+playerChunk.SampleScaledHeightMap(player.transform.position);
            biomeText.text = "Biome: " + biomesHandler.CalculateBiomeName(player.transform.position, playerChunk.SampleScaledHeightMap(player.transform.position));
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<Player>();
        biomesHandler = FindObjectOfType<Biomes>();
        terrain = FindObjectOfType<UnderwaterTerrain>();

        StartCoroutine(UpdateDebugInfo());
    }

    // Update is called once per frame
    void Update()
    {

    }
}
