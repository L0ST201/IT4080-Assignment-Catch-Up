using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class HostEffectsManager : NetworkBehaviour
{
    [SerializeField]
    private ParticleSystem hostParticleEffectPrefab;

    private void Start()
    {
        if (IsOwner && NetworkManager.Singleton && hostParticleEffectPrefab)
        {
            if (NetworkManager.Singleton.LocalClientId == NetworkManager.ServerClientId)
            {
                var effectInstance = NetworkObject.Instantiate(hostParticleEffectPrefab);
                effectInstance.transform.SetParent(transform);
                effectInstance.transform.position = transform.position;
                var particleSystem = effectInstance.GetComponent<ParticleSystem>();
                if (particleSystem)
                {
                    particleSystem.Play();
                }
            }
        }
    }
}
