using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

public class LobbyManager : NetworkBehaviour
{
    [Header("UI Elements")]
    public Button startButton;
    public TMP_Text statusLabel;

    private void Start()
    {
        InitializeLobby();
        
        // Add event listeners
        startButton.onClick.AddListener(OnStartButtonClicked);
        NetworkManager.OnClientStarted += OnClientStarted;
        NetworkManager.OnServerStarted += OnServerStarted;
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
        NetworkManager.SceneManager.LoadScene("TestChat", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public void OnQuitGameButtonClicked()
    {
        startButton.gameObject.SetActive(false);
        statusLabel.text = "Start something, like the server or the host or the client.";
    }

   private new void OnDestroy() 
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveListener(OnStartButtonClicked);
        }

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientStarted -= OnClientStarted;
            NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
        }
    }

}