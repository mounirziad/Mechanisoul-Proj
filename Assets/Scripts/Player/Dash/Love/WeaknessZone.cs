using System.Collections;
using UnityEngine;

public class WeaknessZone : MonoBehaviour
{
    [Header("Lifetime")]
    public float lifetime = 5f;

    [Header("Area")]
    public float radius = 2.5f;
    public LayerMask enemyMask = ~0;

    [Header("Effect")]
    [Range(0, 1)] public float weaknessPercent = 0.15f; // reduce outgoing damage by this %
    public float seconds = 4f;

    public bool forceNonBlocking = true;

    public void Configure(float pct, float dur, float r)
    {
        weaknessPercent = Mathf.Clamp01(pct);
        seconds = Mathf.Max(0f, dur);
        radius = Mathf.Max(0.1f, r);
    }

    void OnEnable()
    {
        if (forceNonBlocking) MakeNonBlocking();
        ApplyWeaknessOnce();
        if (lifetime > 0f) Destroy(gameObject, lifetime);
    }

    void MakeNonBlocking()
    {
        foreach (var col in GetComponentsInChildren<Collider>(true))
        {
            if (col is MeshCollider mc) mc.convex = true;
            col.isTrigger = true;
        }
        var rb = GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;
    }

    void ApplyWeaknessOnce()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius, enemyMask, QueryTriggerInteraction.Ignore);
        object[] payload = new object[] { weaknessPercent, seconds };
        for (int i = 0; i < hits.Length; i++)
        {
            hits[i].SendMessage("AddWeakness", payload, SendMessageOptions.DontRequireReceiver);

            // Sadness icon for "weakened" enemies
            StatusEffectUtility.ApplyStatus(hits[i], StatusEffectType.Sadness, seconds);
        }
    }


#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0.8f, 0.25f);
        Gizmos.DrawSphere(transform.position, radius);
    }
#endif
}
