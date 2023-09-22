using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.UI;

public class NetworkHelper : MonoBehaviour
{
    public Button hostButton;
    public Button clientButton;
    public Button serverButton;
    public Button shutdownButton;
    public Text statusText;

    public NetworkHandler networkHandler;
    public LobbyManager lobbyManager;

    private void Start()
    {
        if (hostButton != null)
            hostButton.onClick.AddListener(() => NetworkManager.Singleton.StartHost());
        if (clientButton != null)
            clientButton.onClick.AddListener(() => NetworkManager.Singleton.StartClient());
        if (serverButton != null)
            serverButton.onClick.AddListener(() => NetworkManager.Singleton.StartServer());

        if (shutdownButton != null)
        {
            shutdownButton.onClick.AddListener(() => 
            {
                Debug.Log("Shutdown button pressed");
                if (networkHandler != null)
                {
                    networkHandler.ShutdownServer();
                }
                else
                {
                    NetworkManager.Singleton.Shutdown();
                }

                if (lobbyManager != null)
                {
                    lobbyManager.OnQuitGameButtonClicked();
                }
            });
        }
    }

    private void Update()
    {
        UpdateButtonVisibility();
        UpdateStatusText();
    }

    private void UpdateButtonVisibility()
    {
        if (NetworkManager.Singleton != null && hostButton != null && clientButton != null && serverButton != null && shutdownButton != null)
        {
            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                hostButton.gameObject.SetActive(true);
                clientButton.gameObject.SetActive(true);
                serverButton.gameObject.SetActive(true);
                shutdownButton.gameObject.SetActive(false);
            }
            else
            {
                hostButton.gameObject.SetActive(false);
                clientButton.gameObject.SetActive(false);
                serverButton.gameObject.SetActive(false);
                shutdownButton.gameObject.SetActive(true);
            }
        }
    }

    private void UpdateStatusText()
    {
        if (statusText == null)
            return;

        string transportTypeName = NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name;
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        string serverPort = "?";
        if (transport != null)
        {
            serverPort = $"{transport.ConnectionData.Address}:{transport.ConnectionData.Port}";
        }

        string mode = NetworkManager.Singleton.IsHost ?
            "Host" : NetworkManager.Singleton.IsServer ? "Server" : "Client";

        statusText.text = $"Transport: {transportTypeName} [{serverPort}]\nMode: {mode}";
    }
}
