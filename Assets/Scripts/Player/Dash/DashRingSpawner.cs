using UnityEngine;
using System;

/// <summary>
/// Spawns a colored ring at the player's feet when a dash finishes.
/// Determines the active dash emotion by checking which dash upgrade component is enabled.
/// For Anger/Sadness (whose radius comes from zone prefabs), provide fallback radii.
/// </summary>
[RequireComponent(typeof(DashAbility))]
public class DashRingSpawner : MonoBehaviour
{
    [Header("Ring Prefab")]
    [SerializeField] RingIndicator ringPrefab;
    [SerializeField] float ringLifetime = 1.2f;

    [Header("Fallback Radii (used for Anger/Sadness)")]
    [SerializeField] float angerRadius = 2.0f;
    [SerializeField] float sadnessRadius = 2.0f;

    [Header("Colors")]
    [SerializeField] Color joyColor = new Color(1f, .92f, .16f, 0.9f);      // yellow
    [SerializeField] Color angerColor = new Color(1f, .35f, .1f, 0.9f);     // orange/red
    [SerializeField] Color sadnessColor = new Color(.35f, .6f, 1f, 0.9f);   // blue
    [SerializeField] Color loveColor = new Color(1f, .4f, .8f, 0.9f);       // pink
    [SerializeField] Color fearColor = new Color(.6f, 0f, 1f, 0.9f);        // purple

    DashAbility dash;

    // Cached refs (optional)
    JoyDashUpgrade joy;
    AngerDashUpgrade anger;
    SadnessDashUpgrade sadness;
    LoveDashUpgrade love;
    FearDashUpgrade fear;

    void Awake()
    {
        dash = GetComponent<DashAbility>();
        if (!dash) { enabled = false; return; }

        // Try cache (components may be auto-added at runtime by your router/handler)
        joy = GetComponent<JoyDashUpgrade>();
        anger = GetComponent<AngerDashUpgrade>();
        sadness = GetComponent<SadnessDashUpgrade>();
        love = GetComponent<LoveDashUpgrade>();
        fear = GetComponent<FearDashUpgrade>();
    }

    void OnEnable()
    {
        dash.OnDashFinished += HandleDashFinished;
    }

    void OnDisable()
    {
        dash.OnDashFinished -= HandleDashFinished;
    }

    void HandleDashFinished(Vector3 start, Vector3 end)
    {
        // Components might be added later; refresh refs if needed
        if (!joy) joy = GetComponent<JoyDashUpgrade>();
        if (!anger) anger = GetComponent<AngerDashUpgrade>();
        if (!sadness) sadness = GetComponent<SadnessDashUpgrade>();
        if (!love) love = GetComponent<LoveDashUpgrade>();
        if (!fear) fear = GetComponent<FearDashUpgrade>();

        // Determine which upgrade is active
        if (joy && joy.isActiveAndEnabled)     SpawnRing(GetJoyRadius(joy), joyColor);
        else if (anger && anger.isActiveAndEnabled)   SpawnRing(GetAngerRadius(), angerColor);
        else if (sadness && sadness.isActiveAndEnabled) SpawnRing(GetSadnessRadius(), sadnessColor);
        else if (love && love.isActiveAndEnabled)     SpawnRing(GetLoveRadius(love), loveColor);
        else if (fear && fear.isActiveAndEnabled)     SpawnRing(GetFearRadius(fear), fearColor);
    }

    void SpawnRing(float radius, Color color)
    {
        if (!ringPrefab) return;
        var ring = Instantiate(ringPrefab, transform.position, Quaternion.identity);
        ring.Spawn(radius, color, ringLifetime);
    }

    // --- Radius helpers ---
    float GetJoyRadius(JoyDashUpgrade j)
    {
        // if script exposes Level, prefer it; otherwise all levels use same radius in our defaults
        try {
            var levelProp = j.GetType().GetProperty("Level");
            if (levelProp != null) {
                int lvl = Mathf.Clamp((int)levelProp.GetValue(j), 0, j.radius.Length - 1);
                return j.radius[Mathf.Max(0, lvl)];
            }
        } catch {}
        return (j.radius != null && j.radius.Length > 0) ? j.radius[j.radius.Length - 1] : 3f;
    }

    float GetLoveRadius(LoveDashUpgrade l)
    {
        try {
            var levelProp = l.GetType().GetProperty("Level");
            if (levelProp != null) {
                int lvl = Mathf.Clamp((int)levelProp.GetValue(l), 0, l.radius.Length - 1);
                return l.radius[Mathf.Max(0, lvl)];
            }
        } catch {}
        return (l.radius != null && l.radius.Length > 0) ? l.radius[l.radius.Length - 1] : 2.5f;
    }

    float GetFearRadius(FearDashUpgrade f)
    {
        try {
            var levelProp = f.GetType().GetProperty("Level");
            if (levelProp != null) {
                int lvl = Mathf.Clamp((int)levelProp.GetValue(f), 0, f.radius.Length - 1);
                return f.radius[Mathf.Max(0, lvl)];
            }
        } catch {}
        return (f.radius != null && f.radius.Length > 0) ? f.radius[f.radius.Length - 1] : 2.5f;
    }

    float GetAngerRadius()
    {
        // Uses DoT zone radius; fallback if unknown
        return Mathf.Max(0.1f, angerRadius);
    }

    float GetSadnessRadius()
    {
        return Mathf.Max(0.1f, sadnessRadius);
    }
}
