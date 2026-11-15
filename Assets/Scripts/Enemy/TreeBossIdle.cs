
using UnityEngine;

public class TreeBossIdle : MonoBehaviour
{
    [Header("Idle Movement")]
    public float swayAmount = 2f;
    public float swaySpeed = 1f;

    [Header("Floating Movement")]
    public float bobAmount = 0.05f;
    public float bobSpeed = 1.5f;

    private Quaternion startRotation;
    private Vector3 startPosition;

    void Start()
    {
        startRotation = transform.localRotation;
        startPosition = transform.localPosition;
    }

    void Update()
    {
        float sway = Mathf.Sin(Time.time * swaySpeed) * swayAmount;
        float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmount;

        transform.localRotation =
            startRotation * Quaternion.Euler(0f, 0f, sway);

        transform.localPosition =
            startPosition + Vector3.up * bob;
    }
}
