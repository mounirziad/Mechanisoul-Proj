using UnityEngine;
using System.Reflection;
using System;

[RequireComponent(typeof(DashAbility))]
public class DashRingSpawner : MonoBehaviour
{
    [Header("Ring")]
    [SerializeField] RingIndicator ringPrefab;
    [SerializeField] float ringLifetime = 1.2f;
    [SerializeField] bool spawnRingAtStart = true;   // we use START now
    [SerializeField] bool spawnEndRing = false;      // disable end ring

    [Header("Colors")]
    [SerializeField] Color angerColor = new Color(1f, .35f, .1f, 0.9f);

    [Header("Anger Burst (START)")]
    [Tooltip("Spawn an Anger burst at dash START if Anger level > 0.")]
    public bool spawnAngerBurstAtStart = true;
    public AngerBurstZone angerBurstPrefab;          // optional; if null we create at runtime
    [Tooltip("Fallback radius if your Anger data is unavailable.")]
    public float angerBurstFallbackRadius = 2.0f;
    [Tooltip("Fallback base damage if your Anger data is unavailable.")]
    public float angerBurstFallbackDamage = 20f;
    [Tooltip("Fallback knockback impulse if your Anger data is unavailable.")]
    public float angerBurstFallbackImpulse = 8f;
    public float angerBurstFallbackKnockup = 2f;

    [Header("Diagnostics")]
    public bool debugLogs = true;
    [Tooltip("If true, an upgrade with upgradeLevel>0 will be considered active even if the component is disabled.")]
    public bool considerLevelEvenIfDisabled = true;

    DashAbility dash;
    AngerDashUpgrade anger; // we only care about anger for this feature

    void Awake()
    {
        dash = GetComponent<DashAbility>();
        if (!dash)
        {
            if (debugLogs) Debug.LogError("[DashRingSpawner] No DashAbility found; disabling.", this);
            enabled = false;
            return;
        }
        RefreshUpgradeRefs();
        if (debugLogs) Debug.Log($"[DashRingSpawner] Awake. ring={(ringPrefab ? ringPrefab.name : "NULL")}", this);
    }

    void OnEnable()
    {
        dash.OnDashFinished += HandleDashFinished;
        if (debugLogs) Debug.Log("[DashRingSpawner] OnEnable: subscribed to OnDashFinished.", this);
    }

    void OnDisable()
    {
        dash.OnDashFinished -= HandleDashFinished;
        if (debugLogs) Debug.Log("[DashRingSpawner] OnDisable: unsubscribed from OnDashFinished.", this);
    }

    void HandleDashFinished(Vector3 start, Vector3 end)
    {
        if (debugLogs) Debug.Log($"[DashRingSpawner] OnDashFinished. start={start} end={end}", this);

        // pickups may add/modify upgrades at runtime
        RefreshUpgradeRefs();

        // Spawn START burst for Anger
        if (spawnAngerBurstAtStart && IsUpgradeActive(anger))
        {
            float pct = GetPercentScale(anger); // read aoePercent[level] if present
            if (angerBurstPrefab != null)
            {
                var burst = Instantiate(angerBurstPrefab, start, Quaternion.identity);
                burst.Configure(pct);
                if (debugLogs) Debug.Log($"[DashRingSpawner] START Anger Burst (prefab) @ {start} +%={pct}", burst);
            }
            else
            {
                // No prefab? Create an ad-hoc burst so your damage/KB still happen.
                var go = new GameObject("AngerBurstZone (AdHoc)");
                go.transform.position = start;
                var burst = go.AddComponent<AngerBurstZone>();
                burst.radius = angerBurstFallbackRadius;
                burst.baseBurstDamage = angerBurstFallbackDamage;
                burst.knockbackImpulse = angerBurstFallbackImpulse;
                burst.knockupImpulse = angerBurstFallbackKnockup;
                burst.includeTriggers = true;
                burst.debugLogs = true; // make it loud so we see hits
                burst.Configure(pct);
                if (debugLogs) Debug.Log($"[DashRingSpawner] START Anger Burst (adhoc) @ {start} +%={pct}", burst);
            }
        }

        //Spawn ring AT START (not end)
        if (spawnRingAtStart)
            SpawnRingAt(start, angerColor);   // red/orange ring at the start
        else if (spawnEndRing)
            SpawnRingAt(end, angerColor);     // end ring if you turn this on
    }

    void SpawnRingAt(Vector3 pos, Color color)
    {
        if (!ringPrefab)
        {
            if (debugLogs) Debug.LogWarning("[DashRingSpawner] ringPrefab is NULL; no ring spawned.", this);
            return;
        }
        var ring = Instantiate(ringPrefab, pos, Quaternion.identity);
        ring.Spawn(2f, color, ringLifetime); // radius here is just visual; tweak if desired
        if (debugLogs) Debug.Log($"[DashRingSpawner] Spawned ring at {pos}", this);
    }

    // ---------- helpers ----------
    void RefreshUpgradeRefs()
    {
        // Try local children parents
        if (!anger) anger = GetComponent<AngerDashUpgrade>();
        if (!anger) anger = GetComponentInChildren<AngerDashUpgrade>(true);
        if (!anger) anger = GetComponentInParent<AngerDashUpgrade>();
    }

    bool IsUpgradeActive(MonoBehaviour mb)
    {
        if (!mb) return false;
        if (mb.isActiveAndEnabled) return true;
        if (!considerLevelEvenIfDisabled) return false;
        return GetLevel(mb) > 0;
    }

    int GetLevel(MonoBehaviour mb)
    {
        if (!mb) return 0;
        var t = mb.GetType();
        var f = t.GetField("upgradeLevel", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (f != null && f.FieldType == typeof(int)) { try { return (int)f.GetValue(mb); } catch { } }
        var pUL = t.GetProperty("upgradeLevel", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (pUL != null && pUL.PropertyType == typeof(int)) { try { return (int)pUL.GetValue(mb); } catch { } }
        var p = t.GetProperty("Level", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (p != null && p.PropertyType == typeof(int)) { try { return (int)p.GetValue(mb); } catch { } }
        return 0;
    }

    // try read float[] aoePercent[level] if it exists; else 0
    float GetPercentScale(MonoBehaviour mb)
    {
        if (!mb) return 0f;
        var t = mb.GetType();
        var f = t.GetField("aoePercent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        int lvl = GetLevel(mb);
        if (f != null && typeof(float[]).IsAssignableFrom(f.FieldType))
        {
            var arr = f.GetValue(mb) as float[];
            if (arr != null && lvl >= 0 && lvl < arr.Length) return arr[lvl];
        }
        return 0f;
    }
}
