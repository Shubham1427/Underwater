using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OceanChunk : MonoBehaviour
{
    Transform player;
    Ocean ocean;
    Vector3Int coords;
    int resolution;
    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    Mesh mesh;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    bool top;

    // Start is called before the first frame update
    public void Init(Ocean _ocean, Vector3Int pos, Transform _player, int _resolution, bool topView)
    {
        ocean = _ocean;
        coords = pos;
        player = _player;
        resolution = _resolution;
        top = topView;

        transform.parent = ocean.transform;

        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        GenerateMeshData();
        CreateMesh();
        UpdateMesh();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = coords * ocean.chunkSize - new Vector3(ocean.chunkSize / 2f, 0f, ocean.chunkSize / 2f) + new Vector3(player.position.x, ocean.oceanLevel, player.position.z);
    }

    void GenerateMeshData()
    {
        int vertexIndex = 0;
        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                Vector3 percent = new Vector3(i, 0, j) / (resolution - 1);
                vertices.Add(percent * ocean.chunkSize);

                if (i != resolution - 1 && j != resolution - 1)
                {
                    if (top)
                    {
                        triangles.Add(vertexIndex);
                        triangles.Add(vertexIndex + 1);
                        triangles.Add(vertexIndex + resolution);
                        triangles.Add(vertexIndex + resolution);
                        triangles.Add(vertexIndex + 1);
                        triangles.Add(vertexIndex + resolution + 1);
                    }
                    else
                    {
                        triangles.Add(vertexIndex);
                        triangles.Add(vertexIndex + resolution);
                        triangles.Add(vertexIndex + 1);
                        triangles.Add(vertexIndex + 1);
                        triangles.Add(vertexIndex + resolution);
                        triangles.Add(vertexIndex + resolution + 1);
                    }
                }
                vertexIndex++;
            }
        }
    }

    void UpdateMesh()
    {
        meshFilter.mesh = mesh;
        if (top)
            meshRenderer.material = ocean.oceanUpMaterial;
        else
            meshRenderer.material = ocean.oceanDownMaterial;
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
