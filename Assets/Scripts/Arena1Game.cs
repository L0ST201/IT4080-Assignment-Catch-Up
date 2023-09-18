using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Arena1Game : NetworkBehaviour
{
    public Player playerPrefab;
    public Camera arenaCamera;

    private void Start()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            SpawnPlayers();
        }
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
            Player playerSpawn = Instantiate(playerPrefab, playerPrefab.transform.position, Quaternion.identity);
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
