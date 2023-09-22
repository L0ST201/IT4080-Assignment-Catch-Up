using UnityEngine;
using Unity.Netcode;

public class Arena1Game : NetworkBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private Player _playerPrefab;
    [SerializeField] private Player _playerWithAuraPrefab;

    [Header("Game Elements")]
    [SerializeField] private Camera _arenaCamera;

    private int _positionIndex = 0;
    private static readonly Vector3[] _startPositions = 
    {
        new Vector3(4, 2, 0),
        new Vector3(-4, 2, 0),
        new Vector3(0, 2, 4),
        new Vector3(0, 2, -4)
    };

    private int _colorIndex = 0;
    private static readonly Color[] _playerColors = 
    {
        Color.blue,
        Color.green,
        Color.yellow,
        Color.magenta,
    };

    void Start()
    {
        SetCameraAndListenerState();

        if (NetworkManager.Singleton.IsServer)
        {
            SpawnPlayers();
        }
        else
        {
            Debug.Log("Not a server. Players not spawned.");
        }

    }

    private void SetCameraAndListenerState()
    {
        _arenaCamera.enabled = !IsClient;
        _arenaCamera.GetComponent<AudioListener>().enabled = !IsClient;
    }

    private Vector3 NextPosition() 
    {
        Vector3 pos = _startPositions[_positionIndex];
        _positionIndex = (_positionIndex + 1) % _startPositions.Length;
        return pos;
    }

    private Color NextColor() 
    {
        Color newColor = _playerColors[_colorIndex];
        _colorIndex = (_colorIndex + 1) % _playerColors.Length;
        return newColor;
    }

    private void SpawnPlayers()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {   
            Player playerSpawn;
            if (clientId == NetworkManager.ServerClientId)
            {
                playerSpawn = Instantiate(_playerWithAuraPrefab, NextPosition(), Quaternion.identity);
            }
            else
            {
                playerSpawn = Instantiate(_playerPrefab, NextPosition(), Quaternion.identity);
            }

            playerSpawn.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
            playerSpawn.NetworkedColorProperty.Value = NextColor(); // Using the property to access NetworkedColor
        }
    }
}
