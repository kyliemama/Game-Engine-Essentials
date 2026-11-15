
using UnityEngine;

public class PlayerCandyCollector : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Player touched: " + other.name);

        CandyPickup candy = other.GetComponentInParent<CandyPickup>();

        if (candy != null)
        {
            Debug.Log("Candy detected!");
            candy.Collect();
        }
    }
}
