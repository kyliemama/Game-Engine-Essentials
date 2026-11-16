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

        if (cached && !warnedNoHealth && !cached.GetComponentInParent<PlayerHealth>() &&
            !cached.GetComponentInChildren<PlayerHealth>())
        {
            warnedNoHealth = true;
            Debug.LogWarning("PlayerLocator: the Player has no PlayerHealth component, so enemy projectiles " +
                             "can't damage it. Add PlayerHealth to the Player.", cached);
        }

        return cached;
    }
}
