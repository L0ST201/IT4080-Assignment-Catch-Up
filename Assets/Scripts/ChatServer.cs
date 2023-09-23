using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ChatServer : NetworkBehaviour
{
    public ChatUi chatUi;
    const ulong SYSTEM_ID = ulong.MaxValue;
    private ulong[] dmClientIds = new ulong[2];

    void Start()
    {
        chatUi.printEnteredText = false;
        chatUi.MessageEntered += OnChatUiMessageEntered;

        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback += ServerOnClientConnected;
            NetworkManager.OnClientDisconnectCallback += ServerOnClientDisconnect; 
            if (IsHost)
            {
                DisplayMessageLocally(SYSTEM_ID, $"You are the host AND client {NetworkManager.LocalClientId}");
            }
            else
            {
                DisplayMessageLocally(SYSTEM_ID, "You are the server");
            }
        }
        else
        {
            DisplayMessageLocally(SYSTEM_ID, $"You are the client {NetworkManager.LocalClientId}");
        }
    }

    private void ServerOnClientConnected(ulong clientId)
    {
        if (IsHost)
        {
            SendWelcomeMessageToClientRpc($"Welcome, I see you Player({clientId}) have connected to the server, well done!", clientId, NetworkManager.LocalClientId);
        }
        SendGlobalMessage($"Player {clientId} has connected to the server.");
    }

    private void ServerOnClientDisconnect(ulong clientId)
    {

        SendGlobalMessage($"Player {clientId} has disconnected from the server.");
    }

    private void DisplayMessageLocally(ulong from, string message)
    {
        string fromStr = $"Player {from}";
        Color textColor = chatUi.defaultTextColor;
        if (from == NetworkManager.LocalClientId)
        {
            fromStr = "you";
            textColor = Color.magenta;
        }
        else if (from == SYSTEM_ID)
        {
            fromStr = "SYS";
            textColor = Color.green;
        }
        chatUi.addEntry(fromStr, message, textColor);
    }

    private void OnChatUiMessageEntered(string message)
    {
        SendChatMessageServerRpc(message);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendChatMessageServerRpc(string message, ServerRpcParams serverRpcParams = default)
    {
        if (message.StartsWith("@"))
        {
            string[] parts = message.Split(" ");
            string clientIdStr = parts[0].Replace("@", "");
            if (ulong.TryParse(clientIdStr, out ulong toClientId))
            {
                if (NetworkManager.Singleton.ConnectedClients.ContainsKey(toClientId))
                {
                    string whisperMessage = string.Join(" ", parts, 1, parts.Length - 1); 
                    ServerSendDirectMessage(whisperMessage, serverRpcParams.Receive.SenderClientId, toClientId);
                }
                else
                {
                SendChatNotificationServerRpc($"The message could not be sent. Player {toClientId} is not connected.", serverRpcParams.Receive.SenderClientId);
                }
            }
            else
            {
                SendChatNotificationServerRpc($"Invalid client ID: {clientIdStr}", serverRpcParams.Receive.SenderClientId);
            }
        }
        else
        {
            ReceiveChatMessageClientRpc(message, serverRpcParams.Receive.SenderClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendChatNotificationServerRpc(string message, ulong targetClientId, ServerRpcParams serverRpcParams = default)
    {
        ReceiveChatMessageClientRpc(message, SYSTEM_ID, new ClientRpcParams { Send = { TargetClientIds = new ulong[] { targetClientId } } });
    }

    [ClientRpc]
    public void ReceiveChatMessageClientRpc(string message, ulong from, ClientRpcParams clientRpcParams = default)
    {
        DisplayMessageLocally(from, message);
    }

    [ClientRpc]
    public void SendWelcomeMessageToClientRpc(string message, ulong targetClientId, ulong fromClientId, ClientRpcParams clientRpcParams = default)
    {
        if (NetworkManager.LocalClientId == targetClientId)
        {
            DisplayMessageLocally(fromClientId, message);
        }
    }
    private void ServerSendDirectMessage(string message, ulong from, ulong to)
    {
        dmClientIds[0] = from;
        dmClientIds[1] = to;
        ClientRpcParams rpcParams = default;
        rpcParams.Send.TargetClientIds = dmClientIds;

        ReceiveChatMessageClientRpc($"<whisper> {message}", from, rpcParams);
    }

    private void SendGlobalMessage(string message)
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClients)
        {
            ReceiveChatMessageClientRpc(message, SYSTEM_ID, new ClientRpcParams { Send = { TargetClientIds = new ulong[] { client.Value.ClientId } } });
        }
    }
}