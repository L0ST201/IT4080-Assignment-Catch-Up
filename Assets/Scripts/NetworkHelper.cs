using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.Serialization.Formatters;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkHelper : MonoBehaviour
{
    public Button hostButton;
    public Button clientButton;
    public Button serverButton;
    public Button shutdownButton;

    public NetworkHandler networkHandler;

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
            });
        }
    }

    private void Update()
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
}
