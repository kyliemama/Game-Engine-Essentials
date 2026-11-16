using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KeepAcrossScenes : MonoBehaviour
{
    private static HashSet<string> persistedObjects = new HashSet<string>();

    [SerializeField] private string mainMenuSceneName = "MainMenu";

    void Awake()
    {
        string id = gameObject.name;

        if (persistedObjects.Contains(id))
        {
            Destroy(gameObject);
            return;
        }

        persistedObjects.Add(id);
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == mainMenuSceneName)
        {
            persistedObjects.Remove(gameObject.name);
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        persistedObjects.Remove(gameObject.name);
    }
}
