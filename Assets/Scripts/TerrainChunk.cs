using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk : MonoBehaviour
{
    // length of chunk in quads
    public int chunkLength = 100;
    
    private Vector3[] chunkVertices;
    private int[] chunkTriangles;
    private Vector3[] chunkNormals;

    void Start()
    {
        // determine how many vertices we need for the specified chunk size
        // 2 polygons per quad
        int polygonRowCount = chunkLength * 2;
        // 3 vertices per polygon
        int verticesRowCount = polygonRowCount * 3;
        
        int totalVerticesCount = verticesRowCount * chunkLength;
        Debug.Log(totalVerticesCount);

        // create new arrays to contain all of our vertices
        chunkVertices = new Vector3[totalVerticesCount];
        chunkTriangles = new int[totalVerticesCount];
        chunkNormals = new Vector3[totalVerticesCount];

        // populate arrays with vertices
        int iterations = 0;
        for (int x = 0; x < chunkLength; x++) {
            for (int z = 0; z < chunkLength; z++) {
                Quad quad = new Quad(new Vector3(x, 0, z), iterations);
                for (int q = 0; q < 6; q++) {
                    chunkVertices[iterations] = quad.vertices[q];
                    chunkTriangles[iterations] = quad.triangles[q];
                    chunkNormals[iterations] = quad.normals[q];
                    iterations++;
                }
            }
        }
        Mesh mesh = new Mesh();
        mesh.vertices = chunkVertices;
        mesh.triangles = chunkTriangles;
        mesh.normals = chunkNormals;

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}


struct Quad {
    public Vector3[] vertices;
    public int[] triangles;
    public Vector3[] normals;

    public Quad(Vector3 root, int meshIndex) {
        Vector3[] v = new Vector3[] { 
            new Vector3(root.x + 1, 0, root.z + 1),
            new Vector3(root.x + 1, 0, root.z),   
            new Vector3(root.x, 0, root.z + 1),
            new Vector3(root.x, 0, root.z + 1),
            new Vector3(root.x + 1, 0, root.z), 
            new Vector3(root.x, 0, root.z),
        };

        // populate triangles and normals
        int[] t = new int[6];
        Vector3[] n = new Vector3[6];
        for (int i = 0; i < 6; i++) {
            t[i] = meshIndex + i;
            n[i] = Vector3.up;
        }

        this.vertices = v;
        this.triangles = t;
        this.normals = n;
    }
}