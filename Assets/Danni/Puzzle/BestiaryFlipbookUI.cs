using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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

    [Header("Always On Top")]
    public int forcedSortingOrder = 1000;

    private int currentIndex;
    private bool explorerLockout;

    private void Awake()
    {
        previousButton.onClick.AddListener(ShowPrevious);
        nextButton.onClick.AddListener(ShowNext);
        closeButton.onClick.AddListener(CloseBook);
        openButton.onClick.AddListener(OpenBook);
        ShowPage(0);

        ForceOnTop(bestiaryFlipbookPanel);
        ForceOnTop(openButton.gameObject);

        StartCoroutine(HideForExplorerWhenReady());
    }

    private void ForceOnTop(GameObject target)
    {
        if (!target) return;

        Canvas canvas = target.GetComponent<Canvas>();
        if (!canvas) canvas = target.AddComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = forcedSortingOrder;

        if (!target.GetComponent<GraphicRaycaster>())
            target.AddComponent<GraphicRaycaster>();

        target.transform.SetAsLastSibling();
    }

    private IEnumerator HideForExplorerWhenReady()
    {
        while (NetworkManager.Singleton == null || NetworkManager.Singleton.LocalClient == null)
            yield return null;

        bool isExplorer = NetworkManager.Singleton.LocalClient.PlayerObject != null;
        if (isExplorer)
        {
            explorerLockout = true;
            openButton.gameObject.SetActive(false);
            bestiaryFlipbookPanel.SetActive(false);
        }
    }

    private void CloseBook()
    {
        bestiaryFlipbookPanel.SetActive(false);
    }

    private void OpenBook()
    {
        if (explorerLockout) return;
        bestiaryFlipbookPanel.SetActive(true);
        bestiaryFlipbookPanel.transform.SetAsLastSibling();
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
