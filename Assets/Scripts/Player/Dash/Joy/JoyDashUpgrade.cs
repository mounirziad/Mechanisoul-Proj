using UnityEngine;

[DisallowMultipleComponent]
public class JoyDashUpgrade : MonoBehaviour
{
    [Header("Prefab")]
    public JoyCritZone joyCritPrefab;

    [Header("Per-Level Tuning (index 0 = off)")]
    [Tooltip("Extra crit chance granted while inside the aura (0..1). Lev1/Lev2 defaults 0.4, 0.6")]
    public float[] chance = new float[3] { 0f, 0.40f, 0.60f };

    [Tooltip("Aura radius per level")]
    public float[] radius = new float[3] { 0f, 3.0f, 3.0f };

    [Tooltip("Aura lifetime per level (seconds)")]
    public float[] duration = new float[3] { 0f, 2.0f, 2.0f };

    [Header("Spawn Options")]
    [Tooltip("If true, spawn at dash end point; otherwise at current player position.")]
    public bool spawnAtDashEnd = true;
    [Tooltip("Small Y offset so the zone sits above ground slightly.")]
    public float yOffset = 0.02f;

    int _level = 0;
    DashAbility _dash;

    // called by UpgradeHandler
    public void SetLevel(int level)
    {
        _level = Mathf.Clamp(level, 0, 2);
        // enable only if active (UpgradeHandler already toggles component.enabled but used as backup)
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

        // resolve values
        float ch = chance[Mathf.Clamp(_level, 0, chance.Length - 1)];
        float rad = radius[Mathf.Clamp(_level, 0, radius.Length - 1)];
        float life = duration[Mathf.Clamp(_level, 0, duration.Length - 1)];

        // spawn position
        Vector3 pos = spawnAtDashEnd ? endPos : transform.position;
        pos.y += yOffset;

        // spawn and configure aura
        var zone = Instantiate(joyCritPrefab, pos, Quaternion.identity);
        zone.Configure(ch, rad, life);
    }
}
