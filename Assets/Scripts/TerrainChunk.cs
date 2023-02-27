using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk : MonoBehaviour
{
    // length of chunk in quads
    public int chunkLength = 32;

    private Color green;

    private Vector3[] chunkVertices;
    private int[] chunkTriangles;

    private MeshRenderer meshRenderer;

    private Color[] meshColors;

    void Awake()
    {
        green = new Color(106f / 255, 192f / 255, 82f / 255);
        // determine how many vertices we need for the specified chunk size
        // 2 polygons per quad
        int polygonRowCount = chunkLength * 2;
        // 3 vertices per polygon
        int verticesRowCount = polygonRowCount * 3;

        int totalVerticesCount = verticesRowCount * chunkLength;

        // create new arrays to contain all of our vertices
        chunkVertices = new Vector3[totalVerticesCount];
        chunkTriangles = new int[totalVerticesCount];
        meshColors = new Color[totalVerticesCount];

        // initialize map texture
        meshRenderer = GetComponent<MeshRenderer>();
    }

    void ApplyUpdatesToMesh()
    {
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = chunkVertices;
        mesh.triangles = chunkTriangles;
        mesh.colors = meshColors;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

    public void GenerateChunk(Vector3 position, Noise terrainNoise, int terrainMaxHeight)
    {
        // populate arrays with vertices
        int iterations = 0;
        for (int x = 0; x < chunkLength; x++)
        {
            for (int z = 0; z < chunkLength; z++)
            {
                Quad quad = new Quad(
                    new Vector3(x + position.x, 0, z + position.z),
                    iterations,
                    terrainNoise
                );
                for (int q = 0; q < 6; q++)
                {
                    chunkVertices[iterations] = quad.vertices[q];
                    chunkTriangles[iterations] = quad.triangles[q];
                    float sample = terrainNoise.getNoise(
                        new Vector3(x + position.x, 0, z + position.z),
                        "Height"
                    );
                    float normalizedSample = sample / terrainMaxHeight;
                    meshColors[iterations] = new Color(
                        green.r * normalizedSample,
                        green.g * normalizedSample,
                        green.b * normalizedSample
                    );
                    iterations++;
                }
            }
        }

        ApplyUpdatesToMesh();
    }
}

class Quad
{
    public Vector3[] vertices;
    public int[] triangles;

    public Quad(Vector3 root, int meshIndex, Noise terrainNoise)
    {
        Vector3[] v = new Vector3[]
        {
            this.GetVertPosition(root.x + 1, root.z + 1, terrainNoise),
            this.GetVertPosition(root.x + 1, root.z, terrainNoise),
            this.GetVertPosition(root.x, root.z + 1, terrainNoise),
            this.GetVertPosition(root.x, root.z + 1, terrainNoise),
            this.GetVertPosition(root.x + 1, root.z, terrainNoise),
            this.GetVertPosition(root.x, root.z, terrainNoise)
        };

        // populate triangles and normals
        int[] t = new int[6];
        Vector3[] n = new Vector3[6];
        for (int i = 0; i < 6; i++)
        {
            t[i] = meshIndex + i;
        }

        this.vertices = v;
        this.triangles = t;
    }

    private Vector3 GetVertPosition(float x, float z, Noise terrainNoise)
    {
        float height = terrainNoise.getNoise(new Vector3(x, 0, z), "Height");
        return new Vector3(x, height, z);
    }
}
