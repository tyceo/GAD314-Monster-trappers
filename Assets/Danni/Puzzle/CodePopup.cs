using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class CodePopup : MonoBehaviour
{
    public static CodePopup Instance { get; private set; }

    public GameObject popupRoot;
    public TMP_Text doorLabelText;
    public TMP_Text codeText;
    public Button closeButton;

    private void Awake()
    {
        Instance = this;
        closeButton.onClick.AddListener(Close);
        popupRoot.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Close();
        }
    }

    public void Show(DoorColor color, string code)
    {
        doorLabelText.text = $"{color} Door Code";
        codeText.text = code;
        popupRoot.SetActive(true);
    }

    public void Close()
    {
        popupRoot.SetActive(false);
    }
}