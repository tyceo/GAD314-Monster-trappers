using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class InstructionPopup : MonoBehaviour
{
    public static InstructionPopup Instance { get; private set; }

    public GameObject popupRoot;
    public TMP_Text titleText;
    public TMP_Text bodyText;
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

    public void Show(string title, string body)
    {
        titleText.text = title;
        bodyText.text = body;
        popupRoot.SetActive(true);
    }

    public void Close()
    {
        popupRoot.SetActive(false);
    }
}
