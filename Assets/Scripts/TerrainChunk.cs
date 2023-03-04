using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk : MonoBehaviour
{
    public Vector3 worldPosition;
    private Color green;

    private Vector3[] chunkVertices;
    private int[] chunkTriangles;

    private MeshRenderer meshRenderer;

    private Color[] meshColors;

    public TerrainChunk New(Vector3 position, Noise noise, int maxHeight, int chunkLength)
    {
        worldPosition = position;
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

        // populate arrays with vertices
        int verticesIndex = 0;
        for (int x = 0; x < chunkLength; x++)
        {
            for (int z = 0; z < chunkLength; z++)
            {
                Vector3 relativeChunkPosition = new Vector3(x, 0, z);
                Quad quad = new Quad(
                    worldPosition,
                    relativeChunkPosition,
                    maxHeight,
                    verticesIndex,
                    noise,
                    green
                ).New();
                for (int q = 0; q < 6; q++)
                {
                    chunkVertices[verticesIndex] = quad.vertices[q];
                    chunkTriangles[verticesIndex] = quad.triangles[q];
                    meshColors[verticesIndex] = quad.colors[q];

                    verticesIndex++;
                }
            }
        }

        ApplyUpdatesToMesh();
        return this;
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
}

class Quad
{
    public Vector3[] vertices;
    public int[] triangles;
    public Color[] colors;

    private Vector3 worldLocation;
    private Vector3 relativeLocation;

    private int maxHeight;
    private int meshIndex;

    private Noise noise;
    private Color baseColor;

    public Quad(
        Vector3 worldLocation,
        Vector3 relativeLocation,
        int maxHeight,
        int meshIndex,
        Noise noise,
        Color baseColor
    )
    {
        this.worldLocation = worldLocation;
        this.relativeLocation = relativeLocation;
        this.maxHeight = maxHeight;
        this.meshIndex = meshIndex;
        this.noise = noise;
        this.baseColor = baseColor;
    }

    public Quad New()
    {
        Vector3[] v = new Vector3[]
        {
            this.GetVertPosition(relativeLocation.x + 1, relativeLocation.z + 1),
            this.GetVertPosition(relativeLocation.x + 1, relativeLocation.z),
            this.GetVertPosition(relativeLocation.x, relativeLocation.z + 1),
            this.GetVertPosition(relativeLocation.x, relativeLocation.z + 1),
            this.GetVertPosition(relativeLocation.x + 1, relativeLocation.z),
            this.GetVertPosition(relativeLocation.x, relativeLocation.z)
        };

        // populate triangles and normals
        int[] t = new int[6];
        this.colors = new Color[6];
        for (int i = 0; i < 6; i++)
        {
            t[i] = meshIndex + i;
            this.colors[i] = this.GetVertColor();
        }

        this.vertices = v;
        this.triangles = t;

        return this;
    }

    private float SampleNoise(string channel, float relativeX, float relativeZ)
    {
        return this.noise.getNoise(
            new Vector3(this.worldLocation.x + relativeX, 0, this.worldLocation.z + relativeZ),
            channel
        );
    }

    private Vector3 GetVertPosition(float vertX, float vertZ)
    {
        float height = this.SampleNoise("base_height", vertX, vertZ);

        return new Vector3(vertX, height, vertZ);
    }

    private Color GetVertColor()
    {
        float height = this.SampleNoise(
            "base_height",
            this.relativeLocation.x,
            this.relativeLocation.z
        );
        float normalizedHeight = height / maxHeight;
        return new Color(
            this.baseColor.r * normalizedHeight,
            this.baseColor.g * normalizedHeight,
            this.baseColor.b * normalizedHeight
        );
    }
}
