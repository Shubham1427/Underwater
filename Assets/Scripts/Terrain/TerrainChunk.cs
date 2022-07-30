using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class TerrainChunk : MonoBehaviour
{
    UnderwaterTerrain terrain;
    Biomes biomesHandler;

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;
    FloraSpawner floraSpawner;
    Mesh mesh;
    List<Vector3> vertices = new List<Vector3>();
    List<Vector2> uvs = new List<Vector2>();
    List<int> triangles = new List<int>();
    float[,,] densityMap;
    int[,,] biomesMap;
    float[,] scaledHeightMap;
    Vector3Int coords;
    public Vector3 position
    {
        get
        {
            return coords * terrain.chunkSize - new Vector3(1f, 0f, 1f) * terrain.chunkSize / 2f;
        }
    }
    bool meshDataReady = false, meshRendered = false;
    AnimationCurve heightMapCurve, cavesDensityCurve, cavesHeightCurve, overhangsHeightCurve, overhangsDensityCurve;
    AnimationCurve[] biomesHeightMask;

    void Update()
    {
        if (meshDataReady && !meshRendered)
        {
            StartCoroutine(floraSpawner.SpawnFlora());
            CreateMesh();
            UpdateMesh();
        }
    }

    public float SampleScaledHeightMap(Vector3 pos, bool worldSpace = true)
    {
        if (worldSpace)
            pos -= position;
        Vector3Int samplePos = new Vector3Int((int)pos.x, (int)pos.y, (int)(pos.z));

        if (scaledHeightMap == null)
            return 0f;

        return scaledHeightMap[samplePos.x, samplePos.z];
    }

    public float SampleDensityMap(Vector3 pos, bool worldSpace = true)
    {
        if (pos.y > terrain.terrainHeight || pos.y < 0f)
            return 0f;
        if (worldSpace)
            pos -= position;
        Vector3Int samplePos = new Vector3Int((int)pos.x, (int)pos.y, (int)(pos.z));

        if (densityMap == null)
            return 0f;


        return densityMap[samplePos.x, samplePos.y, samplePos.z];
    }

    public int SampleBiomeMap(Vector3 pos, bool worldSpace = true)
    {
        if (worldSpace)
            pos -= position;
        Vector3Int samplePos = new Vector3Int((int)pos.x, (int)pos.y, (int)(pos.z));

        if (biomesMap == null)
            return -1;

        return biomesMap[samplePos.x, samplePos.y, samplePos.z];
    }

    public void Init(UnderwaterTerrain t, Vector3Int pos)
    {
        terrain = t;
        biomesHandler = t.GetComponent<Biomes>();
        coords = pos;
        transform.position = terrain.terrainCentre + coords * terrain.chunkSize - new Vector3(1f, 0f, 1f) * terrain.chunkSize / 2f;
        transform.parent = terrain.transform;
        gameObject.layer = LayerMask.NameToLayer("Terrain");

        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshCollider = gameObject.AddComponent<MeshCollider>();
        floraSpawner = gameObject.AddComponent<FloraSpawner>();
        floraSpawner.Init(this, terrain, biomesHandler);

        heightMapCurve = new AnimationCurve(terrain.heightMapCurve.keys);
        cavesDensityCurve = new AnimationCurve(terrain.cavesDensityCurve.keys);
        overhangsDensityCurve = new AnimationCurve(terrain.overhangsDensityCurve.keys);
        cavesHeightCurve = new AnimationCurve(terrain.cavesHeightCurve.keys);
        overhangsHeightCurve = new AnimationCurve(terrain.overhangsHeightCurve.keys);

        biomesHeightMask = new AnimationCurve[biomesHandler.biomes.Length];
        for (int i = 0; i < biomesHandler.biomes.Length; i++)
            biomesHeightMask[i] = new AnimationCurve(biomesHandler.biomes[i].biomeHeightMask.keys);

        meshDataReady = false;
        meshRendered = false;
        Thread newThread = new Thread(new ThreadStart(GenerateMeshData));

        newThread.Start();
    }

    void GenerateMeshData()
    {
        GenerateChunkDensityMap();
        MarchCubes();
        meshDataReady = true;
    }

    void MarchCubes()
    {
        for (int x = 0; x < terrain.chunkSize; x++)
        {
            for (int y = 0; y < terrain.terrainHeight; y++)
            {
                for (int z = 0; z < terrain.chunkSize; z++)
                {
                    GenerateCube(new Vector3Int(x, y, z));
                }
            }
        }
    }

    float GenerateNoise(int x, int y, int z)
    {
        float noise = 0f, frequency = 0.15f;

        // Calculate height
        Vector3 samplePoint = new Vector3((position.x + x + 42.715f), 63.45f, (position.z + z + 86.918f)) * frequency;
        noise = (Utils.Get3DNoise(samplePoint, 1) + 1) / 2f;

        noise = heightMapCurve.Evaluate(noise);
        float height = noise * (terrain.terrainHeight - 20f) + 8f;

        samplePoint = new Vector3((position.x + x + 0.086f), 53.355f, (position.z + z + 0.28f)) * frequency;
        noise = (Utils.Get3DNoise(samplePoint, 1) + 1) / 2f + 0.1f;
        noise = Mathf.Clamp(noise, 0.8f, 1.1f);
        height *= noise;

        //Height Map adjustment based on another noise
        frequency = 1.5f;
        samplePoint = new Vector3((position.x + x + 0.068f), 53.65f, (position.z + z + 0.072f)) * frequency;
        height += Utils.Get3DNoise(samplePoint, 6) * 10f;
        height = Mathf.Clamp(height, 1f, terrain.terrainHeight - 1);

        // Original density at this point
        float returnValue = (y - height) / terrain.terrainHeight;

        // density scaled from -1 to 1
        float scaledDensity = (y - height);
        if (scaledDensity < 0f)
            scaledDensity /= height;
        else
            scaledDensity /= terrain.terrainHeight - height;

        // Height at this x and z coordinates scaled from 0 to 1
        float scaledHeight = height / terrain.terrainHeight;
        scaledHeightMap[x, z] = scaledHeight;

        // Biome at these coordinates
        Vector3 posRelativeToWorld = new Vector3(position.x + x, y, position.z + z);
        biomesMap[x, y, z] = biomesHandler.CalculateBiome(posRelativeToWorld, scaledHeight, biomesHeightMask);

        if (y == terrain.terrainHeight)
            return 1f;
        if (y == 0)
            return -1f;

        //Overhangs noise

        //Variable frequency
        frequency = 0.8f;

        samplePoint = new Vector3((position.x + x + 3.4523f), y + 0.433f, (position.z + z + 2.347f)) * frequency;
        noise = (Utils.Get3DNoise(samplePoint, 1) + 1f) / 2f;
        
        frequency = 2.6f;
        samplePoint = new Vector3((position.x + x + 2.2343f), y + 3.4353f, (position.z + z + 6.654f)) * frequency;
        noise *= (Utils.Get3DNoise(samplePoint, 1) + 1f) / 2f;
        
        float amplitude = 0.3f;

        returnValue -= overhangsHeightCurve.Evaluate(scaledHeight) * overhangsDensityCurve.Evaluate(scaledDensity) * noise * amplitude;

        //Caves Noise

        frequency = 1.25f;
        samplePoint = new Vector3((position.x + x + 0.0244f), y + 0.735f, (position.z + z + 0.055f)) * frequency;
        noise = (Utils.Get3DNoise(samplePoint, 1) + 1f) / 2f;

        returnValue += cavesHeightCurve.Evaluate(scaledHeight) * cavesDensityCurve.Evaluate(scaledDensity) * noise * 0.65f;

        return returnValue;
        // return Mathf.Clamp(returnValue, -1f, 1f);
    }

    void GenerateChunkDensityMap()
    {
        densityMap = new float[terrain.chunkSize + 1, terrain.terrainHeight + 1, terrain.chunkSize + 1];
        scaledHeightMap = new float[terrain.chunkSize + 1, terrain.chunkSize + 1];
        biomesMap = new int[terrain.chunkSize + 1, terrain.terrainHeight + 1, terrain.chunkSize + 1];

        for (int x = 0; x <= terrain.chunkSize; x++)
        {
            for (int z = 0; z <= terrain.chunkSize; z++)
            {
                for (int y = 0; y <= terrain.terrainHeight; y++)
                {
                    float noise = GenerateNoise(x, y, z);
                    densityMap[x, y, z] = noise;
                }
            }
        }
    }

    int CalculateConfigurationIndex(Vector3Int cubePos)
    {
        int configurationIndex = 0;

        for (int i = 0; i < 8; i++)
        {
            Vector3Int index = cubePos + MarchingCubesData.VertexPositionTable[i];

            if (densityMap[index.x, index.y, index.z] <= 0f)
                configurationIndex |= (1 << i);
        }

        return configurationIndex;
    }

    int CheckDuplicateVertex(Vector3 vertex)
    {
        int index = 0;
        foreach (Vector3 v in vertices)
        {
            if (v == vertex)
                return index;
            index++;
        }
        return -1;
    }

    void GenerateCube(Vector3Int pos)
    {
        int configurationIndex = CalculateConfigurationIndex(pos);
        int maxNumberOfVertices = MarchingCubesData.TriangleConnectionTable.GetLength(1);

        for (int i = 0; i < maxNumberOfVertices; i++)
        {
            int edgeIndex = MarchingCubesData.TriangleConnectionTable[configurationIndex, i];

            if (edgeIndex == -1)
                break;

            Vector3Int vertA = pos + MarchingCubesData.VertexPositionTable[MarchingCubesData.EdgeVerticesTable[edgeIndex, 0]];
            Vector3Int vertB = pos + MarchingCubesData.VertexPositionTable[MarchingCubesData.EdgeVerticesTable[edgeIndex, 1]];
            float valueA = densityMap[vertA.x, vertA.y, vertA.z];
            float valueB = densityMap[vertB.x, vertB.y, vertB.z];
            float lerpAmount = Mathf.InverseLerp(valueA, valueB, 0f);

            Vector3 vertexPoint = Vector3.Lerp(vertA, vertB, lerpAmount);

            int vertexIndex = CheckDuplicateVertex(vertexPoint);
            if (vertexIndex == -1)
            {
                vertices.Add(vertexPoint);
                float biomeAtVertA = biomesMap[vertA.x, vertA.y, vertA.z];
                float biomeAtVertB = biomesMap[vertB.x, vertB.y, vertB.z];
                float biomeLerped = Mathf.Lerp(biomeAtVertA, biomeAtVertB, lerpAmount);
                uvs.Add(new Vector2(Mathf.RoundToInt(biomeLerped), 0));
                triangles.Add(vertices.Count - 1);
            }
            else
                triangles.Add(vertexIndex);
        }
    }

    void UpdateMesh()
    {
        meshFilter.mesh = mesh;
        meshRenderer.material = terrain.terrainMaterial;
        meshCollider.sharedMesh = mesh;
        meshRendered = true;
    }

    void ClearMesh()
    {
        mesh.Clear();
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
    }

    void CreateMesh()
    {
        if (mesh == null)
            mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
    }
}
