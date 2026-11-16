using UnityEngine;

// Turns the key icon on while GameManager says the player has the key.
// Put this on an object that stays active, such as the Canvas. Do not put it on the KeyIcon
// itself, because an inactive object never runs its Update.
public class KeyIconUI : MonoBehaviour
{
    [SerializeField] GameObject keyIcon; // The KeyIcon object. It starts inactive.

    void Update()
    {
        if (keyIcon == null) return;

        bool hasKey = GameManager.Get().HasKey;
        if (keyIcon.activeSelf != hasKey) keyIcon.SetActive(hasKey);
    }
}
