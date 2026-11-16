using System.Collections.Generic;
using UnityEngine;

// Switches the background music to Enemy Music while any enemy has the Player in its detection range.
// The enemy AI scripts call Track() each frame, and call Track(this, false) when they are disabled.
public static class EnemyMusic
{
    static readonly HashSet<Object> aware = new HashSet<Object>();

    public static void Track(Object enemy, bool playerDetected)
    {
        bool before = aware.Count > 0;

        if (playerDetected) aware.Add(enemy);
        else aware.Remove(enemy);

        // Forget enemies that were destroyed without calling Track(false).
        aware.RemoveWhere(o => o == null);

        bool after = aware.Count > 0;
        if (before == after || AudioManager.instance == null) return;

        if (after) AudioManager.instance.PlayEnemyMusic();
        else AudioManager.instance.PlayNormalMusic();
    }
}
