using UnityEngine;

public class LeverStationButton : MonoBehaviour, IInteractable
{
    public enum ButtonAction { Reset, Engage }
    public ButtonAction action;

    public void Interact()
    {
        if (action == ButtonAction.Reset) LeverStation.Instance.ResetServerRpc();
        else LeverStation.Instance.EngageServerRpc();
    }
}
