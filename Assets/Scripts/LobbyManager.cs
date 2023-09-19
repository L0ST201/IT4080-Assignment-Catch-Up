using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using UnityEngine.Networking;

public class LobbyManager : NetworkBehaviour
{
    public Button startButton;
    public TMPro.TMP_Text statusLabel;

    void Start()
    {
        InitializeLobby();
        
        startButton.onClick.AddListener(OnStartButtonClicked);
        NetworkManager.Singleton.OnClientStarted += OnClientStarted;
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
    }

    private void InitializeLobby()
    {
        startButton.gameObject.SetActive(false);
        statusLabel.text = "Start something, like the server or the host or the client.";
    }

    private void OnClientStarted()
    {
        if (!IsHost)
        {
            statusLabel.text = "Waiting for host to start the game";
        }
    }

    private void OnServerStarted()
    {
        startButton.gameObject.SetActive(true);
        statusLabel.text = "You are the host, please press start game when you are ready";
    }

    private void OnStartButtonClicked()
    {
        Debug.Log("Start Game button clicked");
        if (NetworkManager.Singleton.IsServer)
        {
            Debug.Log("This instance recognizes itself as a server");
            StartGame();
            StartGameClientRpc();
        }
        else
        {
            Debug.Log("This instance does not recognize itself as a server");
        }
    }

    [ServerRpc]
    public void StartGameServerRpc()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.LogError("StartGameServerRpc was called, but this instance is not a server.");
            return;
        }
        Debug.Log("Inside StartGameServerRpc on server");
        StartGame();
        StartGameClientRpc();
    }


    [ClientRpc]
    public void StartGameClientRpc()
    {
        Debug.Log("Inside StartGameClientRpc on client");
        StartGame();
    }

    public void StartGame()
    {
        Debug.Log("Attempting to load ArenaOne scene");
        UnityEngine.SceneManagement.SceneManager.LoadScene("ArenaOne", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public void OnQuitGameButtonClicked()
    {
        startButton.gameObject.SetActive(false);
        statusLabel.text = "Start something, like the server or the host or the client.";
    }
}
