/// <summary>
/// How an attack finds its targets.
/// Slash = everything inside a box in front of the player (melee).
/// Ray   = a single ray or spherecast along the crosshair (guns, bows).
/// </summary>
public enum AttackShape
{
    Slash,
    Ray
}