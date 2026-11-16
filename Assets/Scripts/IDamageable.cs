/// <summary>
/// Anything that can take damage. Every health script (Health, PlayerHealth, BatHealth,
/// EnemyHealth, TreeBossHealth, GhostBossHealth) implements this, so attacks only need to
/// look for IDamageable instead of knowing about each enemy type.
/// </summary>
public interface IDamageable
{
    void TakeDamage(int amount);
}
