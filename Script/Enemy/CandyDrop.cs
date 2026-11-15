
using UnityEngine;

public class CandyDrop : MonoBehaviour
{
    [Header("Candy Prefabs")]
    public GameObject hpCandyPrefab;
    public GameObject mpCandyPrefab;

    [Header("Drop Amount")]
    public int minCandy = 2;
    public int maxCandy = 3;

    [Header("Throw Settings")]
    public float throwForce = 1.5f;
    public float upwardForce = 1.2f;

    [Header("Drop Sound")]
    public AudioClip candyPopSound;
    [Range(0f, 1f)]
    public float popVolume = 0.7f;

    public void DropCandies()
    {
        if (hpCandyPrefab == null || mpCandyPrefab == null)
        {
            Debug.LogError("Please assign both candy prefabs!");
            return;
        }

        // Play candy pop sound
        if (candyPopSound != null)
        {
            AudioSource.PlayClipAtPoint(
                candyPopSound,
                transform.position,
                popVolume
            );
        }

        int candyAmount = Random.Range(minCandy, maxCandy + 1);

        for (int i = 0; i < candyAmount; i++)
        {
            GameObject selectedCandy;

            // Guarantee one HP and one MP candy
            if (i == 0)
                selectedCandy = hpCandyPrefab;
            else if (i == 1)
                selectedCandy = mpCandyPrefab;
            else
                selectedCandy = Random.value < 0.5f
                    ? hpCandyPrefab
                    : mpCandyPrefab;

            Vector3 spawnPosition =
                transform.position + Vector3.up;

            GameObject candy = Instantiate(
                selectedCandy,
                spawnPosition,
                Random.rotation
            );

            Rigidbody rb = candy.GetComponent<Rigidbody>();

            if (rb != null)
            {
                Vector3 randomDirection = new Vector3(
                    Random.Range(-1f, 1f),
                    upwardForce,
                    Random.Range(-1f, 1f)
                ).normalized;

                rb.AddForce(
                    randomDirection * throwForce,
                    ForceMode.Impulse
                );
            }
        }

        Debug.Log("Pumpkin dropped " + candyAmount + " candies!");
    }
}
