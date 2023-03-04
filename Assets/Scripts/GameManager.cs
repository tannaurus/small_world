using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    PlayerManager playerManager;
    TerrainManager terrainManager;

    void Start()
    {
        playerManager = FindObjectOfType<PlayerManager>();
        terrainManager = FindObjectOfType<TerrainManager>();
    }
}
