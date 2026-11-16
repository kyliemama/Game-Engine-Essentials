using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel; // Leave empty to skip the Credits panel.

    [Header("Ending")]
    [SerializeField] private GameObject endingPanel;   // Shown after the player loses. Its Quit button calls ClosePanel.
    [SerializeField] private Image endingPicture;      // The picture on the ending panel.
    [SerializeField] private TMP_Text endingText;      // The words on the ending panel.
    [SerializeField] private Sprite losePicture;       // Picture for a loss.
    [SerializeField, TextArea] private string loseText = "You were defeated.";
    [Tooltip("Win ending: the picture and text already set on the ending panel are kept. Leave empty to keep the panel's text.")]
    [SerializeField, TextArea] private string winText = "";

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "Enemy";

    private void Start()
    {
        // The menu needs a free mouse. The game locks it.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ShowMainMenu();

        if (GameEnding.LosePending)
        {
            ShowEnding(losePicture, loseText);
        }
        else if (GameEnding.WinPending)
        {
            // Null picture keeps the panel's own (winning) picture.
            ShowEnding(null, string.IsNullOrEmpty(winText) ? null : winText);
        }
    }

    public void StartGame()
    {
        GameManager.Get().ResetRun();
        SceneManager.LoadScene(gameSceneName);
    }

    // The main menu stays visible behind the panel.
    public void OpenSettings()
    {
        SetPanel(creditsPanel, false);
        SetPanel(settingsPanel, true);
    }

    // The main menu stays visible behind the panel.
    public void OpenCredits()
    {
        SetPanel(settingsPanel, false);
        SetPanel(creditsPanel, true);
    }

    // Closes the open Settings, Credits or ending panel and returns to the main menu.
    public void ClosePanel()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        SetPanel(mainMenuPanel, true);
        SetPanel(settingsPanel, false);
        SetPanel(creditsPanel, false);
        SetPanel(endingPanel, false);
    }

    // Shows the ending panel with its picture and words, and hides the main menu.
    public void ShowEnding(Sprite picture, string text)
    {
        SetPanel(mainMenuPanel, false);
        SetPanel(settingsPanel, false);
        SetPanel(creditsPanel, false);
        SetPanel(endingPanel, true);

        if (endingPicture != null && picture != null) endingPicture.sprite = picture;
        if (endingText != null && text != null) endingText.text = text;

        GameEnding.ClearPending();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SetPanel(GameObject panel, bool isVisible)
    {
        if (panel != null)
        {
            panel.SetActive(isVisible);
        }
    }
}
