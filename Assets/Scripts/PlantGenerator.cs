using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PlantGenerator : MonoBehaviour
{
    public Material plantMaterial;
    public float stemRadius, plantHeight;
    [Range(2, 30)]
    public int verticalResolution;
    [Range(4, 30)]
    public int circularResolution;
    public float[] lobeRadii;
    [Range(0, 10)]
    public int lobesStartOffset;
    public Color plantColor, spotsColor;

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;
    Mesh mesh;
    Vector3[] vertices;
    Vector2[] uvs;
    Color[] colors;
    int[] triangles;

    void OnValidate ()
    {
        if (Application.isPlaying)
            return;
        if (mesh == null)
        {
            GenerateMesh();
            CreateMesh();
            UpdateMesh();
        }
        else
        {
            ClearMesh();
            GenerateMesh();
            CreateMesh();
            UpdateMesh();
        }
    }

    void GenerateMesh()
    {
        vertices = new Vector3 [verticalResolution * circularResolution + 1];
        triangles = new int [(verticalResolution - 1) * circularResolution * 6 + circularResolution * 3];
        colors = new Color [vertices.Length];
        uvs = new Vector2 [vertices.Length];
        int vertexIndex = 0, triIndex = 0;

        // Create Stem Vertices
        for (int i=0; i<verticalResolution; i++)
        {
            float height = (float)i / (verticalResolution - 1) * plantHeight;
            Vector3 centre = new Vector3 (0f, height, 0f);

            for (int j = 0; j < circularResolution; j++)
            {
                float theta = j * (360f / circularResolution) * Mathf.Deg2Rad;
                Vector3 point = centre + new Vector3 (Mathf.Cos(theta), 0f, Mathf.Sin(theta)) * stemRadius;
                vertices[vertexIndex] = point;
                colors[vertexIndex++] = plantColor;
                uvs[vertexIndex] = new Vector2 (0, 0);
            }

            if (i == verticalResolution - 1)
            {
                vertices[vertexIndex] = centre + Vector3.up * plantHeight * 0.025f;
                colors[vertexIndex] = plantColor;
                uvs[vertexIndex] = new Vector2 (0, 0);
            }
        }


        // Create Stem triangles
        for (int i=0; i<verticalResolution - 1; i++)
        {
            int index;
            for (int j = 0; j < circularResolution - 1; j++)
            {
                index = j + i * circularResolution;
                triangles[triIndex] = index;
                triangles[triIndex + 1] = index + circularResolution;
                triangles[triIndex + 2] = index + 1;
                triangles[triIndex + 3] = index + 1;
                triangles[triIndex + 4] = index + circularResolution;
                triangles[triIndex + 5] = index + circularResolution + 1;
                triIndex += 6;
            }

            // Set manually for the last vertex in circle
            index = i * circularResolution + circularResolution - 1;
            triangles[triIndex] = index;
            triangles[triIndex + 1] = index + circularResolution;
            triangles[triIndex + 2] = index - circularResolution + 1;
            triangles[triIndex + 3] = index - circularResolution + 1;
            triangles[triIndex + 4] = index + circularResolution;
            triangles[triIndex + 5] = index + 1;
            triIndex += 6;
        }

        // Fill Top part
        for (int i=0; i < circularResolution - 1; i++)
        {
            int index = (verticalResolution - 1) * circularResolution + i;
            triangles[triIndex] = index;
            triangles[triIndex + 1] = vertexIndex;
            triangles[triIndex + 2] = index + 1;
            triIndex += 3;
        }

        triangles[triIndex] = verticalResolution * circularResolution - 1;
        triangles[triIndex + 1] = vertexIndex;
        triangles[triIndex + 2] = (verticalResolution - 1) * circularResolution;
        triIndex += 3;

        // Create Lobes
        for (int i = lobesStartOffset; i < verticalResolution; i++)
        {
            float height = (float)i / (verticalResolution - 1) * plantHeight;
            Vector3 centre = new Vector3 (0f, height, 0f);
            float range = verticalResolution - lobesStartOffset - 1;
            float frequency = range / lobeRadii.Length;
            float input = (i - lobesStartOffset) / frequency;
            int lobeNumber = Mathf.FloorToInt((i - lobesStartOffset) * lobeRadii.Length / (verticalResolution - lobesStartOffset));
            input -= Mathf.Floor(input);
            float amplitude = Mathf.Sin(Mathf.PI * input) * lobeRadii[lobeNumber];
            
            for (int j=0; j < circularResolution; j++)
            {
                int index = index = j + i * circularResolution;
                vertices[index] += (vertices[index] - centre).normalized * amplitude;
            }
        }

        
    }

    void UpdateMesh()
    {
        if (meshFilter == null)
        {
            meshFilter = gameObject.GetComponent<MeshFilter>();
            if (meshFilter == null)
                meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (meshRenderer == null)
                meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        if (meshCollider == null)
        {
            meshCollider = gameObject.GetComponent<MeshCollider>();
            if (meshCollider == null)
                meshCollider = gameObject.AddComponent<MeshCollider>();
        }
        meshFilter.sharedMesh = mesh;
        meshRenderer.sharedMaterial = plantMaterial;
        meshCollider.sharedMesh = mesh;
    }

    void ClearMesh()
    {
        mesh.Clear();
        vertices = null;
        triangles = null;
        colors = null;
        uvs = null;
    }

    void CreateMesh()
    {
        if (mesh == null)
            mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
    }

    void CreateSpots ()
    {
        // Create spots
        for (int i=0; i < 5; i++)
        {
            int randomHeight = Random.Range(1 + verticalResolution / lobeRadii.Length, verticalResolution - 1);
            int j = Random.Range(1, circularResolution - 1) + randomHeight * circularResolution;
            colors[j] = spotsColor;
            colors[j + circularResolution] = spotsColor;
            colors[j - circularResolution] = spotsColor;
            colors[j + 1] = spotsColor;
            colors[j + 1 - circularResolution] = spotsColor;
            colors[j + 1 + circularResolution] = spotsColor;
            colors[j - 1] = spotsColor;
            colors[j - 1 - circularResolution] = spotsColor;
            colors[j - 1 + circularResolution] = spotsColor;
            uvs[j] = new Vector2 (1, 1f);
            uvs[j + circularResolution] = new Vector2 (1, 0.8f);
            uvs[j - circularResolution] = new Vector2 (1, 0.8f);
            uvs[j + 1] = new Vector2 (1, 0.8f);
            uvs[j + 1 - circularResolution] = new Vector2 (1, 0.6f);
            uvs[j + 1 + circularResolution] = new Vector2 (1, 0.6f);
            uvs[j - 1] = new Vector2 (1, 0.8f);
            uvs[j - 1 - circularResolution] = new Vector2 (1, 0.6f);
            uvs[j - 1 + circularResolution] = new Vector2 (1, 0.6f);
        }
    }

    void Start ()
    {
        if (!Application.isPlaying)
            return;

        GenerateMesh();
        CreateSpots();
        CreateMesh();
        UpdateMesh();
    }
}
