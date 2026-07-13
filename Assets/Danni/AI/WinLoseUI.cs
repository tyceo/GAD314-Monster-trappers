using UnityEngine;
using UnityEngine.UI;

public class WinLoseUI : MonoBehaviour
{
    public GameObject winPanel;
    public GameObject losePanel;
    public Button restartButton;
    public Button respawnButton;

    private void Awake()
    {
        restartButton.onClick.AddListener(() => GameSessionManager.Instance.RestartRelay());
        respawnButton.onClick.AddListener(() => GameSessionManager.Instance.RespawnLevel());
    }

    public void ShowWin()
    {
        winPanel.SetActive(true);
        losePanel.SetActive(false);
    }

    public void ShowLose()
    {
        winPanel.SetActive(false);
        losePanel.SetActive(true);
    }

    public void HideAll()
    {
        winPanel.SetActive(false);
        losePanel.SetActive(false);
    }
}