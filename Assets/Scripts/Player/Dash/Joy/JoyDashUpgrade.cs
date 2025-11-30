using UnityEngine;

[DisallowMultipleComponent]
public class JoyDashUpgrade : MonoBehaviour
{
    [Header("Prefab")]
    public JoyCritZone joyCritPrefab;

    [Header("Per-Level Tuning (index 0 = off)")]
    public float[] chance = { 0f, 0.40f, 0.40f, 0.60f };
    public float[] radius = { 0f, 2.5f, 3.5f, 3.5f }; // lvl2+ bigger AOE
    public float[] duration = { 0f, 2.0f, 2.0f, 2.0f }; // same lifetime at all active levels


    [Header("Spawn Options")]
    [Tooltip("If true, spawn at dash end point; otherwise at current player position.")]
    public bool spawnAtDashEnd = true;
    [Tooltip("Small Y offset so the zone sits above ground slightly.")]
    public float yOffset = 0.02f;

    int _level = 0;
    DashAbility _dash;

    public void SetLevel(int level)
    {
        _level = Mathf.Clamp(level, 0, 3); // allow 3
        enabled = _level > 0;
    }

    void Awake()
    {
        _dash = GetComponent<DashAbility>();
        if (!_dash)
        {
            Debug.LogWarning("[JoyDashUpgrade] DashAbility not found on the same GameObject.");
            enabled = false;
        }
    }

    void OnEnable()
    {
        if (_dash != null) _dash.OnDashFinished += HandleDashFinished;
    }

    void OnDisable()
    {
        if (_dash != null) _dash.OnDashFinished -= HandleDashFinished;
    }

    void HandleDashFinished(Vector3 startPos, Vector3 endPos)
    {
        if (_level <= 0 || !joyCritPrefab) return;

        float ch = chance[Mathf.Clamp(_level, 0, chance.Length - 1)];
        float rad = radius[Mathf.Clamp(_level, 0, radius.Length - 1)];
        float life = duration[Mathf.Clamp(_level, 0, duration.Length - 1)];

        Vector3 pos = startPos;
        pos.y += yOffset;

        var zone = Instantiate(joyCritPrefab, pos, Quaternion.identity);
        zone.Configure(ch, rad, life);
    }

}
