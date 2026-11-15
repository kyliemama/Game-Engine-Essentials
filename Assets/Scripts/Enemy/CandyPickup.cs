
using UnityEngine;

public class CandyPickup : MonoBehaviour
{
    [Header("Candy Type")]
    public bool isHPCandy = true;

    [Header("HP Reward")]
    public int minHPRecovery = 20;
    public int maxHPRecovery = 30;

    [Header("MP Reward")]
    public int minMPRecovery = 20;
    public int maxMPRecovery = 30;

    private bool collected = false;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Candy touched by: " + other.name);

        // Check whether the object touching the candy
        // belongs to the Player
        CharacterController player =
            other.GetComponentInParent<CharacterController>();

        if (player == null || collected)
            return;

        Collect();
    }

    public void Collect()
    {
        if (collected)
            return;

        collected = true;

        if (isHPCandy)
        {
            int amount = Random.Range(
                minHPRecovery, maxHPRecovery + 1);

            Debug.Log("HP Candy collected! +" + amount + " HP");
        }
        else
        {
            int amount = Random.Range(
                minMPRecovery, maxMPRecovery + 1);

            Debug.Log("MP Candy collected! +" + amount + " MP");
        }

        // Remove the entire candy
        Destroy(transform.parent.gameObject);
    }
}
