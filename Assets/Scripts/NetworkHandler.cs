using System;
using System.Collections;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class NetworkHandler : NetworkBehaviour
{
    [SerializeField]
    private TextMeshProUGUI statusText;

    private enum NetworkStatus
    {
        ManualTest,
        Host,
        Server,
        Client,
        NothingYet,
        Disconnected
    }

    private NetworkStatus currentStatus = NetworkStatus.ManualTest;

    void Start()
    {
        NetworkManager.Singleton.OnClientStarted += OnClientStarted;
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
    }

    private void Update()
    {
        UpdateStatusText();
    }

    private void UpdateStatusText()
    {
        if (statusText == null) return;

        string status;
        switch (currentStatus)
        {
            case NetworkStatus.Host:
                status = "I am the Host!";
                break;
            case NetworkStatus.Server:
                status = "I am the Server!";
                break;
            case NetworkStatus.Client:
                status = "I am a Client!";
                break;
            case NetworkStatus.Disconnected:
                status = "Disconnected";
                break;
            case NetworkStatus.NothingYet:
            default:
                status = "... Nothing Yet!";
                break;
        }

        statusText.text = "Status: " + status;
    }

    private void ResetStatus()
    {
        currentStatus = NetworkStatus.NothingYet;
    }

    private void OnClientStarted()
    {
        if (!IsHost)
        {
            currentStatus = NetworkStatus.Client;
        }
        SubscribeClientEvents();
    }

    private void OnServerStarted()
    {
        currentStatus = IsHost ? NetworkStatus.Host : NetworkStatus.Server;
        SubscribeServerEvents();
    }

    private void SubscribeClientEvents()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += ClientOnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += ClientOnClientDisconnected;
        NetworkManager.Singleton.OnClientStopped += ClientOnClientStopped;
    }

    private void UnsubscribeClientEvents()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= ClientOnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= ClientOnClientDisconnected;
        NetworkManager.Singleton.OnClientStopped -= ClientOnClientStopped;
    }

    private void SubscribeServerEvents()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += ServerOnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += ServerOnClientDisconnected;
        NetworkManager.Singleton.OnServerStopped += ServerOnServerStopped;
    }

    private void UnsubscribeServerEvents()
    {
        NetworkManager.Singleton.OnClientConnectedCallback -= ServerOnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= ServerOnClientDisconnected;
        NetworkManager.Singleton.OnServerStopped -= ServerOnServerStopped;
    }

    private void ClientOnClientConnected(ulong clientId)
    {
        // Placeholder for future logic
    }

    private void ClientOnClientDisconnected(ulong clientId)
    {
        if (IsClient && !IsHost)
        {
            currentStatus = NetworkStatus.Disconnected;
        }
    }

    private void ClientOnClientStopped(bool indicator)
    {
        if (IsClient)
        {
            currentStatus = NetworkStatus.Disconnected;
        }
        UnsubscribeClientEvents();
    }

    private void ServerOnClientConnected(ulong clientId)
    {
        // Placeholder for future logic
    }

    private void ServerOnClientDisconnected(ulong clientId)
    {
        // Placeholder for future logic
    }

    private void ServerOnServerStopped(bool indicator)
    {
        UnsubscribeServerEvents();
    }

    public void ShutdownServer()
    {
        currentStatus = NetworkStatus.Disconnected;
        NetworkManager.Singleton.Shutdown();
    }
}
