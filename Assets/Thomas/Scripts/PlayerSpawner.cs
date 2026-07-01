using Unity.Netcode;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    public static PlayerSpawner Instance { get; private set; }

    [SerializeField] private NetworkObject playerPrefab;

    private void Awake()
    {
        Instance = this;
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestSpawnServerRpc(ulong clientId)
    {
        NetworkObject player = Instantiate(playerPrefab);
        player.SpawnAsPlayerObject(clientId, true);
    }
}