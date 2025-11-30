using UnityEngine;

public class SadnessDashUpgrade : DashUpgradeBase
{
    [Header("Sadness (single zone @ START if legacy toggle is ON)")]
    [SerializeField] private SlowZone slowZonePrefab;
    public float[] slowPercent = { 0f, 0.25f, 0.35f, 0.35f }; // lvl3 same strength as lvl2
    public float[] slowSeconds = { 1.5f, 2.0f, 2.0f, 2.0f };

    [Header("Area of Effect (radius per level)")]
    [SerializeField] private float[] slowRadius = { 0f, 2.0f, 2.0f, 3.5f };
    // index 0 = off, 1 = base, 2 = lv1, 3 = lv2

    [Header("Debug")]
    public bool debugLogs = true;

    protected override void HandleDashFinished(Vector3 start, Vector3 end)
    {
        if (!autoSubscribeToDash) return;
        if (upgradeLevel <= 0) return;
        if (!slowZonePrefab) return;

        int lvl = Mathf.Clamp(upgradeLevel, 0, Mathf.Max(slowPercent.Length - 1, 0));
        float pct = slowPercent[Mathf.Clamp(lvl, 0, slowPercent.Length - 1)];
        float sec = slowSeconds[Mathf.Clamp(lvl, 0, slowSeconds.Length - 1)];

        // choose radius based on level, defaulting to prefab radius
        float radius = slowZonePrefab.radius;
        if (slowRadius != null && slowRadius.Length > 0)
        {
            radius = slowRadius[Mathf.Clamp(lvl, 0, slowRadius.Length - 1)];
        }

        var z = Spawn(slowZonePrefab, start);
        if (z != null)
        {
            z.Configure(pct, sec);
            z.radius = radius; // apply per level aoe

            if (debugLogs)
            {
                Debug.Log(
                    $"[SadnessDashUpgrade] (Legacy) SlowZone @ {start} | slow={pct:P0} | sec={sec:F1} | radius={radius:F2} | level={upgradeLevel}",
                    z
                );
            }
        }
    }
}
