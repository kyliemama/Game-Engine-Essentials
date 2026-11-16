using UnityEngine;
using UnityEngine.SceneManagement;

// Sends the player to the main menu after a loss. The main menu then shows the ending panel.
public static class GameEnding
{
    public static bool LosePending { get; private set; }
    public static bool WinPending { get; private set; }

    static bool losing;
    static bool winning;

    // Called by PlayerHealth when HP reaches 0.
    public static void LoseGame()
    {
        if (losing) return;

        losing = true;
        LosePending = true;
        GameManager.Get().ResetRun();
        SceneManager.LoadScene("MainMenu");
    }

    // Called by the chest (Explosion) when the final boss is defeated and the chest opens.
    public static void WinGame()
    {
        if (winning || losing) return;

        winning = true;
        WinPending = true;
        GameManager.Get().ResetRun();
        SceneManager.LoadScene("MainMenu");
    }

    // Called by MenuController after it shows the ending.
    public static void ClearPending()
    {
        LosePending = false;
        WinPending = false;
        losing = false;
        winning = false;
    }
}
