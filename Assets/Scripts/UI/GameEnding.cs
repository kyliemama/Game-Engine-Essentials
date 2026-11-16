using UnityEngine;
using UnityEngine.SceneManagement;

// Sends the player to the main menu after a loss. The main menu then shows the ending panel.
public static class GameEnding
{
    public static bool LosePending { get; private set; }

    static bool losing;

    // Called by PlayerHealth when HP reaches 0.
    public static void LoseGame()
    {
        if (losing) return;

        losing = true;
        LosePending = true;
        SceneManager.LoadScene("MainMenu");
    }

    // Called by MenuController after it shows the ending.
    public static void ClearPending()
    {
        LosePending = false;
        losing = false;
    }
}
