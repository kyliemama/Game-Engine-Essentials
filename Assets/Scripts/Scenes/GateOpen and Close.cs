using UnityEngine;

public class GateOpenandClose : MonoBehaviour
{
    public Transform leftHinge;
    public Transform rightHinge;
    public float openAngle = 90f;
    public float openSpeed = 2f;

    private Quaternion leftClosedRot;
    private Quaternion rightClosedRot;
    private Quaternion leftOpenRot;
    private Quaternion rightOpenRot;
    private bool isPlayerNearby = false;

    void Start()
    {
        leftClosedRot = leftHinge.localRotation;
        rightClosedRot = rightHinge.localRotation;

        leftOpenRot = leftClosedRot * Quaternion.Euler(0, -openAngle, 0);
        rightOpenRot = rightClosedRot * Quaternion.Euler(0, openAngle, 0);
    }

    void Update()
    {
        Quaternion targetLeft = isPlayerNearby ? leftOpenRot : leftClosedRot;
        Quaternion targetRight = isPlayerNearby ? rightOpenRot : rightClosedRot;

        leftHinge.localRotation = Quaternion.Slerp(leftHinge.localRotation, targetLeft, Time.deltaTime * openSpeed);
        rightHinge.localRotation = Quaternion.Slerp(rightHinge.localRotation, targetRight, Time.deltaTime * openSpeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }
}
