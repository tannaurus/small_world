using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    [SerializeField]
    GameObject chunk;

    [SerializeField]
    PlayerManager player;

    [SerializeField]
    int initialWorldSize = 3;

    private int chunkLength;

    private List<List<TerrainChunk>> chunks;
    private Noise terrainNoise;

    public int maxHeight = 6;
    public int scale = 15;

    // Start is called before the first frame update
    void Start()
    {
        terrainNoise = new Noise();
        terrainNoise.addChannel(
            new Channel(
                "Height",
                Algorithm.Perlin3d,
                scale,
                NoiseStyle.Second,
                1f,
                maxHeight,
                Edge.Smooth
            ).setFractal(4, 1.0f, 0.5f)
        );
        chunkLength = chunk.GetComponent<TerrainChunk>().chunkLength;
        chunks = new List<List<TerrainChunk>>();

        for (int x = 0; x < initialWorldSize; x++)
        {
            List<TerrainChunk> xChunks = new List<TerrainChunk>();
            for (int z = 0; z < initialWorldSize; z++)
            {
                TerrainChunk spawnedChunk = Instantiate(
                        chunk,
                        new Vector3(x * chunkLength, 0, z * chunkLength),
                        Quaternion.identity
                    )
                    .GetComponent<TerrainChunk>();
                spawnedChunk.GenerateChunk(
                    new Vector3(x * chunkLength, 0, z * chunkLength),
                    terrainNoise,
                    maxHeight
                );
                xChunks.Insert(z, spawnedChunk);
            }
            chunks.Insert(x, xChunks);
        }
    }
}
