using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class LeverStation : NetworkBehaviour
{
    public static LeverStation Instance { get; private set; }

    [Header("Lever Visuals")]
    public Transform greenLeverVisual;
    public Transform blueLeverVisual;
    public Transform orangeLeverVisual;
    public Transform purpleLeverVisual;
    public float leverUpAngle = 0f;
    public float leverDownAngle = -60f;

    [Header("Key Prefab And Spawn Points")]
    public GameObject keyPrefab;
    public Transform greenKeySpawnPoint;
    public Transform blueKeySpawnPoint;
    public Transform orangeKeySpawnPoint;
    public Transform purpleKeySpawnPoint;

    [Header("Level Facts")]
    public bool blueDeadEndPresent;
    public bool greenDoorBlocksPath;
    public bool orangeCageElevated;
    public bool purpleTwoCubesVisible;

    private NetworkVariable<bool> greenUp = new NetworkVariable<bool>(true);
    private NetworkVariable<bool> blueUp = new NetworkVariable<bool>(true);
    private NetworkVariable<bool> orangeUp = new NetworkVariable<bool>(true);
    private NetworkVariable<bool> purpleUp = new NetworkVariable<bool>(true);

    private readonly Dictionary<DoorColor, NetworkObject> spawnedKeys = new Dictionary<DoorColor, NetworkObject>();

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        greenUp.OnValueChanged += (_, v) => UpdateLeverVisual(greenLeverVisual, v);
        blueUp.OnValueChanged += (_, v) => UpdateLeverVisual(blueLeverVisual, v);
        orangeUp.OnValueChanged += (_, v) => UpdateLeverVisual(orangeLeverVisual, v);
        purpleUp.OnValueChanged += (_, v) => UpdateLeverVisual(purpleLeverVisual, v);

        UpdateLeverVisual(greenLeverVisual, greenUp.Value);
        UpdateLeverVisual(blueLeverVisual, blueUp.Value);
        UpdateLeverVisual(orangeLeverVisual, orangeUp.Value);
        UpdateLeverVisual(purpleLeverVisual, purpleUp.Value);
    }

    private void UpdateLeverVisual(Transform lever, bool isUp)
    {
        if (!lever) return;
        float angle = isUp ? leverUpAngle : leverDownAngle;
        lever.localRotation = Quaternion.Euler(angle, 0f, 0f);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ToggleLeverServerRpc(DoorColor color)
    {
        switch (color)
        {
            case DoorColor.Green: greenUp.Value = !greenUp.Value; break;
            case DoorColor.Blue: blueUp.Value = !blueUp.Value; break;
            case DoorColor.Orange: orangeUp.Value = !orangeUp.Value; break;
            default: purpleUp.Value = !purpleUp.Value; break;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ResetServerRpc()
    {
        greenUp.Value = true;
        blueUp.Value = true;
        orangeUp.Value = true;
        purpleUp.Value = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void EngageServerRpc()
    {
        TryRevealKey(DoorColor.Green);
        TryRevealKey(DoorColor.Blue);
        TryRevealKey(DoorColor.Orange);
        TryRevealKey(DoorColor.Purple);
    }

    private void TryRevealKey(DoorColor color)
    {
        if (spawnedKeys.ContainsKey(color)) return;

        (bool green, bool blue, bool orange, bool purple)? target = GetTargetPattern(color);
        if (target == null) return;

        if (greenUp.Value == target.Value.green && blueUp.Value == target.Value.blue &&
            orangeUp.Value == target.Value.orange && purpleUp.Value == target.Value.purple)
        {
            SpawnKey(color);
        }
    }

    private (bool green, bool blue, bool orange, bool purple)? GetTargetPattern(DoorColor color)
    {
        if (GameSessionManager.Instance.currentMonsterType != MonsterType.BloodDemon) return null;

        switch (color)
        {
            case DoorColor.Blue:
                return (true, false, !blueDeadEndPresent, false);
            case DoorColor.Green:
                return (false, !greenDoorBlocksPath, true, false);
            case DoorColor.Orange:
                return (!orangeCageElevated, false, false, true);
            default:
                return (!purpleTwoCubesVisible, true, false, true);
        }
    }

    private void SpawnKey(DoorColor color)
    {
        Transform point = GetSpawnPoint(color);
        if (!point || !keyPrefab) return;

        GameObject obj = Instantiate(keyPrefab, point.position, point.rotation);
        KeyPickup pickup = obj.GetComponent<KeyPickup>();
        if (pickup) pickup.color = color;

        NetworkObject netObj = obj.GetComponent<NetworkObject>();
        if (netObj)
        {
            netObj.Spawn();
            spawnedKeys[color] = netObj;
        }
    }

    private Transform GetSpawnPoint(DoorColor color)
    {
        switch (color)
        {
            case DoorColor.Green: return greenKeySpawnPoint;
            case DoorColor.Blue: return blueKeySpawnPoint;
            case DoorColor.Orange: return orangeKeySpawnPoint;
            default: return purpleKeySpawnPoint;
        }
    }

    public void ResetPuzzle()
    {
        if (!IsServer) return;

        foreach (var kvp in spawnedKeys)
        {
            if (kvp.Value != null && kvp.Value.IsSpawned) kvp.Value.Despawn();
        }
        spawnedKeys.Clear();

        greenUp.Value = true;
        blueUp.Value = true;
        orangeUp.Value = true;
        purpleUp.Value = true;
    }
}
