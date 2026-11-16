using System.Collections.Generic;
using UnityEngine;

public class KeepAcrossScenes : MonoBehaviour
{
    private static HashSet<string> persistedObjects = new HashSet<string>();

    void Awake()
    {
        // Use the GameObject's name as a unique key
        string id = gameObject.name;

        if (persistedObjects.Contains(id))
        {
            Destroy(gameObject);
            return;
        }

        persistedObjects.Add(id);
        DontDestroyOnLoad(gameObject);
    }
}
