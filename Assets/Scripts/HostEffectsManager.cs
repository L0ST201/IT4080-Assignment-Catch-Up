using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class HostEffectsManager : NetworkBehaviour
{
    [SerializeField]
    private ParticleSystem hostParticleEffectPrefab;

    private ParticleSystem instantiatedEffect;

    private void Start()
    {
        // If this is the host
        if (IsHost())
        {
            // Instantiate the aura as a networked GameObject
            var effectInstance = Instantiate(hostParticleEffectPrefab, transform.position, Quaternion.identity);

            // Get the NetworkObject component
            var networkObject = effectInstance.GetComponent<NetworkObject>();
            if (networkObject)
            {
                networkObject.Spawn(); // Spawn it first
            }

            effectInstance.transform.SetParent(transform, true); // Then set the parent
            instantiatedEffect = effectInstance.GetComponent<ParticleSystem>();
            instantiatedEffect.Play();
        }
    }

    private bool IsHost()
    {
        return NetworkManager.Singleton != null && NetworkManager.Singleton.LocalClientId == NetworkManager.ServerClientId;
    }
}
