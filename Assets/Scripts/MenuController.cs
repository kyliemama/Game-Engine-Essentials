using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel; // Leave empty to skip the Credits panel.

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "SampleScene";

    private void Start()
    {
        ShowMainMenu();
    }

    public void StartGame()
    {
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

    // Closes the open Settings or Credits panel and returns to the main menu.
    public void ClosePanel()
    {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        SetPanel(mainMenuPanel, true);
        SetPanel(settingsPanel, false);
        SetPanel(creditsPanel, false);
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
