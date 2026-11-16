using UnityEngine;

// Put on the Player. Touching a candy adds it to GameManager's count. Press Q to eat one and restore HP.
public class PlayerCandyCollector : MonoBehaviour
{
    [Header("Eating")]
    public KeyCode eatKey = KeyCode.Q;
    public int healPerCandy = 25;

    private PlayerHealth health;

    private void Start()
    {
        health = GetComponent<PlayerHealth>();

        // Eating candy heals PlayerHealth, so make sure the Player has one.
        if (health == null)
        {
            PlayerLocator.EnsurePlayerComponents(transform);
            health = GetComponentInParent<PlayerHealth>();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(eatKey))
        {
            EatCandy();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        CandyPickup candy = other.GetComponentInParent<CandyPickup>();

        if (candy != null)
        {
            AddCandy(candy);
        }
    }

    // Stores a candy in GameManager and removes it from the world. Ignores candies already taken.
    public void AddCandy(CandyPickup candy)
    {
        if (candy == null || candy.IsCollected) return;
        GameManager.Get().AddCandy();
        candy.Collect();
    }

    private void EatCandy()
    {
        if (health == null) return;

        if (GameManager.Get().EatCandy())
        {
            health.Heal(healPerCandy);
        }
    }
}
