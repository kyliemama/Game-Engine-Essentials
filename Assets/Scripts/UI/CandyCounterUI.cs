using UnityEngine;
using TMPro;

// Bottom-right candy frame. Shows the candy count from GameManager.
public class CandyCounterUI : MonoBehaviour
{
    [SerializeField] TMP_Text countText; // The CandyCount text.

    void Update()
    {
        if (countText == null) return;

        countText.text = GameManager.Get().CandyCount.ToString();
    }
}
