
using UnityEngine;

public class LollipopKeyPickup : MonoBehaviour
{
    private GameObject deathEffect;
    private bool collected = false;

    public void SetDeathEffect(GameObject effect)
    {
        deathEffect = effect;
    }

    void OnTriggerEnter(Collider other)
    {
        if (collected || !other.transform.root.CompareTag("Player"))
            return;

        collected = true;

        Debug.Log("Lollipop key collected!");

        GameManager.Get().GiveKey();

        if (deathEffect != null)
            Destroy(deathEffect);

        Destroy(gameObject);
    }
}
