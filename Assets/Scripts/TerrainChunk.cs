using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk : MonoBehaviour
{
    public Vector3 worldPosition;
    private Color green;

    private Vector3[] chunkVertices;
    private int[] chunkTriangles;

    private MeshFilter _meshFilter;
    private MeshCollider _meshCollider;

    private Color[] meshColors;

    void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _meshCollider = GetComponent<MeshCollider>();
    }

    public TerrainChunk New(
        Vector3 position,
        Noise noise,
        int maxHeight,
        int chunkLength,
        float waterLevel,
        Color[] colors
    )
    {
        worldPosition = position;

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
                    colors,
                    waterLevel
                ).New();
                for (int q = 0; q < 6; q++)
                {
                    chunkVertices[verticesIndex] = quad.vertices[q];
                    chunkTriangles[verticesIndex] = quad.triangles[q];
                    meshColors[verticesIndex] = quad.meshColors[q];

                    verticesIndex++;
                }
            }
        }

        Mesh mesh = BuildNewMesh();
        ApplyUpdatesToMesh(mesh);

        return this;
    }

    Mesh BuildNewMesh()
    {
        Mesh mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        mesh.vertices = chunkVertices;
        mesh.triangles = chunkTriangles;
        mesh.colors = meshColors;
        mesh.RecalculateNormals();

        return mesh;
    }

    void ApplyUpdatesToMesh(Mesh mesh)
    {
        ApplyColorsToMesh(mesh);
        _meshFilter.mesh = mesh;
        _meshCollider.sharedMesh = mesh;
    }

    void ApplyColorsToMesh(Mesh mesh)
    {
        mesh.colors = meshColors;
    }
}

struct VertMetadata
{
    public float height;
    public Color color;

    public VertMetadata(float height, float normalizedHeight, Color color, string type)
    {
        this.height = height;
        if (type == "land")
        {
            this.color = new Color(
                color.r * normalizedHeight,
                color.g * normalizedHeight,
                color.b * normalizedHeight
            );
        }
        else
        {
            this.color = color;
        }
    }
}

class Quad
{
    public Vector3[] vertices;
    public int[] triangles;
    public Color[] meshColors;

    private Vector3 _worldLocation;
    private Vector3 _relativeLocation;

    private int _maxHeight;
    private int _meshIndex;

    private Noise _noise;
    private Color[] _colors;

    private float _waterLevel;

    public Quad(
        Vector3 worldLocation,
        Vector3 relativeLocation,
        int maxHeight,
        int meshIndex,
        Noise noise,
        Color[] colors,
        float waterLevel
    )
    {
        this._worldLocation = worldLocation;
        this._relativeLocation = relativeLocation;
        this._maxHeight = maxHeight;
        this._meshIndex = meshIndex;
        this._noise = noise;
        this._colors = colors;
        this._waterLevel = waterLevel;
    }

    public Quad New()
    {
        Vector3[] v = new Vector3[]
        {
            this.GetVertPosition(_relativeLocation.x + 1, _relativeLocation.z + 1),
            this.GetVertPosition(_relativeLocation.x + 1, _relativeLocation.z),
            this.GetVertPosition(_relativeLocation.x, _relativeLocation.z + 1),
            this.GetVertPosition(_relativeLocation.x, _relativeLocation.z + 1),
            this.GetVertPosition(_relativeLocation.x + 1, _relativeLocation.z),
            this.GetVertPosition(_relativeLocation.x, _relativeLocation.z)
        };

        // populate triangles and normals
        int[] t = new int[6];
        this.meshColors = new Color[6];
        for (int i = 0; i < 6; i++)
        {
            t[i] = this._meshIndex + i;
            this.meshColors[i] = this.GetVertColor();
        }

        this.vertices = v;
        this.triangles = t;

        return this;
    }

    private VertMetadata GetVertMetadata(float relativeX, float relativeZ)
    {
        float heightSample = this._noise.getNoise(
            new Vector3(this._worldLocation.x + relativeX, 0, this._worldLocation.z + relativeZ),
            "base_height"
        );
        float lakeSample = this._noise.getNoise(
            new Vector3(this._worldLocation.x + relativeX, 0, this._worldLocation.z + relativeZ),
            "lakes"
        );

        bool hasLake = this._waterLevel > lakeSample;
        return new VertMetadata(
            hasLake ? this._waterLevel : heightSample,
            heightSample / this._maxHeight,
            hasLake ? this._colors[1] : this._colors[0],
            hasLake ? "water" : "land"
        );
    }

    private Vector3 GetVertPosition(float vertX, float vertZ)
    {
        VertMetadata vertMetadata = this.GetVertMetadata(vertX, vertZ);

        return new Vector3(vertX, vertMetadata.height, vertZ);
    }

    private Color GetVertColor()
    {
        VertMetadata vertMetadata = this.GetVertMetadata(
            this._relativeLocation.x,
            this._relativeLocation.z
        );
        return vertMetadata.color;
    }
}
