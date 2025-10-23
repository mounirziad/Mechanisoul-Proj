using UnityEngine;

public class FearDashUpgrade : DashUpgradeBase
{
    [Header("Fear Settings")]
    public float[] seconds = { 0f, 4f, 7f };
    public float[] radius = { 0f, 2.5f, 3.0f };

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
        var fz = Spawn(fearZonePrefab, end);
        if (fz != null) fz.Configure(seconds[upgradeLevel], radius[upgradeLevel], transform);
    }
}