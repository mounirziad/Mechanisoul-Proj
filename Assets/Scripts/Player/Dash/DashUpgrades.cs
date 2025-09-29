using UnityEngine;

[System.Serializable]
public struct DashUpgrades
{
    [Header("Anger")]
    [Tooltip("Percent of dash-base damage applied in the Fire/DoT zone (0.05 = 5%).")]
    public float angerAOEPercent;
    public bool angerDoTOnDash; // enable/disable spawning a fire DoT zone on dash

    [Header("Sadness")]
    [Range(0f, 1f)] public float sadnessSlowPercent;
    public float sadnessSlowSeconds;
}
