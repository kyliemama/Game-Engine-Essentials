using UnityEngine;

public class TeleportLevels : MonoBehaviour
{
    [SerializeField] private Staircase staircaseElevator;
    [SerializeField] private Transform hatch1To2Destination;
    [SerializeField] private GameObject floor1Group;
    [SerializeField] private GameObject floor2Group;
    [SerializeField] private Transform hatch2To3Destination;
    [SerializeField] private GameObject floor3Group;
    [SerializeField] private Vector3 landingOffset = new Vector3(0f, 1.2f, -1.2f);
    [SerializeField] private float teleportCooldown = 1.5f;

    private int currentFloor = 1;
    private float lastTeleportTime = -10f;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (Time.time - lastTeleportTime < teleportCooldown) return;

        if (currentFloor == 1 && hatch1To2Destination != null)
        {
            lastTeleportTime = Time.time;
            currentFloor = 2;

            if (floor2Group != null) floor2Group.SetActive(true);
            if (floor1Group != null) floor1Group.SetActive(false);
            if (staircaseElevator != null) staircaseElevator.CloseHatch1To2();

            MovePlayer(other.transform, hatch1To2Destination);
        }
        else if (currentFloor == 2 && hatch2To3Destination != null)
        {
            lastTeleportTime = Time.time;
            currentFloor = 3;

            if (floor3Group != null) floor3Group.SetActive(true);
            if (floor2Group != null) floor2Group.SetActive(false);

            MovePlayer(other.transform, hatch2To3Destination);
        }
    }

    private void MovePlayer(Transform player, Transform hatchTarget)
    {
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.position = hatchTarget.position + landingOffset;

        if (cc != null) cc.enabled = true;
    }
}
