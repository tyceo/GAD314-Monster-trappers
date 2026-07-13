using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ControllerButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public DoorColor color;
    public Image buttonImage;
    public Color activatedColor = Color.green;
    public Color inactiveColor = Color.gray;

    private bool isActivated;

    private void Awake()
    {
        if (!buttonImage) buttonImage = GetComponent<Image>();
        SetActivated(false);
    }

    public void SetActivated(bool activated)
    {
        isActivated = activated;
        buttonImage.color = activated ? activatedColor : inactiveColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!isActivated) return;
        GameSessionManager.Instance.SetDoorHeldServerRpc(color, true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isActivated) return;
        GameSessionManager.Instance.SetDoorHeldServerRpc(color, false);
    }
}