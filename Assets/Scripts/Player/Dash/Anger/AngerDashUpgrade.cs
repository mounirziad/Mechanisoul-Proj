using UnityEngine;

public class AngerDashUpgrade : DashUpgradeBase
{
    [Header("Anger Burst")]
    [Tooltip("Extra % damage applied to the burst (index by upgrade level). 0.25 = +25%.")]
    public float[] aoePercent = { 0f, 0.10f, 0.25f };

    [Header("Burst Prefab")]
    public AngerBurstZone fireBurstPrefab;

    [Header("Debug")]
    public bool debugLogs = true;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void HandleDashFinished(Vector3 start, Vector3 end)
    {
        if (upgradeLevel <= 0)
        {
            if (debugLogs) Debug.Log("[AngerDashUpgrade] upgradeLevel<=0, not spawning start burst.", this);
            return;
        }

        if (fireBurstPrefab == null)
        {
            Debug.LogWarning("[AngerDashUpgrade] fireBurstPrefab not assigned; no burst will spawn.", this);
            return;
        }

        float percent = (aoePercent != null && aoePercent.Length > upgradeLevel)
            ? aoePercent[upgradeLevel]
            : 0f;

        var burst = Spawn(fireBurstPrefab, start);
        if (burst != null)
        {
            burst.Configure(percent);
            if (debugLogs)
                Debug.Log($"[AngerDashUpgrade] Spawned START burst at {start} (level={upgradeLevel}, +%={percent})", burst);
        }
    }
}