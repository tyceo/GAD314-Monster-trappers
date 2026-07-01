using System;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayButtons : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private int maxConnections = 4;
    [SerializeField] private string connectionType = "dtls";

    [Header("UI")]
    [Tooltip("Input field where the client types the join code")]
    [SerializeField] private TMP_InputField joinCodeInputField;

    [Tooltip("Text that displays the join code after the host creates a relay")]
    [SerializeField] private TMP_Text joinCodeDisplayText;
    
    
    [SerializeField] private GameObject[] uiToHideOnConnect;
    
    [Header("Player")]
    [SerializeField] private NetworkObject playerPrefab;


    /// <summary>Call this from the Host button's OnClick event.</summary>
    public void OnStartHostButtonClicked()
    {
        _ = StartHostWithRelay(maxConnections, connectionType);
    }

    /// <summary>Call this from the Join button's OnClick event.</summary>
    public void OnJoinClientButtonClicked()
    {
        string code = joinCodeInputField != null ? joinCodeInputField.text.Trim() : string.Empty;

        if (string.IsNullOrEmpty(code))
        {
            Debug.LogWarning("[RelayButtons] Join code is empty!");
            return;
        }

        _ = StartClientWithRelay(code, connectionType);
    }
    
    public async System.Threading.Tasks.Task<string> StartHostWithRelay(int maxConnections, string connectionType)
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
        NetworkManager.Singleton.GetComponent<UnityTransport>()
            .SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, connectionType));

        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        bool started = NetworkManager.Singleton.StartHost();
        if (started)
        {
            Debug.Log($"[RelayButtons] Host started. Join code: {joinCode}");
            if (joinCodeDisplayText != null)
                joinCodeDisplayText.text = $"Join Code: {joinCode}";
            HideUI();
            SpawnPlayer(NetworkManager.Singleton.LocalClientId);
            return joinCode;
        }

        Debug.LogError("[RelayButtons] Failed to start host.");
        return null;
    }

    public async System.Threading.Tasks.Task<bool> StartClientWithRelay(string joinCode, string connectionType)
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode: joinCode);
        NetworkManager.Singleton.GetComponent<UnityTransport>()
            .SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, connectionType));

        bool started = !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();
        Debug.Log(started ? "[RelayButtons] Client joined successfully." : "[RelayButtons] Failed to join as client.");
        if (started)
        {
            HideUI();
            // Wait until connected then request spawn from host
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
        return started;
    }
    private void OnClientConnected(ulong clientId)
    {
        if (clientId != NetworkManager.Singleton.LocalClientId) return;
        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        PlayerSpawner.Instance.RequestSpawnServerRpc(clientId);
    }

    private void SpawnPlayer(ulong clientId)
    {
        NetworkObject player = Instantiate(playerPrefab);
        player.SpawnAsPlayerObject(clientId, true);
    }

    private void HideUI()
    {
        foreach (GameObject ui in uiToHideOnConnect)
        {
            if (ui != null)
                ui.SetActive(false);
        }
    }
    
    public void OnJoinClientNoPlayerSpawnButtonClicked()
    {
        string code = joinCodeInputField != null ? joinCodeInputField.text.Trim() : string.Empty;

        if (string.IsNullOrEmpty(code))
        {
            Debug.LogWarning("[RelayButtons] Join code is empty!");
            return;
        }

        _ = StartClientWithRelayNoSpawn(code, connectionType);
    }
    
    public async System.Threading.Tasks.Task<bool> StartClientWithRelayNoSpawn(string joinCode, string connectionType)
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode: joinCode);
        NetworkManager.Singleton.GetComponent<UnityTransport>()
            .SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, connectionType));

        bool started = !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();
        Debug.Log(started ? "[RelayButtons] Client joined successfully (no player spawn)." : "[RelayButtons] Failed to join as client.");
        if (started) HideUI();
        return started;
    }
}