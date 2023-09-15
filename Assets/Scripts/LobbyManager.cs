/* using Unity.Netcode;
using UnityEngine;

public class LobbyManager : NetworkBehaviour
{
    public NetworkObject startButton;
    public NetworkObject hostButton; 
    public NetworkObject joinButton;
    public NetworkObject quitButton;

    public string ipAddress = "127.0.0.1";

    public override void OnNetworkSpawn()
    {
        if(IsServer)
        {
            startButton.gameObject.SetActive(true);
        }
        else 
        {
            startButton.gameObject.SetActive(false);
        }

        if(IsClient)
        {
            hostButton.gameObject.SetActive(false);
            joinButton.gameObject.SetActive(false);
            quitButton.gameObject.SetActive(true);
        }
        else
        {
            hostButton.gameObject.SetActive(true);
            joinButton.gameObject.SetActive(true);
            quitButton.gameObject.SetActive(false);
        }
    }

    public void HostGame()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.StartHost();

        startButton.gameObject.SetActive(true);
        hostButton.gameObject.SetActive(false);
        joinButton.gameObject.SetActive(false);
        quitButton.gameObject.SetActive(true);
    }

    public void JoinGame()
    {
        NetworkManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(ipAddress);
        NetworkManager.Singleton.StartClient();

        startButton.gameObject.SetActive(false);
        hostButton.gameObject.SetActive(false);
        joinButton.gameObject.SetActive(false);
        quitButton.gameObject.SetActive(true);
    }

    public void QuitGame()
    {
        if(IsServer)
        {
            NetworkManager.Singleton.StopHost();
        }
        else if(IsClient)
        {
            NetworkManager.Singleton.StopClient();
        }

        startButton.gameObject.SetActive(false);
        hostButton.gameObject.SetActive(true);
        joinButton.gameObject.SetActive(true);
        quitButton.gameObject.SetActive(false);
    }

    public void StartGame()
    {
        if(IsServer)
        {
            NetworkSceneManager.SwitchScene("GameScene");
        }
    }

    public void ApprovalCheck(byte[] connectionData, ulong clientId, NetworkManager.ConnectionApprovedDelegate callback)
    {
        string ipAddress = System.Text.Encoding.ASCII.GetString(connectionData);
        callback(true, null, false, null, null);
    }
}
 */