using UnityEngine;

public class JoyDashUpgrade : DashUpgradeBase
{
    [Header("Joy Settings by Level (index: 0..2)")]
    [Range(0f,1f)] public float[] critChance = { 0f, 0.40f, 0.60f };
    public float[] radius = { 0f, 3.0f, 3.0f };
    public float[] critDamage = { 0f, 35f, 35f };

    [Header("Prefabs")]
    public JoyCritZone joyCritPrefab;

    protected override void Awake()
    {
        base.Awake();
        selectedEmotion = Emotions.Joy;
    }

    protected override void HandleDashFinished(Vector3 start, Vector3 end)
    {
        if (upgradeLevel <= 0) return;
        var zone = Spawn(joyCritPrefab, end);
        if (zone != null) zone.Configure(critChance[upgradeLevel], radius[upgradeLevel], critDamage[upgradeLevel]);
    }
}