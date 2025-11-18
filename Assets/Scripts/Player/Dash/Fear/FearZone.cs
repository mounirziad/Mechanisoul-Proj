using UnityEngine;

public class FearZone : MonoBehaviour
{
    [Header("Lifetime")]
    public float lifetime = 5f;

    [Header("Area")]
    public float radius = 2.5f;
    public LayerMask enemyMask = ~0;

    [Header("Effect")]
    public float fearSeconds = 4f;
    Transform fearSource; // typically the player

    public bool forceNonBlocking = true;

    public void Configure(float seconds, float r, Transform source)
    {
        fearSeconds = Mathf.Max(0f, seconds);
        radius = Mathf.Max(0.1f, r);
        fearSource = source;
    }

    void OnEnable()
    {
        if (forceNonBlocking) MakeNonBlocking();
        ApplyFearOnce();
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

    void ApplyFearOnce()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius, enemyMask, QueryTriggerInteraction.Ignore);
        object[] payload = new object[] { fearSeconds, (fearSource ? fearSource.position : transform.position) };
        for (int i = 0; i < hits.Length; i++)
        {
            hits[i].SendMessage("AddFear", payload, SendMessageOptions.DontRequireReceiver);

            // also show Fear icon for the same duration
            StatusEffectUtility.ApplyStatus(hits[i], StatusEffectType.Fear, fearSeconds);
        }
    }


#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.6f, 0.4f, 1f, 0.25f);
        Gizmos.DrawSphere(transform.position, radius);
    }
#endif
}
