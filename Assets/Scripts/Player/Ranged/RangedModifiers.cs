using UnityEngine;

[System.Serializable]
public struct RangedModifiers
{
    // JOY
    [Tooltip("1 = normal. >1 shoots faster (Rapid Fire).")]
    public float joyFireRateMultiplier; 
    [Tooltip("1 = normal. >1 increases projectile damage.")]
    public float joyDamageMultiplier; 

    // ANGER
    [Tooltip("Percent of projectile damage used by spawned AoE/DoT (0.15 = 15%).")]
    public float angerAOEPercent;
    [Tooltip("If true, spawn an explosion/DoT on impact.")]
    public bool angerExplosionOnHit;
}
