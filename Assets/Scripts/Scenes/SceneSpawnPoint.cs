using UnityEngine;

public class SceneSpawnPoint : MonoBehaviour
{
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Temporarily disable CharacterController if your player uses one so position updates cleanly
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            player.transform.position = transform.position;
            player.transform.rotation = transform.rotation;

            if (cc != null) cc.enabled = true;
        }
    }
}
