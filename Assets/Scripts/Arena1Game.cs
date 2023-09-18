using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Arena1Game : NetworkBehaviour
{
    public Player playerPrefab;
    public Camera arenaCamera;

    private int positionIndex = 0;
    private Vector3[] startPositions = new Vector3[]
    {
        new Vector3(4, 2, 0),
        new Vector3(-4, 2, 0),
        new Vector3(0, 2, 4),
        new Vector3(0, 2, -4)
    };

    private void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            SpawnPlayers();
        }
    }

    private Vector3 NextPosition() {
        Vector3 pos = startPositions[positionIndex];
        positionIndex += 1;
        if (positionIndex > startPositions.Length - 1) {
            positionIndex = 0;
        }
        return pos;
    }

    private void SpawnPlayers()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogWarning("SpawnPlayers should only be called on the server.");
            return;
        }

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Player playerSpawn = Instantiate(playerPrefab, NextPosition(), Quaternion.identity);
            NetworkObject networkObject = playerSpawn.GetComponent<NetworkObject>();
            networkObject.SpawnWithOwnership(clientId);

            AudioListener audioListener = playerSpawn.GetComponentInChildren<AudioListener>();
            if (audioListener)
            {
                audioListener.enabled = networkObject.IsOwner;
            }
        }
    }
}