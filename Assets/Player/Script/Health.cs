using UnityEngine;
using UnityEngine.Events;
 
/// <summary>Put on anything that can be hit (enemies, props, the player).</summary>
public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    public int current;
    public UnityEvent<int> onDamaged;
    public UnityEvent onDied;
 
    void Awake() => current = maxHealth;
 
    public void Heal(int amount)
    {
        if (current <= 0) return;
        current = Mathf.Min(maxHealth, current + amount);
    }
 
    public void TakeDamage(int amount)
    {
        if (current <= 0) return;
 
        current -= amount;
        onDamaged?.Invoke(amount);
 
        if (current <= 0)
        {
            current = 0;
            onDied?.Invoke();
        }
    }
}
 