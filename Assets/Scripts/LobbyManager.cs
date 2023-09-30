using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

// Later uncomment the startbuttons in start, initalizeLobby, OnServerStarted,
// and also remove the OnStartButtonClicked method within OnServerStarted to
// restore the original functionality of the start button on Lobby page.
public class LobbyManager : NetworkBehaviour
{
    [Header("UI Elements")]
    public Button startButton;
    public TMP_Text statusLabel;

    private void Start()
    {
        InitializeLobby();
        
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
            StartGameServerRpc();
        }
        else
        {
            Debug.Log("This instance does not recognize itself as a server");
        }
    }

    [ServerRpc]
    public void StartGameServerRpc()
    {
        Debug.Log("Inside StartGameServerRpc on server");
        StartGame();
        StartGameClientRpc();
    }

    [ClientRpc]
    public void StartGameClientRpc()
    {
        Debug.Log("Inside StartGameClientRpc on client");
        // Will put client-specific logic here.
    }

    public void StartGame()
    {
        Debug.Log("Attempting to load ArenaOne/TestChat scene");
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.SceneManager.LoadScene("ArenaOne", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
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