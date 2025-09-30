using UnityEngine;

[System.Serializable]
public struct RangedModifiers
{
    // JOY
    [Tooltip("1 = normal. >1 shoots faster (Rapid Fire).")]
    public float joyFireRateMultiplier;   // e.g., 1.00, 1.15, 1.30...
    [Tooltip("1 = normal. >1 increases projectile damage.")]
    public float joyDamageMultiplier;     // e.g., 1.00, 1.10, 1.20...
    [Tooltip("If true, spawn a JOY VFX explosion on enemy hit (visual only).")]
    public bool joyExplosionOnHit;

    // ANGER (ranged)
    [Tooltip("Percent of projectile damage used by spawned AoE/DoT (0.15 = 15%).")]
    public float angerAOEPercent;
    [Tooltip("If true, spawn an ANGER AoE/DoT on impact.")]
    public bool angerExplosionOnHit;
}
