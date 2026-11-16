using UnityEngine;

// Put on each enemy. Switches the background music while the enemy can see the player.
// Music goes back to normal when no enemy sees the player.
public class EnemySight : MonoBehaviour
{
    [SerializeField] Transform player;                 // Leave empty to find the object tagged Player.
    [SerializeField] float sightRange = 15f;
    [SerializeField] float eyeHeight = 1.5f;
    [SerializeField] LayerMask blockingLayers = ~0;    // Walls and ground that block sight.

    static int enemiesSeeingPlayer;
    bool seesPlayer;

    void Start()
    {
        if (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null) player = found.transform;
        }
    }

    void Update()
    {
        SetSeesPlayer(CanSeePlayer());
    }

    void OnDisable()
    {
        SetSeesPlayer(false);
    }

    void SetSeesPlayer(bool sees)
    {
        if (sees == seesPlayer) return;
        seesPlayer = sees;

        enemiesSeeingPlayer += sees ? 1 : -1;
        enemiesSeeingPlayer = Mathf.Max(enemiesSeeingPlayer, 0);

        if (AudioManager.instance == null) return;

        if (enemiesSeeingPlayer > 0)
            AudioManager.instance.PlayEnemyMusic();
        else
            AudioManager.instance.PlayNormalMusic();
    }

    bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 eye = transform.position + Vector3.up * eyeHeight;
        Vector3 target = player.position + Vector3.up * eyeHeight;
        Vector3 toPlayer = target - eye;

        if (toPlayer.magnitude > sightRange) return false;

        if (Physics.Raycast(eye, toPlayer.normalized, out RaycastHit hit, sightRange, blockingLayers, QueryTriggerInteraction.Ignore))
        {
            return hit.transform == player || hit.transform.IsChildOf(player);
        }

        return false;
    }
}
