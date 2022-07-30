using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class Creature : MonoBehaviour
{
    protected UnderwaterTerrain terrain;
    protected FaunaSpawner faunaSpawner;
    protected bool navigationPointFound = false, calculatingNavigationPoint = false;
    protected Vector3 navigationPoint, currentPosition, randomUnitSpherePoint;
    public int id;
    public float navPointMinHeight, navPointMaxHeight;

    protected virtual void GetNavigationPoint()
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

            // Check if navigation point height is valid
            if (navigationPoint.y > navPointMaxHeight || navigationPoint.y < navPointMinHeight)
                continue;

            // Check if biome is valid
            if (!faunaSpawner.creatureSpawnDatas[id].IsNativeBiome(chunk, navigationPoint))
                continue;

            // Check if path passes through solid
            bool flag = false;
            Vector3 pos = currentPosition;
            while ((navigationPoint - pos).magnitude > 0.1f)
            {
                chunk = terrain.GetChunkFromCoords(terrain.GetChunkCoordsFromWorldPos(pos));

                if (chunk == null || chunk.SampleDensityMap(pos) <= 0f)
                {
                    flag = true;
                    break;
                }

                pos += (navigationPoint - pos).normalized * 0.1f;
            }

            if (flag)
                continue;

            navigationPointFound = true;
            calculatingNavigationPoint = false;
            return;
        }
    }

    protected void ExecuteCreatureStateMachine ()
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
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(navigationPoint - transform.position), Time.deltaTime * 3f);
            float distance = (transform.position - navigationPoint).magnitude;
            Vector3 newPos = transform.position + transform.forward * Time.deltaTime * Mathf.Clamp01(distance) * 4f;
            transform.position = newPos;

            if (distance < 0.2f)
            {
                transform.position = navigationPoint;
                navigationPointFound = false;
            }
        }
    }

    public void Init(UnderwaterTerrain t, FaunaSpawner f)
    {
        terrain = t;
        faunaSpawner = f;
        navigationPointFound = false;
        calculatingNavigationPoint = false;
    }

    void Update()
    {
        ExecuteCreatureStateMachine();
    }
}
