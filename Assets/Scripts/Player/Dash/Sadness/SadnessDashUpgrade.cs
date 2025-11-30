using UnityEngine;

public class SadnessDashUpgrade : DashUpgradeBase
{
    [Header("Sadness (single zone @ START if legacy toggle is ON)")]
    [SerializeField] private SlowZone slowZonePrefab;
    public float[] slowPercent = { 0f, 0.25f, 0.35f, 0.35f }; // lvl3 same strength as lvl2
    public float[] slowSeconds = { 1.5f, 2.0f, 2.0f, 2.0f };

    [Header("Debug")]
    public bool debugLogs = true;
    protected override void HandleDashFinished(Vector3 start, Vector3 end)
    {
        if (!autoSubscribeToDash) return;              // centralized spawner path = OFF
        if (upgradeLevel <= 0) return;
        if (!slowZonePrefab) return;

        int lvl = Mathf.Clamp(upgradeLevel, 0, Mathf.Max(slowPercent.Length - 1, 0));
        float pct = slowPercent[Mathf.Clamp(lvl, 0, slowPercent.Length - 1)];
        float sec = slowSeconds[Mathf.Clamp(lvl, 0, slowSeconds.Length - 1)];

        var z = Spawn(slowZonePrefab, start);
        if (z != null)
        {
            z.Configure(pct, sec);
            if (debugLogs)
                Debug.Log($"[SadnessDashUpgrade] (Legacy) SlowZone @ {start} | slow={pct:P0} | sec={sec:F1} | level={upgradeLevel}", z);
        }
    }
}
