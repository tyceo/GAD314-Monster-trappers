using Unity.Netcode;
using UnityEngine;

public class KeyCube : NetworkBehaviour
{
    public DoorColor color;
    public Renderer keyRenderer;
    public Collider keyCollider;
    public Renderer keyVisual;

    private NetworkVariable<bool> isActive = new NetworkVariable<bool>(false);
    private bool collected;

    private void Awake()
    {
        if (!keyRenderer) keyRenderer = GetComponent<Renderer>();
        if (!keyVisual) keyVisual = GetComponentInChildren<Renderer>();
        if (!keyCollider) keyCollider = GetComponent<Collider>();
    }

    public override void OnNetworkSpawn()
    {
        isActive.OnValueChanged += (_, v) => UpdateVisibility(v);
        UpdateVisibility(isActive.Value);
    }

    private void UpdateVisibility(bool active)
    {
        if (keyRenderer) keyRenderer.enabled = active;
        if (keyCollider) keyCollider.enabled = active;
    }

    public void SetSolvedState(bool solved)
    {
        if (!IsServer) return;
        if (collected) return;
        isActive.Value = solved;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        if (!isActive.Value) return;
        if (!other.CompareTag("Player")) return;

        collected = true;
        isActive.Value = false;
        GameSessionManager.Instance.CollectKey(color);
    }

    public void ResetKey()
    {
        if (!IsServer) return;
        collected = false;
        isActive.Value = false;
    }
}