using System;
using System.Collections;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class NetworkHandler : NetworkBehaviour
{
    public NetworkHelper NetworkHelper;

    [SerializeField]
    private TextMeshProUGUI statusText;

    private NetworkManager _netMgr;

    void Start()
    {
        _netMgr = NetworkManager.Singleton;
        NetworkManager.Singleton.OnClientStarted += OnClientStarted;
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
    }

    private void Update()
    {
        if (statusText == null) return;

        if (NetworkHelper == null)
        {
            Debug.LogError("NetworkHelper reference is not set in NetworkHandler.");
            return;
        }

        // Update the statusText UI component with latest status
        statusText.text = GetCurrentStatus();
    }

    private string GetCurrentStatus()
    {
        if (IsHost)
        {
            return "I am the Host!";
        }
        else if (_netMgr.IsServer)
        {
            return "I am the Server!";
        }
        else if (_netMgr.IsClient)
        {
            return "I am a Client!";
        }
        return "... Nothing Yet!";
    }

    private void OnClientStarted()
    {
        NetworkHelper.Log("!! Client Started !!");
        NetworkHelper.Log("I AM a Server! " + _netMgr.LocalClientId);
        NetworkHelper.Log("I AM a Host! " + _netMgr.LocalClientId + "/0");
        NetworkHelper.Log("I AM a Client! " + _netMgr.LocalClientId);
        SubscribeClientEvents();
    }

    private void OnServerStarted()
    {
        NetworkHelper.Log("!! Server Started !!");
        NetworkHelper.Log($"I AM a Server! {_netMgr.LocalClientId}");
        NetworkHelper.Log($"I AM a Host! {_netMgr.LocalClientId}/0");
        NetworkHelper.Log($"I AM a Client! {_netMgr.LocalClientId}");
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
        NetworkHelper.Log($"Client {clientId} connected to the server");
        if (clientId == _netMgr.LocalClientId)
        {
            NetworkHelper.Log($"I have connected {clientId}");
        }
    }

    private void ClientOnClientDisconnected(ulong clientId)
    {
        NetworkHelper.Log($"Client {clientId} disconnected from the server");
    }

    private void ClientOnClientStopped(bool indicator)
    {
        NetworkHelper.Log("!! Client Stopped !!");
        NetworkHelper.Log($"I AM a Server! {_netMgr.LocalClientId}");
        NetworkHelper.Log($"I AM a Host! {_netMgr.LocalClientId}/0");
        NetworkHelper.Log($"I AM a Client! {_netMgr.LocalClientId}");
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
        NetworkHelper.Log("!! Server Stopped !!");
        NetworkHelper.Log($"I AM a Server! {_netMgr.LocalClientId}");
        NetworkHelper.Log($"I AM a Host! {_netMgr.LocalClientId}/0");
        NetworkHelper.Log($"I AM a Client! {_netMgr.LocalClientId}");
        UnsubscribeServerEvents();
    }

    public void ShutdownServer()
    {
        NetworkManager.Singleton.Shutdown();
    }
}
