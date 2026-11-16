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
    [SerializeField] private GameObject miniboss;
    [SerializeField] private GameObject finalBoss;

    private int currentFloor = 1;
    private Transform playerInRange;

    private void Update()
    {
        if (playerInRange == null) return;

        if (Input.GetKeyDown(KeyCode.C))
        {
            TryTeleport();
        }
    }

    private void TryTeleport()
    {
        if (currentFloor == 1 && hatch1To2Destination != null)
        {
            if (!AreEnemiesDefeated(miniboss)) return;
            currentFloor = 2;

            if (floor2Group != null) floor2Group.SetActive(true);
            if (floor1Group != null) floor1Group.SetActive(false);
            if (staircaseElevator != null) staircaseElevator.CloseHatch1To2();

            MovePlayer(playerInRange, hatch1To2Destination);
            playerInRange = null; // Clear reference since player teleported away
        }
        else if (currentFloor == 2 && hatch2To3Destination != null)
        {
            if (!AreEnemiesDefeated(finalBoss)) return;
            currentFloor = 3;

            if (floor3Group != null) floor3Group.SetActive(true);
            if (floor2Group != null) floor2Group.SetActive(false);

            MovePlayer(playerInRange, hatch2To3Destination);
            playerInRange = null; // Clear reference since player teleported away
        }
    }

    private bool AreEnemiesDefeated(GameObject Enemies)
    {
        return Enemies == null || !Enemies.activeInHierarchy;
    }

    private void MovePlayer(Transform player, Transform hatchTarget)
    {
        CharacterController cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        player.position = hatchTarget.position + landingOffset;

        if (cc != null) cc.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = null;
        }
    }
}
