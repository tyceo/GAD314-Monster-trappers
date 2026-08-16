using Unity.Netcode;
using UnityEngine;

public class Switchboard : NetworkBehaviour
{
    [Header("Switchboard Identity")]
    [SerializeField] private DoorColor boardColor;

    [Header("Switch GameObjects")]
    [SerializeField] private GameObject switchOff1;
    [SerializeField] private GameObject switchOff2;
    [SerializeField] private GameObject switchOff3;
    [SerializeField] private GameObject switchOff4;
    [SerializeField] private GameObject switchOn1;
    [SerializeField] private GameObject switchOn2;
    [SerializeField] private GameObject switchOn3;
    [SerializeField] private GameObject switchOn4;

    // Network variables to sync switch states - Server writes, everyone reads
    private NetworkVariable<bool> switch1On = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> switch2On = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> switch3On = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> switch4On = new NetworkVariable<bool>(false);

    private void Start()
    {
        // Auto-find children if not assigned
        if (!switchOff1) switchOff1 = transform.Find("SwitchOff1")?.gameObject;
        if (!switchOff2) switchOff2 = transform.Find("SwitchOff2")?.gameObject;
        if (!switchOff3) switchOff3 = transform.Find("SwitchOff3")?.gameObject;
        if (!switchOff4) switchOff4 = transform.Find("SwitchOff4")?.gameObject;
        if (!switchOn1) switchOn1 = transform.Find("SwitchOn1")?.gameObject;
        if (!switchOn2) switchOn2 = transform.Find("SwitchOn2")?.gameObject;
        if (!switchOn3) switchOn3 = transform.Find("SwitchOn3")?.gameObject;
        if (!switchOn4) switchOn4 = transform.Find("SwitchOn4")?.gameObject;

        UpdateSwitchVisuals();
    }

    public override void OnNetworkSpawn()
    {
        // Subscribe to changes and notify GameSessionManager when all switches are on
        switch1On.OnValueChanged += OnSwitchChanged;
        switch2On.OnValueChanged += OnSwitchChanged;
        switch3On.OnValueChanged += OnSwitchChanged;
        switch4On.OnValueChanged += OnSwitchChanged;

        UpdateSwitchVisuals();
        CheckAndNotifyAllSwitchesOn();
        
        Debug.Log($"[Switchboard {boardColor}] NetworkSpawned. IsServer: {IsServer}");
    }

    private void OnSwitchChanged(bool oldVal, bool newVal)
    {
        UpdateSwitchVisuals();
        CheckAndNotifyAllSwitchesOn();
    }

    private void UpdateSwitchVisuals()
    {
        if (switchOff1) switchOff1.SetActive(!switch1On.Value);
        if (switchOn1) switchOn1.SetActive(switch1On.Value);

        if (switchOff2) switchOff2.SetActive(!switch2On.Value);
        if (switchOn2) switchOn2.SetActive(switch2On.Value);

        if (switchOff3) switchOff3.SetActive(!switch3On.Value);
        if (switchOn3) switchOn3.SetActive(switch3On.Value);

        if (switchOff4) switchOff4.SetActive(!switch4On.Value);
        if (switchOn4) switchOn4.SetActive(switch4On.Value);
    }

    private void CheckAndNotifyAllSwitchesOn()
    {
        if (!IsServer) return;

        bool allOn = AllSwitchesOn();
        
        // Notify GameSessionManager to activate this door color
        if (GameSessionManager.Instance != null)
        {
            GameSessionManager.Instance.SetSwitchboardCompleteServerRpc(boardColor, allOn);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void ToggleSwitchServerRpc(int switchNumber)
    {
        Debug.Log($"[Switchboard {boardColor}] ToggleSwitchServerRpc called for switch {switchNumber}");
        
        if (!IsServer)
        {
            Debug.LogError($"[Switchboard {boardColor}] ServerRpc called but not on server!");
            return;
        }

        switch (switchNumber)
        {
            case 1: switch1On.Value = !switch1On.Value; break;
            case 2: switch2On.Value = !switch2On.Value; break;
            case 3: switch3On.Value = !switch3On.Value; break;
            case 4: switch4On.Value = !switch4On.Value; break;
        }

        Debug.Log($"[Switchboard {boardColor}] Switch {switchNumber} toggled to {GetSwitchState(switchNumber)}");
    }

    public bool GetSwitchState(int switchNumber)
    {
        return switchNumber switch
        {
            1 => switch1On.Value,
            2 => switch2On.Value,
            3 => switch3On.Value,
            4 => switch4On.Value,
            _ => false
        };
    }

    public bool AllSwitchesOn()
    {
        return switch1On.Value && switch2On.Value && switch3On.Value && switch4On.Value;
    }

    public bool AllSwitchesOff()
    {
        return !switch1On.Value && !switch2On.Value && !switch3On.Value && !switch4On.Value;
    }

    public DoorColor GetBoardColor()
    {
        return boardColor;
    }
}