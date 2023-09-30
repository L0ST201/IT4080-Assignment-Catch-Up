using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine.UI;
using UnityEngine;

public class NetworkHelper : MonoBehaviour
{
    public Button hostButton;
    public Button clientButton;
    public Button serverButton;
    public Button shutdownButton;
    public Text statusText;
    public Text clientIdText; // New text UI to display the client's ID

    public NetworkHandler networkHandler;
    public LobbyManager lobbyManager;
    private NetworkManager _netMgr;

    private void Start()
    {
        _netMgr = NetworkManager.Singleton;
        if (hostButton != null)
            hostButton.onClick.AddListener(() => _netMgr.StartHost());
        if (clientButton != null)
            clientButton.onClick.AddListener(() => _netMgr.StartClient());
        if (serverButton != null)
            serverButton.onClick.AddListener(() => _netMgr.StartServer());

        if (shutdownButton != null)
        {
            shutdownButton.onClick.AddListener(() => 
            {
                Log("Shutdown button pressed");
                if (networkHandler != null)
                {
                    networkHandler.ShutdownServer();
                }
                else
                {
                    _netMgr.Shutdown();
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
        UpdateClientIdText(); // New method to update the client's ID text
    }

    private void UpdateButtonVisibility()
    {
        if (_netMgr != null && hostButton != null && clientButton != null && serverButton != null && shutdownButton != null)
        {
            if (!_netMgr.IsClient && !_netMgr.IsServer)
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

        string transportTypeName = _netMgr.NetworkConfig.NetworkTransport.GetType().Name;
        UnityTransport transport = _netMgr.GetComponent<UnityTransport>();
        string serverPort = "?";
        if (transport != null)
        {
            serverPort = $"{transport.ConnectionData.Address}:{transport.ConnectionData.Port}";
        }

        string mode = _netMgr.IsHost ? "Host" : _netMgr.IsServer ? "Server" : "Client";
        statusText.text = $"Transport: {transportTypeName} [{serverPort}]\nMode: {mode}";
    }

    private void UpdateClientIdText()
    {
        if (clientIdText == null || _netMgr == null)
            return;

        if (_netMgr.IsClient)
        {
            clientIdText.text = $"ClientId = {_netMgr.LocalClientId}";
        }
        else
        {
            clientIdText.text = "";
        }
    }

    private string GetNetworkMode()
    {
        if (_netMgr.IsServer)
        {
            if (_netMgr.IsHost)
            {
                return "host";
            }
            return "server";
        }
        return "client";
    }

     public void Log(string msg)
    {
        Debug.Log($"[{GetNetworkMode()} {_netMgr.LocalClientId}]: {msg}");
    }

    public void Log(NetworkBehaviour what, string msg)
    {
        ulong ownerId = what.OwnerClientId;
        Debug.Log($"[{GetNetworkMode()} {_netMgr.LocalClientId}][{what.GetType().Name} {ownerId}]: {msg}");
    }
}