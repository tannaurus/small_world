using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public Vector3 GetChunkPosition(int chunkSize)
    {
        return new Vector3(transform.position.x / chunkSize, 0, transform.position.z / chunkSize);
    }
}
