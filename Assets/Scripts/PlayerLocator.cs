using UnityEngine;

/// <summary>
/// Finds the Player for enemy scripts. Looks for the "Player" tag first, then for an object with
/// PlayerHealth, then PlayerMovement. The result is cached until the Player is destroyed
/// (e.g. on a scene change), and failed searches are retried at most twice a second.
/// </summary>
public static class PlayerLocator
{
    static Transform cached;
    static float nextSearchTime;
    static bool warnedNoHealth;

    /// <summary>
    /// Returns 'current' if it is a real Player in a loaded scene, otherwise finds the Player.
    /// Fixes Inspector fields that were dragged from the Player prefab asset (Project window)
    /// instead of the Player in the scene - those point at the prefab file, never move, and
    /// make enemies aim at the wrong spot.
    /// </summary>
    public static Transform Resolve(Transform current)
    {
        if (current && current.gameObject.scene.IsValid()) return current;
        return Find();
    }

    /// <summary>
    /// True if 'from' can see the Player: not too far above/below (different floor) and no wall or
    /// floor in between. Colliders belonging to 'self' or to the Player are ignored.
    /// </summary>
    public static bool HasLineOfSight(Vector3 from, Transform player, Transform self, float maxHeightDifference)
    {
        if (!player) return false;

        Vector3 to = player.position + Vector3.up * 1f;   // aim at the chest, not the feet
        if (Mathf.Abs(to.y - from.y) > maxHeightDifference) return false;

        Vector3 dir = to - from;
        RaycastHit[] hits = Physics.RaycastAll(from, dir.normalized, dir.magnitude, ~0, QueryTriggerInteraction.Ignore);
        foreach (RaycastHit h in hits)
        {
            Transform t = h.collider.transform;
            if (self && t.IsChildOf(self.root)) continue;
            if (t.IsChildOf(player.root)) continue;
            return false;   // a wall, floor or ceiling is in the way
        }
        return true;
    }

    /// <summary>
    /// Makes sure the Player has PlayerHealth (enemies damage it) and PlayerCandyCollector (picks up
    /// and eats candy). The Player prefab doesn't have them, so they're added at runtime if missing.
    /// </summary>
    public static void EnsurePlayerComponents(Transform player)
    {
        if (!player) return;

        GameObject root = player.gameObject;
        CharacterController cc = player.GetComponentInParent<CharacterController>();
        if (cc) root = cc.gameObject;

        if (!root.GetComponentInParent<PlayerHealth>() && !root.GetComponentInChildren<PlayerHealth>())
        {
            root.AddComponent<PlayerHealth>();
            if (!warnedNoHealth)
            {
                warnedNoHealth = true;
                Debug.LogWarning("PlayerLocator: the Player had no PlayerHealth, so one was added at runtime. " +
                                 "Add PlayerHealth to the Player prefab to set its Max Health.", root);
            }
        }

        if (!root.GetComponentInParent<PlayerCandyCollector>() && !root.GetComponentInChildren<PlayerCandyCollector>())
            root.AddComponent<PlayerCandyCollector>();
    }

    public static Transform Find()
    {
        if (cached) return cached;
        if (Time.unscaledTime < nextSearchTime) return null;
        nextSearchTime = Time.unscaledTime + 0.5f;

        // The real Player is the object that moves (PlayerMovement). Prefer it, so an old test
        // player or a model child that is also tagged "Player" can't be picked by mistake.
        PlayerMovement pm = Object.FindFirstObjectByType<PlayerMovement>();
        if (pm) cached = pm.transform;

        if (!cached)
        {
            GameObject tagged = GameObject.FindGameObjectWithTag("Player");
            if (tagged) cached = tagged.transform;
        }

        if (!cached)
        {
            PlayerHealth ph = Object.FindFirstObjectByType<PlayerHealth>();
            if (ph) cached = ph.transform;
        }

        if (cached) EnsurePlayerComponents(cached);

        return cached;
    }
}
