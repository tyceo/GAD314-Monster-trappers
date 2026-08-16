using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CodeEntryPanel : MonoBehaviour
{
    public DoorColor color;
    public GameObject panelRoot;
    public TMP_InputField codeInputField;
    public Button submitButton;
    public TMP_Text feedbackText;

    private void Awake()
    {
        submitButton.onClick.AddListener(OnSubmit);
        SetVisible(false);
    }

    public void SetVisible(bool visible)
    {
        panelRoot.SetActive(visible);
        if (!visible) return;
        codeInputField.text = "";
        if (feedbackText) feedbackText.text = "";
    }

    public void Hide()
    {
        panelRoot.SetActive(false);
    }

    public void ShowError()
    {
        if (feedbackText) feedbackText.text = "Incorrect code";
    }

    private void OnSubmit()
    {
<<<<<<< HEAD
        string code = codeInputField.text.Trim();
        // GameSessionManager.Instance.SubmitCodeServerRpc(color, code);
=======
        //string code = codeInputField.text.Trim();
        //GameSessionManager.Instance.SubmitCodeServerRpc(color, code);
>>>>>>> Thomas-28-07-2026
    }
}