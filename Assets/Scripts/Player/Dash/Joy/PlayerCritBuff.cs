using System.Collections.Generic;
using UnityEngine;

public class PlayerCritBuff : MonoBehaviour
{
    readonly Dictionary<object, float> sources = new Dictionary<object, float>();

    // Total extra crit chance (0..1)
    // Combine with base melee crit
    public float TotalBonusCritChance
    {
        get
        {
            float sum = 0f;
            foreach (var kv in sources) sum += kv.Value;
            return Mathf.Clamp01(sum);
        }
    }

    public void AddOrUpdate(object key, float bonusChance)
    {
        if (key == null) return;
        sources[key] = Mathf.Max(0f, bonusChance);
    }

    public void Remove(object key)
    {
        if (key == null) return;
        sources.Remove(key);
    }
}
