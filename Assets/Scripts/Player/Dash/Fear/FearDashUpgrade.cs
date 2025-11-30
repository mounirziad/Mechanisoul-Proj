using UnityEngine;

public class FearDashUpgrade : DashUpgradeBase
{
    [Header("Fear Settings")]
    public float[] seconds = { 0f, 4f, 4f, 7f };   // lvl1 4s, lvl2 4s, lvl3 7s
    public float[] radius = { 0f, 2.5f, 3.5f, 3.5f }; // lvl2+ get bigger AOE

    [Header("Prefabs")]
    public FearZone fearZonePrefab;

    protected override void Awake()
    {
        base.Awake();
        SelectEmotion(Emotions.Fear);
    }

    protected override void HandleDashFinished(Vector3 start, Vector3 end)
    {
        if (upgradeLevel <= 0) return;
        if (!fearZonePrefab) return;

        var fz = Spawn(fearZonePrefab, start);
        if (fz != null)
            fz.Configure(seconds[upgradeLevel], radius[upgradeLevel], transform);
    }

}