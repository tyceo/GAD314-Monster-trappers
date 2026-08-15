using Unity.Netcode;
using UnityEngine;
using System.Collections;

public enum DoorColor { Green, Blue, Orange, Purple }
public enum SessionState { Connecting, Playing, Won, Lost }
public enum MonsterType { BloodDemon, AstralLizard, SludgeDragon, IceWizard }

public class GameSessionManager : NetworkBehaviour
{
    public static GameSessionManager Instance { get; private set; }

    [Header("UI Roots")]
    public GameObject relayUI;
    public GameObject controllerButtonUI;
    public WinLoseUI winLoseUIController;

    [Header("Doors")]
    public Door greenDoor;
    public Door blueDoor;
    public Door orangeDoor;
    public Door purpleDoor;

    [Header("Controller Buttons")]
    public ControllerButton greenButton;
    public ControllerButton blueButton;
    public ControllerButton orangeButton;
    public ControllerButton purpleButton;

    [Header("Lever Puzzle")]
    public LeverStation leverStation;

    [Header("Current Monster")]
    public MonsterType currentMonsterType = MonsterType.BloodDemon;

    private bool IsLocalController =>
        NetworkManager.Singleton.LocalClient != null &&
        NetworkManager.Singleton.LocalClient.PlayerObject == null;

    [Header("Refs")]
    public MonsterController monster;

    private NetworkVariable<bool> explorerConnected = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> controllerConnected = new NetworkVariable<bool>(false);

    private NetworkVariable<bool> greenActivated = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> blueActivated = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> orangeActivated = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> purpleActivated = new NetworkVariable<bool>(false);

    private NetworkVariable<bool> greenOpen = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> blueOpen = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> orangeOpen = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> purpleOpen = new NetworkVariable<bool>(false);

    private NetworkVariable<bool> monsterInCage = new NetworkVariable<bool>(false);
    private NetworkVariable<SessionState> sessionState = new NetworkVariable<SessionState>(SessionState.Connecting);

    private bool monsterDeathAnimPlaying; // blocks the win check until PlayTrappedAnimation's callback fires

    public bool IsPlaying => sessionState.Value == SessionState.Playing;

    private void Awake()
    {
        Instance = this;
        RenderSettings.fog = false;
    }

    public void StopGameButton()
    {
        Application.Quit();
    }

    public override void OnNetworkSpawn()
    {
        explorerConnected.OnValueChanged += (_, _) => RefreshConnectionUI();
        controllerConnected.OnValueChanged += (_, _) => RefreshConnectionUI();
        sessionState.OnValueChanged += (_, _) => RefreshSessionUI();

        greenActivated.OnValueChanged += (_, v) => { if (IsLocalController) greenButton.SetActivated(v); };
        blueActivated.OnValueChanged += (_, v) => { if (IsLocalController) blueButton.SetActivated(v); };
        orangeActivated.OnValueChanged += (_, v) => { if (IsLocalController) orangeButton.SetActivated(v); };
        purpleActivated.OnValueChanged += (_, v) => { if (IsLocalController) purpleButton.SetActivated(v); };

        greenOpen.OnValueChanged += (_, v) => greenDoor.SetOpen(v);
        blueOpen.OnValueChanged += (_, v) => blueDoor.SetOpen(v);
        orangeOpen.OnValueChanged += (_, v) => orangeDoor.SetOpen(v);
        purpleOpen.OnValueChanged += (_, v) => purpleDoor.SetOpen(v);

        RefreshConnectionUI();
        RefreshSessionUI();

        if (IsLocalController)
        {
            greenButton.SetActivated(greenActivated.Value);
            blueButton.SetActivated(blueActivated.Value);
            orangeButton.SetActivated(orangeActivated.Value);
            purpleButton.SetActivated(purpleActivated.Value);
        }

        greenDoor.SetOpen(greenOpen.Value);
        blueDoor.SetOpen(blueOpen.Value);
        orangeDoor.SetOpen(orangeOpen.Value);
        purpleDoor.SetOpen(purpleOpen.Value);

        if (IsServer)
        {
            sessionState.Value = SessionState.Playing;
            Debug.Log("[GameSessionManager] Host started - game is now playing!");
        }
    }

    private void RefreshConnectionUI()
    {
        bool bothConnected = explorerConnected.Value && controllerConnected.Value;
        if (relayUI) relayUI.SetActive(!bothConnected);

        bool isLocalController = NetworkManager.Singleton.LocalClient != null &&
                                  NetworkManager.Singleton.LocalClient.PlayerObject == null;
        if (controllerButtonUI) controllerButtonUI.SetActive(bothConnected && isLocalController);
    }

