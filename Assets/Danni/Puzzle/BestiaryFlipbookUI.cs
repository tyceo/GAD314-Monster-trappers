using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class BestiaryPage
{
    public string monsterName;
    [TextArea(3, 6)]
    public string description;
    // public Sprite illustration;
}

public class BestiaryFlipbookUI : MonoBehaviour
{
    public BestiaryPage[] pages;
    public TMP_Text nameText;
    public TMP_Text descriptionText;
    // public Image illustrationImage;
    public Button previousButton;
    public Button nextButton;
    public Button closeButton;
    public Button openButton;
    public GameObject bestiaryFlipbookPanel;

    private int currentIndex;

    private void Awake()
    {
        previousButton.onClick.AddListener(ShowPrevious);
        nextButton.onClick.AddListener(ShowNext);
        closeButton.onClick.AddListener(CloseBook);
        openButton.onClick.AddListener(OpenBook);
        ShowPage(0);
    }

    private void CloseBook()
    {
        bestiaryFlipbookPanel.SetActive(false);
    }
    
    private void OpenBook()
    {
        bestiaryFlipbookPanel.SetActive(true);
    }
    
    private void ShowPrevious()
    {
        int newIndex = currentIndex - 1;
        if (newIndex < 0) newIndex = pages.Length - 1;
        ShowPage(newIndex);
    }

    private void ShowNext()
    {
        int newIndex = currentIndex + 1;
        if (newIndex >= pages.Length) newIndex = 0;
        ShowPage(newIndex);
    }

    private void ShowPage(int index)
    {
        if (pages.Length == 0) return;
        currentIndex = index;

        BestiaryPage page = pages[currentIndex];
        nameText.text = page.monsterName;
        descriptionText.text = page.description;
        // if (illustrationImage) illustrationImage.sprite = page.illustration;
    }
}
