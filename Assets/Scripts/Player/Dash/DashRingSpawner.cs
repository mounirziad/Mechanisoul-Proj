using UnityEngine;

[RequireComponent(typeof(DashAbility))]
public class DashRingSpawner : MonoBehaviour
{
    [Header("Ring Prefab")]
    [SerializeField] private RingIndicator ringPrefab;
    [SerializeField] private float ringLifetime = 1.2f;

    [Header("Spawn timing")]
    [Tooltip("Spawn at the beginning of the dash path (the 'start' position passed by DashAbility).")]
    [SerializeField] private bool spawnAtStart = true;
    [Tooltip("Spawn at the end of the dash path (the 'end' position passed by DashAbility).")]
    [SerializeField] private bool spawnAtEnd = false;

    [Header("Visual Radii (per emotion, per level)")]

    [SerializeField] private float[] joyRadii = { 2.5f, 3.0f, 3.5f };
    [SerializeField] private float[] loveRadii = { 2.0f, 2.5f, 3.0f };
    [SerializeField] private float[] fearRadii = { 2.0f, 2.5f, 3.0f };
    [SerializeField] private float[] angerRadii = { 2.0f, 2.5f, 3.0f };
    [SerializeField] private float[] sadnessRadii = { 2.0f, 2.5f, 3.0f };

    [Header("Colors")]
    [SerializeField] private Color joyColor = new Color(1f, .92f, .16f, 0.9f); // yellow
    [SerializeField] private Color angerColor = new Color(1f, .35f, .10f, 0.9f); // orange/red
    [SerializeField] private Color sadnessColor = new Color(.35f, .60f, 1f, 0.9f); // blue
    [SerializeField] private Color loveColor = new Color(1f, .40f, .80f, 0.9f); // pink
    [SerializeField] private Color fearColor = new Color(.60f, 0f, 1f, 0.9f);   // purple

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private DashAbility dash;
    private UpgradeHandler handler;

    private void Awake()
    {
        dash = GetComponent<DashAbility>();
        handler = GetComponent<UpgradeHandler>() ?? GetComponentInParent<UpgradeHandler>();

        if (debugLogs)
        {
            Debug.Log($"[DashRingSpawner] Awake. prefab={(ringPrefab ? ringPrefab.name : "NULL")}", this);
        }

        if (!dash)
        {
            Debug.LogError("[DashRingSpawner] No DashAbility found; disabling.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (dash != null)
        {
            dash.OnDashFinished += HandleDashFinished;
            if (debugLogs) Debug.Log("[DashRingSpawner] OnEnable: subscribed to OnDashFinished.", this);
        }
    }

    private void OnDisable()
    {
        if (dash != null)
        {
            dash.OnDashFinished -= HandleDashFinished;
            if (debugLogs) Debug.Log("[DashRingSpawner] OnDisable: unsubscribed from OnDashFinished.", this);
        }
    }

    private void HandleDashFinished(Vector3 start, Vector3 end)
    {
        if (!ringPrefab)
        {
            if (debugLogs) Debug.LogWarning("[DashRingSpawner] No ringPrefab assigned; skipping.", this);
            return;
        }

        // --- Determine the single active dash emotion from UpgradeHandler levels ---
        var (emotion, lvl) = ResolveActiveEmotionFromHandler();

        if (emotion == Emotion.None)
        {
            if (debugLogs)
            {
                Debug.Log($"[DashRingSpawner] No active dash emotion (Joy={Get(handler?.dashJoyLvl)}, Anger={Get(handler?.dashAngerLvl)}, " +
                          $"Sadness={Get(handler?.dashSadnessLvl)}, Love={Get(handler?.dashLoveLvl)}, Fear={Get(handler?.dashFearLvl)}).", this);
            }
            return;
        }

        // Clamp level to 0..2 for our radius arrays (adjust if you support more)
        int clampedLvl = Mathf.Clamp(lvl, 0, 2);

        float radius = GetRadius(emotion, clampedLvl);
        Color color = GetColor(emotion);

        if (spawnAtStart)
        {
            SpawnRing(start, radius, color, "START", emotion, clampedLvl);
        }

        if (spawnAtEnd)
        {
            SpawnRing(end, radius, color, "END", emotion, clampedLvl);
        }
    }

    private void SpawnRing(Vector3 pos, float radius, Color color, string where, Emotion e, int lvl)
    {
        var ring = Instantiate(ringPrefab, pos, Quaternion.identity);
        ring.Spawn(Mathf.Max(0.05f, radius), color, ringLifetime);

        if (debugLogs)
        {
            Debug.Log($"[DashRingSpawner] Spawned {where} ring for {e} (lvl {lvl}) at {pos} radius={radius} lifetime={ringLifetime}", this);
        }
    }

    private (Emotion, int) ResolveActiveEmotionFromHandler()
    {
        if (handler == null)
        {
            if (debugLogs) Debug.LogWarning("[DashRingSpawner] No UpgradeHandler found; cannot resolve active emotion.", this);
            return (Emotion.None, 0);
        }

        // Only one should be > 0. If multiple, we pick the first found in this order.
        if (handler.dashJoyLvl > 0) return (Emotion.Joy, handler.dashJoyLvl);
        if (handler.dashAngerLvl > 0) return (Emotion.Anger, handler.dashAngerLvl);
        if (handler.dashSadnessLvl > 0) return (Emotion.Sadness, handler.dashSadnessLvl);
        if (handler.dashLoveLvl > 0) return (Emotion.Love, handler.dashLoveLvl);
        if (handler.dashFearLvl > 0) return (Emotion.Fear, handler.dashFearLvl);

        return (Emotion.None, 0);
    }

    private float GetRadius(Emotion e, int lvl)
    {
        switch (e)
        {
            case Emotion.Joy: return GetFrom(joyRadii, lvl, 3f);
            case Emotion.Anger: return GetFrom(angerRadii, lvl, 2f);
            case Emotion.Sadness: return GetFrom(sadnessRadii, lvl, 2f);
            case Emotion.Love: return GetFrom(loveRadii, lvl, 2.5f);
            case Emotion.Fear: return GetFrom(fearRadii, lvl, 2.5f);
            default: return 2f;
        }
    }

    private Color GetColor(Emotion e)
    {
        switch (e)
        {
            case Emotion.Joy: return joyColor;
            case Emotion.Anger: return angerColor;
            case Emotion.Sadness: return sadnessColor;
            case Emotion.Love: return loveColor;
            case Emotion.Fear: return fearColor;
            default: return Color.white;
        }
    }

    private static float GetFrom(float[] arr, int index, float fallback)
    {
        if (arr != null && arr.Length > 0)
        {
            int i = Mathf.Clamp(index, 0, arr.Length - 1);
            return arr[i];
        }
        return fallback;
    }

    private static string Get(int? v) => v.HasValue ? v.Value.ToString() : "null";

    private enum Emotion { None, Joy, Anger, Sadness, Love, Fear }
}
