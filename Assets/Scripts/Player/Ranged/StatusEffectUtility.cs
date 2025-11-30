using UnityEngine;

public static class StatusEffectUtility
{
    public static void ApplyStatus(Component hit, StatusEffectType type, float duration)
    {
        if (hit == null) return;

        // First look up the hierarchy
        var tracker = hit.GetComponentInParent<EnemyStatusTracker>();

        // Then look down, in case the tracker was put on a child
        if (tracker == null)
            tracker = hit.GetComponentInChildren<EnemyStatusTracker>();

        if (tracker != null)
        {
            tracker.ApplyStatus(type, duration);
        }
        else
        {
            Debug.LogWarning(
                $"[StatusEffectUtility] No EnemyStatusTracker found for {hit.name}",
                hit
            );
        }
    }
}
