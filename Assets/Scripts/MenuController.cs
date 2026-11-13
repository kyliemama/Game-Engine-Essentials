using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject creditsPanel; // Leave empty: the scene has no Credits panel yet.

    [Header("Sound")]
    [SerializeField] private AudioSource uiSound;     // Same UISound object used by VolumeController.
    [SerializeField] private AudioClip buttonClip;    // Plays on menu button presses.

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "SampleScene";

    private void Start()
    {
        ShowMainMenu();
    }

    public void StartGame()
    {
        PlayButtonSound();
        SceneManager.LoadScene(gameSceneName);
    }

    // The main menu stays visible behind the panel.
    public void OpenSettings()
    {
        PlayButtonSound();
        SetPanel(creditsPanel, false);
        SetPanel(settingsPanel, true);
    }

    // The main menu stays visible behind the panel.
    public void OpenCredits()
    {
        PlayButtonSound();
        SetPanel(settingsPanel, false);
        SetPanel(creditsPanel, true);
    }

    // Closes the open Settings or Credits panel and returns to the main menu.
    public void ClosePanel()
    {
        PlayButtonSound();
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
        PlayButtonSound();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void PlayButtonSound()
    {
        if (uiSound != null && buttonClip != null)
        {
            uiSound.PlayOneShot(buttonClip);
        }
    }

    private void SetPanel(GameObject panel, bool isVisible)
    {
        if (panel != null)
        {
            panel.SetActive(isVisible);
        }
    }
}
