using UnityEngine;

public static class StatusEffectUtility
{
    public static void ApplyStatus(Component hit, StatusEffectType type, float duration)
    {
        if (hit == null) return;

        var tracker = hit.GetComponentInParent<EnemyStatusTracker>();
        if (tracker != null)
        {
            tracker.ApplyStatus(type, duration);
        }
    }
}