    private void RefreshSessionUI()
    {
        if (!winLoseUIController) return;
        switch (sessionState.Value)
        {
            case SessionState.Won:
                winLoseUIController.ShowWin();
                if (IsServer) UnlockCursorClientRpc();
                break;
            case SessionState.Lost:
                winLoseUIController.ShowLose();
                if (IsServer) UnlockCursorClientRpc();
                break;
            default: winLoseUIController.HideAll(); break;
        }
    }

    [ClientRpc]
    private void UnlockCursorClientRpc()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void RegisterExplorerServerRpc() { explorerConnected.Value = true; }

    [ServerRpc(RequireOwnership = false)]
    public void RegisterControllerServerRpc() { controllerConnected.Value = true; }

    public void CollectKey(DoorColor color)
    {
        if (!IsServer) return;
        SetActivated(color, true);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetDoorHeldServerRpc(DoorColor color, bool held)
    {
        if (!GetActivated(color)) return;
        SetOpen(color, held);
        CheckWinCondition();
    }

    public void SetMonsterInCage(bool inCage)
    {
        if (!IsServer) return;
        monsterInCage.Value = inCage;

        if (!inCage)
        {
            monsterDeathAnimPlaying = false;
            CheckWinCondition();
            return;
        }

        if (monsterDeathAnimPlaying) return;
        monsterDeathAnimPlaying = true;

        monster.PlayTrappedAnimation(() =>
        {
            monsterDeathAnimPlaying = false;
            CheckWinCondition();
        });
    }

    public void NotifyPlayerCaught()
    {
        if (!IsServer) return;
        if (sessionState.Value != SessionState.Playing) return;
        sessionState.Value = SessionState.Lost;
    }

    private void CheckWinCondition()
    {
        if (!IsServer) return;
        if (sessionState.Value != SessionState.Playing) return;
        if (monsterDeathAnimPlaying) return;

        bool allClosed = !greenOpen.Value && !blueOpen.Value && !orangeOpen.Value && !purpleOpen.Value;
        if (monsterInCage.Value && allClosed)
            sessionState.Value = SessionState.Won;
    }

    private bool GetActivated(DoorColor color)
    {
        switch (color)
        {
            case DoorColor.Green: return greenActivated.Value;
            case DoorColor.Blue: return blueActivated.Value;
            case DoorColor.Orange: return orangeActivated.Value;
            default: return purpleActivated.Value;
        }
    }

    private void SetActivated(DoorColor color, bool value)
    {
        switch (color)
        {
            case DoorColor.Green: greenActivated.Value = value; break;
            case DoorColor.Blue: blueActivated.Value = value; break;
            case DoorColor.Orange: orangeActivated.Value = value; break;
            default: purpleActivated.Value = value; break;
        }
    }

    private void SetOpen(DoorColor color, bool value)
    {
        switch (color)
        {
            case DoorColor.Green: greenOpen.Value = value; break;
            case DoorColor.Blue: blueOpen.Value = value; break;
            case DoorColor.Orange: orangeOpen.Value = value; break;
            default: purpleOpen.Value = value; break;
        }
    }

    public void RestartRelay() { RestartRelayServerRpc(); }

    [ServerRpc(RequireOwnership = false)]
    private void RestartRelayServerRpc()
    {
        ShutdownAndReloadClientRpc();
        Invoke(nameof(ShutdownHost), 0.2f);
    }

    [ClientRpc]
    private void ShutdownAndReloadClientRpc()
    {
        if (IsHost) return;
        NetworkManager.Singleton.Shutdown();
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }


    private void ShutdownHost()
    {
        NetworkManager.Singleton.Shutdown();
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void RespawnLevel() { RespawnLevelServerRpc(); }

    [ServerRpc(RequireOwnership = false)]
    private void RespawnLevelServerRpc()
    {
        greenActivated.Value = false;
        blueActivated.Value = false;
        orangeActivated.Value = false;
        purpleActivated.Value = false;

        greenOpen.Value = false;
        blueOpen.Value = false;
        orangeOpen.Value = false;
        purpleOpen.Value = false;

        if (leverStation) leverStation.ResetPuzzle();

        monsterDeathAnimPlaying = false;

        monster.ResetToSpawn();
        RespawnPlayersClientRpc();

        sessionState.Value = SessionState.Playing;
    }

    [ClientRpc]
    private void RespawnPlayersClientRpc()
    {
        if (NetworkManager.Singleton.LocalClient == null) return;
        NetworkObject playerObj = NetworkManager.Singleton.LocalClient.PlayerObject;
        if (playerObj == null) return;

        PlayerController pc = playerObj.GetComponent<PlayerController>();
        if (pc) pc.ServerRequestedRespawn();
    }
}