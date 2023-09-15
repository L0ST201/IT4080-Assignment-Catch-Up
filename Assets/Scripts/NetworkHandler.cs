using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

    private void Start()
    {
        if (statusText != null)
        {
            statusText.text = "Status: Manual Test";
        }

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

    public void ResetStatus()
    {
        currentStatus = NetworkStatus.NothingYet;
    }

    public void ShutdownServer()
    {
        currentStatus = NetworkStatus.Disconnected;
        NetworkManager.Singleton.Shutdown();
    }

    private void OnClientShutdown(bool indicator)
    {
        if (IsClient && !IsHost)
        {
            currentStatus = NetworkStatus.Disconnected;
        }
        UnsubscribeClientEvents();
    }

    private void OnHostEndedGame()
    {
        if (IsServer && !IsHost)
        {
            currentStatus = NetworkStatus.Disconnected;
        }
    }

    private void OnClientStarted()
    {
        if (!IsHost)
        {
            currentStatus = NetworkStatus.Client;
        }
        SubscribeClientEvents();
    }

    private void SubscribeClientEvents()
    {
        NetworkManager.OnClientConnectedCallback += ClientOnClientConnected;
        NetworkManager.OnClientDisconnectCallback += ClientOnClientDisconnected;
        NetworkManager.OnServerStopped += ClientOnClientStopped;
    }

    private void UnsubscribeClientEvents()
    {
        NetworkManager.OnClientConnectedCallback -= ClientOnClientConnected;
        NetworkManager.OnClientDisconnectCallback -= ClientOnClientDisconnected;
        NetworkManager.OnServerStopped -= ClientOnClientStopped;
    }

    private void ClientOnClientConnected(ulong clientId)
    {
        // Placeholder for furue logic
    }

    private void ClientOnClientDisconnected(ulong clientId)
    {
        if (IsClient && !IsHost) // Ensure you're not a host
        {
            currentStatus = NetworkStatus.Disconnected;
        }
        // Other logic if any
    }

    private void OnServerStarted()
    {
        if (IsHost)
        {
            currentStatus = NetworkStatus.Host;
        }
        else
        {
            currentStatus = NetworkStatus.Server;
        }
        SubscribeServerEvents();
    }

    private void SubscribeServerEvents()
    {
        NetworkManager.OnClientConnectedCallback += ServerOnClientConnected;
        NetworkManager.OnClientDisconnectCallback += ServerOnClientDisconnected;
        NetworkManager.OnServerStopped += ServerOnServerStopped;
    }

    private void UnsubscribeServerEvents()
    {
        NetworkManager.OnClientConnectedCallback -= ServerOnClientConnected;
        NetworkManager.OnClientDisconnectCallback -= ServerOnClientDisconnected;
        NetworkManager.OnServerStopped -= ServerOnServerStopped;
    }

    private void ServerOnClientConnected(ulong clientId)
    {
        // Placeholder for furue logic
    }

    private void ServerOnClientDisconnected(ulong clientId)
    {
        // Placeholder for furue logic
    }

    private void ServerOnServerStopped(bool indicator)
    {
        UnsubscribeServerEvents();
    }

    private void OnHostStarted()
    {
        currentStatus = NetworkStatus.Host;
        SubscribeHostEvents();
    }

    private void SubscribeHostEvents()
    {
        NetworkManager.OnClientConnectedCallback += HostOnClientConnected;
        NetworkManager.OnClientDisconnectCallback += HostOnClientDisconnected;
        NetworkManager.OnServerStopped += HostOnHostStopped;
    }

    private void UnsubscribeHostEvents()
    {
        NetworkManager.OnClientConnectedCallback -= HostOnClientConnected;
        NetworkManager.OnClientDisconnectCallback -= HostOnClientDisconnected;
        NetworkManager.OnServerStopped -= HostOnHostStopped;
    }

    private void HostOnClientConnected(ulong clientId)
    {
        // Placeholder for furue logic
    }

    private void HostOnClientDisconnected(ulong clientId)
    {
        // Placeholder for furue logic
    }

    private void HostOnHostStopped(bool indicator)
    {
        UnsubscribeHostEvents();
    }

    private void ClientOnClientStopped(bool indicator)
    {
        Debug.Log("Client stopped method triggered");
        if (IsClient)
        {
            currentStatus = NetworkStatus.Disconnected;
        }
        UnsubscribeClientEvents();
    }

}
