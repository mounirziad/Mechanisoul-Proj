using UnityEngine;

[System.Serializable]
public struct RangedModifiers
{
    // joy
    public float joyFireRateMultiplier;
    public float joyDamageMultiplier;
    public float joyCritChance;
    public float joyCritMultiplier;
    public bool joyExplosionOnHit;

    // anger
    public int angerPelletCount;
    public float angerPelletSpreadDeg;
    public float angerRangeMultiplier;
    public float angerFireRateMultiplier;
    public float angerAOEPercent;
    public bool angerExplosionOnHit;

    // sadness
    public float sadnessSlowPerStack;
    public int sadnessMaxStacks;

    // love
    public float loveCharmChance;
    public float loveCharmDuration;

    // fear
    public float fearStunDuration;
    public float fearStunRadius;
}
