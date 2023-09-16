using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Arena1Game : NetworkBehaviour
{
    private int positionIndex = 0;
    private Vector3[] startPositions = new Vector3[]
    {
        new Vector3(4, 2, 0),
        new Vector3(-4, 2, 0),
        new Vector3(0, 2, 4),
        new Vector3(0, 2, -4)
    };

    private int colorIndex = 0;
    private Color[] playerColors = new Color[] {
        Color.blue,
        Color.green,
        Color.yellow,
        Color.magenta,
    };

    public GameObject playerPrefab; 

    private Vector3 NextPosition()
    {
        Vector3 pos = startPositions[positionIndex];
        positionIndex += 1;
        if (positionIndex > startPositions.Length - 1)
        {
            positionIndex = 0;
        }
        return pos;
    }

    private Color NextColor()
    {
        Color newColor = playerColors[colorIndex];
        colorIndex += 1;
        if (colorIndex > playerColors.Length - 1)
        {
            colorIndex = 0;
        }
        return newColor;
    }

    [ServerRpc]
    public void RequestSpawnPlayerServerRpc(ulong clientId)
    {
        Vector3 spawnPosition = NextPosition();
        Color assignedColor = NextColor();

        GameObject playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
        PlayerController playerController = playerInstance.GetComponent<PlayerController>();

        playerController.playerColor.Value = assignedColor;

        playerInstance.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
    }
}
