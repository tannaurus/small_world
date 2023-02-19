using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk : MonoBehaviour
{
    // length of chunk in quads
    public int chunkLength = 100;
    public float scale = 1f;

    private Vector3[] chunkVertices;
    private int[] chunkTriangles;
    private Vector3[] chunkNormals;

    private Noise terrainNoise;
    private Texture2D terrainTexture;
    private MeshRenderer meshRenderer;

    void Start()
    {
        // determine how many vertices we need for the specified chunk size
        InitializeMeshStorage();

        // initialize map texture
        terrainTexture = new Texture2D(chunkLength, chunkLength, TextureFormat.ARGB32, true);
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material.mainTexture = terrainTexture;

        // initialize noise
        terrainNoise = new Noise();
        terrainNoise.addChannel(
            new Channel(
                "Height",
                Algorithm.Turbulence3d,
                150.0f,
                NoiseStyle.Linear,
                0.0f,
                1.0f,
                Edge.Smooth
            ).setFractal(4, 2.0f, 0.5f)
        );

        // populate arrays with vertices
        int iterations = 0;
        for (int x = 0; x < chunkLength; x++)
        {
            for (int z = 0; z < chunkLength; z++)
            {
                Quad quad = new Quad(new Vector3(x, 0, z), iterations, terrainNoise, scale);
                for (int q = 0; q < 6; q++)
                {
                    chunkVertices[iterations] = quad.vertices[q];
                    chunkTriangles[iterations] = quad.triangles[q];
                    chunkNormals[iterations] = quad.normals[q];
                    iterations++;
                }
                // TODO: move height calculation for verts up here too
                // float sample = terrainNoise.getNoise(new Vector3(x, 0, z), "Height") * scale;
                terrainTexture.SetPixel(x, z, Color.red);
            }
        }

        ApplyUpdatesToMesh();
    }

    void InitializeMeshStorage()
    {
        // 2 polygons per quad
        int polygonRowCount = chunkLength * 2;
        // 3 vertices per polygon
        int verticesRowCount = polygonRowCount * 3;

        int totalVerticesCount = verticesRowCount * chunkLength;

        // create new arrays to contain all of our vertices
        chunkVertices = new Vector3[totalVerticesCount];
        chunkTriangles = new int[totalVerticesCount];
        chunkNormals = new Vector3[totalVerticesCount];
    }

    void ApplyUpdatesToMesh()
    {
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = chunkVertices;
        mesh.triangles = chunkTriangles;
        mesh.normals = chunkNormals;

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;

        terrainTexture.Apply();
    }
}

class Quad
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector3[] normals;

    public Quad(Vector3 root, int meshIndex, Noise terrainNoise, float scale)
    {
        Vector3[] v = new Vector3[]
        {
            this.GetVertPosition(root.x + 1, root.z + 1, terrainNoise, scale),
            this.GetVertPosition(root.x + 1, root.z, terrainNoise, scale),
            this.GetVertPosition(root.x, root.z + 1, terrainNoise, scale),
            this.GetVertPosition(root.x, root.z + 1, terrainNoise, scale),
            this.GetVertPosition(root.x + 1, root.z, terrainNoise, scale),
            this.GetVertPosition(root.x, root.z, terrainNoise, scale)
        };

        // populate triangles and normals
        int[] t = new int[6];
        Vector3[] n = new Vector3[6];
        for (int i = 0; i < 6; i++)
        {
            t[i] = meshIndex + i;
            n[i] = Vector3.right;
        }

        this.vertices = v;
        this.triangles = t;
        this.normals = n;
    }

    private Vector3 GetVertPosition(float x, float z, Noise terrainNoise, float scale)
    {
        float height = terrainNoise.getNoise(new Vector3(x, 0, z), "Height") * scale;
        return new Vector3(x, height, z);
    }
}
