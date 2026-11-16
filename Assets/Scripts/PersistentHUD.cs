using UnityEngine;
using UnityEngine.SceneManagement;

// Keeps the game HUD (inventory bar, settings button and settings panel) alive across scenes.
// The HUD stays hidden in the scene named hiddenInScene.
public class PersistentHUD : MonoBehaviour
{
    public static PersistentHUD instance;

    // True while the settings panel is open. Game scripts check this to block attacks and movement.
    public static bool SettingsOpen =>
        instance != null && instance.settingsPanel != null && instance.settingsPanel.activeSelf;

    [SerializeField] Canvas hudCanvas;                 // The HUD Canvas, a child of this object.
    [SerializeField] GameObject settingsPanel;         // Settings panel inside hudCanvas. Starts inactive.
    [SerializeField] UnityEngine.UI.Graphic hudPanelGraphic; // Image of HUDPanel. Clicks pass through it.
    [SerializeField] string hiddenInScene = "MainMenu";

    private void Awake()
    {
        if (instance != null)
        {
            // A HUD already exists from an earlier scene.
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        UpdateVisibility(SceneManager.GetActiveScene().name);

        // Let clicks pass through the HUD panel, so it never blocks the settings sliders.
        if (hudPanelGraphic != null) hudPanelGraphic.raycastTarget = false;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateVisibility(scene.name);
    }

    private void UpdateVisibility(string sceneName)
    {
        if (hudCanvas != null)
        {
            hudCanvas.gameObject.SetActive(sceneName != hiddenInScene);
        }
    }

    // Esc opens the settings panel, or closes it when it is already open.
    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;
        if (hudCanvas == null || !hudCanvas.gameObject.activeSelf) return;
        if (InventoryUI.IsOpen) return;

        if (SettingsOpen)
            CloseSettings();
        else
            OpenSettings();
    }

    // Settings button: shows or hides the settings panel.
    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
            // Draw the settings panel on top of its sibling panels, such as HUDPanel.
            settingsPanel.transform.SetAsLastSibling();
            // The mouse must be free to click and drag the sliders.
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    // Close button on the settings panel.
    public void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);

            // Lock the mouse again for play, unless the inventory window is open.
            if (!InventoryUI.IsOpen)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }
}
