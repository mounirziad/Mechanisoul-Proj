using UnityEngine;

public class AngerDashUpgrade : DashUpgradeBase
{
    [Header("Anger Settings")]
    [Tooltip("AOE % of player damage per tick")] public float[] aoePercent = { 0f, 0.10f, 0.15f };
    [Tooltip("How many burn nodes to drop between start and end")] public int[] nodes = { 0, 1, 2 };

    [Header("Prefabs")]
    public AngerDoTZone fireDoTPrefab;
    public float nodeEdgePadding = 0.15f;

    protected override void Awake()
    {
        base.Awake();
        SelectEmotion(Emotions.Anger);
    }

    protected override void HandleDashFinished(Vector3 start, Vector3 end)
    {
        if (upgradeLevel <= 0) return;
        int n = Mathf.Max(1, nodes[upgradeLevel]);
        float pad = Mathf.Clamp01(nodeEdgePadding);
        for (int i = 1; i <= n; i++)
        {
            float t = Mathf.Lerp(pad, 1f - pad, i / (n + 1f));
            Vector3 p = Vector3.Lerp(start, end, t);
            var dot = Spawn(fireDoTPrefab, p);
            if (dot != null) dot.Configure(aoePercent[upgradeLevel]);
        }
    }
}