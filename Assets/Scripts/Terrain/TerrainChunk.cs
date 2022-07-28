using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class TerrainChunk : MonoBehaviour
{
    UnderwaterTerrain terrain;

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;
    Mesh mesh;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    float[,,] chunkDensityMap;
    Vector3Int coords;
    Noise noiseGenerator = new Noise();
    Vector3 position
    {
        get
        {
            return coords * terrain.chunkSize - new Vector3(1f, 0f, 1f) * terrain.chunkSize / 2f;
        }
    }
    bool meshDataReady = false, meshRendered = false;
    AnimationCurve heightMapCurve, cavesDensityCurve, cavesHeightCurve, overhangsHeightCurve, overhangsDensityCurve;

    void Update()
    {
        if (meshDataReady && !meshRendered)
        {
            CreateMesh();
            UpdateMesh();
        }
    }

    public void Init(UnderwaterTerrain t, Vector3Int pos)
    {
        terrain = t;
        coords = pos;
        transform.position = terrain.terrainCentre + coords * terrain.chunkSize - new Vector3(1f, 0f, 1f) * terrain.chunkSize / 2f;
        transform.parent = terrain.transform;

        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshCollider = gameObject.AddComponent<MeshCollider>();

        heightMapCurve = new AnimationCurve(terrain.heightMapCurve.keys);
        cavesDensityCurve = new AnimationCurve(terrain.cavesDensityCurve.keys);
        overhangsDensityCurve = new AnimationCurve(terrain.overhangsDensityCurve.keys);
        cavesHeightCurve = new AnimationCurve(terrain.cavesHeightCurve.keys);
        overhangsHeightCurve = new AnimationCurve(terrain.overhangsHeightCurve.keys);

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

    float NoiseNew(int x, int y, int z)
    {
        if (y == terrain.terrainHeight)
            return 1f;
        if (y == 0)
            return -1f;

        float noise = 0f, frequency = 0.0025f;

        Vector3 samplePoint = new Vector3((position.x + x + 42.715f), 63.45f, (position.z + z + 86.918f)) * frequency;
        noise = (Utils.Get3DNoise(noiseGenerator, samplePoint, 1) + 1) / 2f;

        noise = heightMapCurve.Evaluate(noise);
        float height = noise * (terrain.terrainHeight - 20f) + 8f;

        samplePoint = new Vector3((position.x + x + 0.086f), 53.355f, (position.z + z + 0.28f)) * frequency;
        noise = (Utils.Get3DNoise(noiseGenerator, samplePoint, 1) + 1) / 2f + 0.1f;
        noise = Mathf.Clamp(noise, 0.8f, 1.1f);
        height *= noise;

        //Height Map
        frequency = 0.01f;
        samplePoint = new Vector3((position.x + x + 0.068f), 53.65f, (position.z + z + 0.072f)) * frequency;
        height += Utils.Get3DNoise(noiseGenerator, samplePoint, 6) * 10f;
        height = Mathf.Clamp(height, 1f, terrain.terrainHeight - 1);

        float returnValue = (y - height) / terrain.terrainHeight;
        
        float scaledHeight = (y - height);
        if (scaledHeight < 0f)
            scaledHeight /= height;
        else
            scaledHeight /= terrain.terrainHeight - height;

        //Overhangs noise

        //Variable frequency
        frequency = 0.0075f;
        samplePoint = new Vector3((position.x + x + 6.443f), y + 6.45343f, (position.z + z + 7.2643f)) * frequency;
        frequency = 0.0125f + Utils.Get3DNoise(noiseGenerator, samplePoint, 1) * 0.006f;

        //Overhang noise calculation
        samplePoint = new Vector3((position.x + x + 3.4523f), y + 0.433f, (position.z + z + 2.347f)) * frequency;
        noise = (Utils.Get3DNoise(noiseGenerator, samplePoint, 1) + 1f) / 2f;

        //Variable amplitude
        frequency = 0.0075f;
        samplePoint = new Vector3((position.x + x + 6.2343f), y + 34.2365f, (position.z + z + 234.43986f)) * frequency;
        float amplitude = 0.15f + Utils.Get3DNoise(noiseGenerator, samplePoint, 1) * 0.025f;

        returnValue -= overhangsHeightCurve.Evaluate((float)height / terrain.terrainHeight) * overhangsDensityCurve.Evaluate(scaledHeight) * noise * amplitude;
        
        //Caves Noise

        frequency = 0.015f;
        samplePoint = new Vector3((position.x + x + 0.0244f), y + 0.735f, (position.z + z + 0.055f)) * frequency;
        noise = (Utils.Get3DNoise(noiseGenerator, samplePoint, 1) + 1f) / 2f;

        returnValue += cavesHeightCurve.Evaluate((float)height / terrain.terrainHeight) * cavesDensityCurve.Evaluate(scaledHeight) * noise * 0.6f;

        return returnValue;
        // return Mathf.Clamp(returnValue, -1f, 1f);
    }

    void GenerateChunkDensityMap()
    {
        chunkDensityMap = new float[terrain.chunkSize + 1, terrain.terrainHeight + 1, terrain.chunkSize + 1];

        for (int x = 0; x <= terrain.chunkSize; x++)
        {
            for (int z = 0; z <= terrain.chunkSize; z++)
            {
                for (int y = 0; y <= terrain.terrainHeight; y++)
                {
                    float noise;

                    // noise = NoiseOld(x, y, z);
                    noise = NoiseNew(x, y, z);

                    chunkDensityMap[x, y, z] = noise;
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

            if (chunkDensityMap[index.x, index.y, index.z] <= 0f)
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
            float valueA = chunkDensityMap[vertA.x, vertA.y, vertA.z];
            float valueB = chunkDensityMap[vertB.x, vertB.y, vertB.z];
            float lerpAmount = Mathf.InverseLerp(valueA, valueB, 0f);

            Vector3 vertexPoint = Vector3.Lerp(vertA, vertB, lerpAmount);

            int vertexIndex = CheckDuplicateVertex(vertexPoint);
            if (vertexIndex == -1)
            {
                vertices.Add(vertexPoint);
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
    }

    void CreateMesh()
    {
        if (mesh == null)
            mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }
}
