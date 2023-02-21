using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk : MonoBehaviour
{
    // length of chunk in quads
    public int chunkLength = 100;
    public float scale = 1f;

    public Color green = new Color(116, 192, 92);

    private Vector3[] chunkVertices;
    private int[] chunkTriangles;
    private Vector3[] chunkNormals;

    private Noise terrainNoise;
    private Texture2D terrainTexture;
    private MeshRenderer meshRenderer;

    public float terrainMaxHeight = 3f;
    public float terrainScale = 10f;

    void Start()
    {
        // determine how many vertices we need for the specified chunk size
        // 2 polygons per quad
        int polygonRowCount = chunkLength * 2;
        // 3 vertices per polygon
        int verticesRowCount = polygonRowCount * 3;

        int totalVerticesCount = verticesRowCount * chunkLength;

        // create new arrays to contain all of our vertices
        chunkVertices = new Vector3[totalVerticesCount];
        chunkTriangles = new int[totalVerticesCount];

        // initialize map texture
        terrainTexture = new Texture2D(
            verticesRowCount,
            verticesRowCount,
            TextureFormat.ARGB32,
            true
        );
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material.mainTexture = terrainTexture;

        // initialize noise
        terrainNoise = new Noise();
        terrainNoise.addChannel(
            new Channel(
                "Height",
                Algorithm.Turbulence3d,
                terrainScale,
                NoiseStyle.Linear,
                0.0f,
                terrainMaxHeight,
                Edge.Smooth
            ).setFractal(4, 2.0f, 0.5f)
        );

        // populate arrays with vertices
        int iterations = 0;
        for (int x = 0; x < chunkLength; x++)
        {
            for (int z = 0; z < chunkLength; z++)
            {
                Quad quad = new Quad(new Vector3(x, 0, z), iterations, terrainNoise);
                for (int q = 0; q < 6; q++)
                {
                    chunkVertices[iterations] = quad.vertices[q];
                    chunkTriangles[iterations] = quad.triangles[q];
                    iterations++;
                }
                // TODO: move height calculation for verts up here too
                float sample = terrainNoise.getNoise(new Vector3(x, 0, z), "Height");
                float normalizedSample = sample / terrainMaxHeight;
                Color pixelColor = Color.red;
                Debug.Log(normalizedSample);
                Debug.Log(terrainMaxHeight);
                if (normalizedSample < terrainMaxHeight)
                {
                    pixelColor = Color.red;
                }
                terrainTexture.SetPixel(x, z, pixelColor);
            }
        }

        ApplyUpdatesToMesh();
    }

    void ApplyUpdatesToMesh()
    {
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = chunkVertices;
        mesh.triangles = chunkTriangles;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;

        terrainTexture.Apply();
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
