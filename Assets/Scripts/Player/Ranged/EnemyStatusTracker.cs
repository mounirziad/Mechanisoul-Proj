using System.Collections.Generic;
using UnityEngine;

public class EnemyStatusTracker : MonoBehaviour
{
    class StatusEntry
    {
        public float remaining;
    }

    [Tooltip("If not set, will search in children.")]
    public EnemyStatusIconLabel iconLabel;

    Dictionary<StatusEffectType, StatusEntry> active =
        new Dictionary<StatusEffectType, StatusEntry>();

    List<StatusEffectType> removalBuffer = new List<StatusEffectType>();
    List<StatusEffectType> orderedBuffer = new List<StatusEffectType>();

    void Awake()
    {
        if (iconLabel == null)
        {
            iconLabel = GetComponentInChildren<EnemyStatusIconLabel>();
        }
        RefreshIcons();
    }

    void Update()
    {
        if (active.Count == 0) return;

        float dt = Time.deltaTime;
        removalBuffer.Clear();

        foreach (var kvp in active)
        {
            kvp.Value.remaining -= dt;
            if (kvp.Value.remaining <= 0f)
            {
                removalBuffer.Add(kvp.Key);
            }
        }

        if (removalBuffer.Count > 0)
        {
            for (int i = 0; i < removalBuffer.Count; i++)
                active.Remove(removalBuffer[i]);

            RefreshIcons();
        }
    }

    public void ApplyStatus(StatusEffectType type, float duration)
    {
        if (duration <= 0f) duration = 0.01f;

        StatusEntry entry;
        if (!active.TryGetValue(type, out entry))
        {
            entry = new StatusEntry();
            active[type] = entry;
        }

        entry.remaining = Mathf.Max(entry.remaining, duration);

        RefreshIcons();
    }

    public bool HasStatus(StatusEffectType type)
    {
        return active.ContainsKey(type);
    }

    void RefreshIcons()
    {
        if (iconLabel == null)
            return;

        if (active.Count == 0)
        {
            iconLabel.ShowEffects(null);
            return;
        }

        orderedBuffer.Clear();

        // Fear, Love, Sadness
        if (active.ContainsKey(StatusEffectType.Fear))
            orderedBuffer.Add(StatusEffectType.Fear);
        if (active.ContainsKey(StatusEffectType.Love))
            orderedBuffer.Add(StatusEffectType.Love);
        if (active.ContainsKey(StatusEffectType.Sadness))
            orderedBuffer.Add(StatusEffectType.Sadness);

        iconLabel.ShowEffects(orderedBuffer);
    }
}
