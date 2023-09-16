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
        StartGame();
    }

    public void StartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("ArenaOne", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public void OnQuitGameButtonClicked()
    {
        startButton.gameObject.SetActive(false);
        statusLabel.text = "Start something, like the server or the host or the client.";
    }
}
