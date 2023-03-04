using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    [SerializeField]
    GameObject chunk;

    [SerializeField]
    int drawDistance = 10;

    private PlayerManager playerManager;
    private Dictionary<int, Dictionary<int, TerrainChunk>> chunks;
    private Noise terrainNoise;

    private int maxHeight = 6;
    private int scale = 15;
    private int chunkLength = 32;

    // Start is called before the first frame update
    void Start()
    {
        terrainNoise = new Noise();
        terrainNoise.addChannel(
            new Channel(
                "base_height",
                Algorithm.Perlin3d,
                scale,
                NoiseStyle.Second,
                1f,
                maxHeight,
                Edge.Smooth
            ).setFractal(4, 1.0f, 0.5f)
        );
        terrainNoise.addChannel(
            new Channel(
                "Lakes",
                Algorithm.Simplex2d,
                scale * 100,
                NoiseStyle.Second,
                1f,
                maxHeight,
                Edge.Smooth
            )
        );
        chunks = new Dictionary<int, Dictionary<int, TerrainChunk>>();
        playerManager = FindObjectOfType<PlayerManager>(); // TODO: use player transform.forward to only generate chunks in front of player
        Vector3 playerPosition = playerManager.GetPosition();
        GenerateMapAroundPosition(playerPosition);
    }

    void Update() { }

    void GenerateMapAroundPosition(Vector3 position)
    {
        Vector3 worldPosition = new Vector3(position.x / chunkLength, 0, position.z / chunkLength);
        for (
            int x = Mathf.FloorToInt(worldPosition.x - drawDistance / 2);
            x < Mathf.FloorToInt(worldPosition.x + drawDistance / 2);
            x++
        )
        {
            Dictionary<int, TerrainChunk> xChunks = new Dictionary<int, TerrainChunk>();
            if (xChunks.ContainsKey(x))
            {
                xChunks = chunks[x];
            }
            for (
                int z = Mathf.FloorToInt(worldPosition.z - drawDistance / 2);
                z < Mathf.FloorToInt(worldPosition.z + drawDistance / 2);
                z++
            )
            {
                // check if chunk already exists
                if (xChunks.ContainsKey(z))
                {
                    break;
                }
                GameObject chunkObject = Instantiate(
                    chunk,
                    new Vector3(x * chunkLength, 0, z * chunkLength),
                    Quaternion.identity
                );
                chunkObject.transform.SetParent(transform);
                xChunks[z] = chunkObject
                    .GetComponent<TerrainChunk>()
                    .New(
                        new Vector3(x * chunkLength, 0, z * chunkLength),
                        terrainNoise,
                        maxHeight,
                        chunkLength
                    );
            }
            chunks[x] = xChunks;
        }
    }
}
