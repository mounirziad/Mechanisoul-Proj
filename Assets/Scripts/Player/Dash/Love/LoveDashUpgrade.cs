using UnityEngine;

public class LoveDashUpgrade : DashUpgradeBase
{
    [Header("Love Settings")]
    public float[] weaknessPercent = { 0f, 0.15f, 0.30f, 0.30f }; // lvl3 keeps 30% but lasts longer
    public float[] duration = { 0f, 4f, 4f, 7f };
    public float[] radius = { 0f, 2.5f, 2.5f, 2.5f }; // AOE size constant

    [Header("Prefabs")]
    public WeaknessZone weaknessZonePrefab;

    protected override void Awake()
    {
        base.Awake();
        SelectEmotion(Emotions.Love);
    }

    protected override void HandleDashFinished(Vector3 start, Vector3 end)
    {
        if (upgradeLevel <= 0) return;
        if (!weaknessZonePrefab) return;

        var wz = Spawn(weaknessZonePrefab, start);
        if (wz != null)
            wz.Configure(weaknessPercent[upgradeLevel], duration[upgradeLevel], radius[upgradeLevel]);
    }

}