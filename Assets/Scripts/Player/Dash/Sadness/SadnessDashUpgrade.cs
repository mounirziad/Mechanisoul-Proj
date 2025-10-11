using UnityEngine;

public class SadnessDashUpgrade : DashUpgradeBase
{
    [Header("Sadness Settings")]
    [Range(0f,1f)] public float[] slowPercent = { 0f, 0.35f, 0.35f };
    public float[] slowSeconds = { 0f, 2.5f, 2.5f };
    public float dropEveryMeters = 0.65f;

    [Header("Prefabs")]
    public SlowZone slowZonePrefab;

    float accum;
    Vector3 _lastPos;

    protected override void Awake()
    {
        base.Awake();
        selectedEmotion = Emotions.Sadness;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        _lastPos = transform.position;
        accum = 0f;
    }

    protected override void HandleDashStep(Vector3 pos)
    {
        if (upgradeLevel <= 0) { _lastPos = pos; return; }
        float step = Vector3.Distance(pos, _lastPos);
        _lastPos = pos;
        accum += step;
        if (accum >= dropEveryMeters)
        {
            accum = 0f;
            var z = Spawn(slowZonePrefab, pos);
            if (z != null) z.Configure(slowPercent[upgradeLevel], slowSeconds[upgradeLevel]);
        }
    }
}