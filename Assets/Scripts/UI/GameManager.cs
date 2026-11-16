using System;
using UnityEngine;

// Keeps the candy count and the key state for the whole game.
// Put on the GameManager object in MainMenu, the same object as SceneController, so it survives scene changes.
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int CandyCount { get; private set; }
    public bool HasKey { get; private set; }

    // Fires whenever the candy count or the key changes.
    public event Action OnChanged;

    void Awake()
    {
        if (instance != null)
        {
            // A GameManager already exists from an earlier scene.
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Returns the GameManager. If none exists, such as when you test straight from Enemy, it creates one.
    public static GameManager Get()
    {
        if (instance == null)
        {
            GameObject go = new GameObject("GameManager");
            go.AddComponent<GameManager>(); // Awake sets instance and keeps it across scenes.
        }

        return instance;
    }

    public void AddCandy()
    {
        CandyCount++;
        OnChanged?.Invoke();
    }

    // Returns false when there are no candies to eat.
    public bool EatCandy()
    {
        if (CandyCount <= 0) return false;

        CandyCount--;
        OnChanged?.Invoke();
        return true;
    }

    // Called when the lollipop key is picked up.
    public void GiveKey()
    {
        HasKey = true;
        OnChanged?.Invoke();
    }

    // Called when the key is used.
    public void UseKey()
    {
        HasKey = false;
        OnChanged?.Invoke();
    }
}
