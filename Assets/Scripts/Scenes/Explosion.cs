using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    [SerializeField] private GameObject finalboss;
    [SerializeField] private Animator chestAnimator;
    [SerializeField] private float lidOpenDelay = 0.4f;
    [SerializeField] private Transform candiesParent;
    [SerializeField] private float upwardForce = 6f;
    [SerializeField] private float outwardForce = 4f;
    [SerializeField] private float spinForce = 8f;


    private bool playerInRange = false;
    private bool isOpened = false;
    private List<Rigidbody> chestCandies = new List<Rigidbody>();

    void Start()
    {
        if (candiesParent != null)
        {
            foreach (Transform child in candiesParent)
            {
                Rigidbody rb = child.GetComponent<Rigidbody>();
                if (rb == null) rb = child.gameObject.AddComponent<Rigidbody>();

                chestCandies.Add(rb);
                child.gameObject.SetActive(false);
            }
        }
    }

    void Update()
    {
        if (isOpened || !playerInRange) return;
        if (!IsBossDefeated()) return;
        if (Input.GetKeyDown(KeyCode.E))
        {
            isOpened = true;
            StartCoroutine(OpenAndExplodeCandies());
        }
    }

    bool IsBossDefeated()
    {
        return finalboss == null || !finalboss.activeInHierarchy;
    }

    IEnumerator OpenAndExplodeCandies()
    {
        if (chestAnimator != null)
        {
            chestAnimator.SetTrigger("Witch Defeated");
            yield return new WaitForSeconds(lidOpenDelay);
        }

        foreach (Rigidbody rb in chestCandies)
        {
            if (rb == null) continue;

            rb.transform.SetParent(null);
            rb.gameObject.SetActive(true);

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            Vector3 randomDir = new Vector3(
                Random.Range(-1f, 1f),
                0f,
                Random.Range(-1f, 1f)
            ).normalized;

            Vector3 launchVelocity = (randomDir * Random.Range(outwardForce * 0.5f, outwardForce))
                                   + (Vector3.up * Random.Range(upwardForce * 0.8f, upwardForce * 1.2f));

            rb.AddForce(launchVelocity, ForceMode.Impulse);
            rb.AddTorque(Random.insideUnitSphere * spinForce, ForceMode.Impulse);

            yield return new WaitForSeconds(0.02f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
