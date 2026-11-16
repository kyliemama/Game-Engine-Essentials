using UnityEngine;

// Put on the Player. Touching a candy stores it. Press Q to eat one and restore HP.
public class PlayerCandyCollector : MonoBehaviour
{
    [Header("Eating")]
    public KeyCode eatKey = KeyCode.Q;
    public int healPerCandy = 25;

    public int CandyCount { get; private set; }

    private PlayerHealth health;

    private void Start()
    {
        health = GetComponent<PlayerHealth>();
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
        Debug.Log("Player touched: " + other.name);

        CandyPickup candy = other.GetComponentInParent<CandyPickup>();

        if (candy != null)
        {
            Debug.Log("Candy detected!");
            AddCandy(candy);
        }
    }

    // Stores a candy and removes it from the world. Ignores candies already taken.
    public void AddCandy(CandyPickup candy)
    {
        if (candy == null || candy.IsCollected) return;

        CandyCount++;
        candy.Collect();
    }

    private void EatCandy()
    {
        if (CandyCount <= 0 || health == null) return;

        CandyCount--;
        health.Heal(healPerCandy);
        Debug.Log("Ate a candy. HP +" + healPerCandy + ", candies left: " + CandyCount);
    }
}
