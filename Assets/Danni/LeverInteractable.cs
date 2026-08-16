using UnityEngine;

public class LeverInteractable : MonoBehaviour, IInteractable
{
    public DoorColor color;

    public void Interact()
    {
        LeverStation.Instance.ToggleLeverServerRpc(color);
    }
}
