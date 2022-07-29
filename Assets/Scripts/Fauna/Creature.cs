using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class Creature : MonoBehaviour
{
    UnderwaterTerrain terrain;
    bool navigationPointFound = false, calculatingNavigationPoint = false;
    Vector3 navigationPoint, currentPosition, randomUnitSpherePoint;

    void GetNavigationPoint()
    {
        System.Random rng = new System.Random();
        while (true)
        {
            navigationPoint = currentPosition + new Vector3((rng.Next() % 11 - 5) * 10f, (rng.Next() % 11 - 5) * 4f, (rng.Next() % 11 - 5) * 10f);
            if (navigationPoint.y < 0f || navigationPoint.y > terrain.terrainHeight)
                continue;

            TerrainChunk chunk = terrain.GetChunkFromCoords(terrain.GetChunkCoordsFromWorldPos(navigationPoint));

            if (chunk == null)
                continue;

            // Check if navigation point is not solid
            if (chunk.SampleDensityMap(navigationPoint) <= 0f)
                continue;

            // Check if navigation point is underwater
            if (navigationPoint.y >= 30f)
                continue;

            // Check if biome is deeps
            if (chunk.SampleBiomeMap(navigationPoint) != 3)
                continue;

            navigationPointFound = true;
            calculatingNavigationPoint = false;
            return;
        }
    }

    public void Init(UnderwaterTerrain t)
    {
        terrain = t;
    }

    // Start is called before the first frame update
    void Start()
    {
        navigationPointFound = false;
        calculatingNavigationPoint = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!navigationPointFound && !calculatingNavigationPoint)
        {
            Thread newThread = new Thread(new ThreadStart(GetNavigationPoint));
            currentPosition = transform.position;
            newThread.Start();
            calculatingNavigationPoint = true;
        }
        else if (navigationPointFound)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(navigationPoint - transform.position), Time.deltaTime * 2f);
            float distance = (transform.position - navigationPoint).magnitude;
            transform.position += transform.forward * Time.deltaTime * (Mathf.Clamp01(distance) + 0.05f) * 4f;
            if (distance < 0.1f)
            {
                transform.position = navigationPoint;
                navigationPointFound = false;
            }
        }
    }
}
