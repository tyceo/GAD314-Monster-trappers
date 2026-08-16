using Unity.Netcode;
using UnityEngine;
using System;

public class KeyPickup : NetworkBehaviour
{
    public DoorColor color;

    public static event Action<DoorColor, Vector3> OnKeySpawned;

    public override void OnNetworkSpawn()
    {
        OnKeySpawned?.Invoke(color, transform.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        if (!other.CompareTag("Player")) return;

        // GameSessionManager.Instance.CollectKey(color);

        NetworkObject netObj = GetComponent<NetworkObject>();
        if (netObj) netObj.Despawn();
    }
}
