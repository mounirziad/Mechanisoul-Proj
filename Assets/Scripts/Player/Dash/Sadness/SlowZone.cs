using System.Collections;
using UnityEngine;

public class SlowZone : MonoBehaviour
{
    [Header("Lifetime")]
    public float lifetime = 4f;
    public float tickInterval = 0.25f;

    [Header("Area")]
    public float radius = 2.0f;
    [Tooltip("Which layers count as enemies. Set this to your Enemy layer.")]
    public LayerMask enemyMask = ~0;

    [Header("Physics Safety")]
    [Tooltip("If true, converts any colliders to triggers & makes RB kinematic so this zone never blocks the player.")]
    public bool forceNonBlocking = true;

    // Set by DashAbility.Configure
    float slowPercent = 0.3f;   // 0.3 = 30% slow
    float slowSeconds = 2f;

    public void Configure(float percent, float seconds)
    {
        slowPercent = Mathf.Clamp01(percent);
        slowSeconds = Mathf.Max(0f, seconds);
    }

    void OnEnable()
    {
        if (forceNonBlocking) MakeNonBlocking();
        StartCoroutine(SlowLoop());
        if (lifetime > 0f) Destroy(gameObject, lifetime + 0.05f);
    }

    void MakeNonBlocking()
    {
        var cols = GetComponentsInChildren<Collider>(true);
        foreach (var col in cols)
        {
            if (col is MeshCollider mc) mc.convex = true;
            col.isTrigger = true;
        }
        var rb = GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;
    }

    IEnumerator SlowLoop()
    {
        var wait = new WaitForSeconds(tickInterval);
        while (true)
        {
            ApplySlow();
            yield return wait;
        }
    }

    void ApplySlow()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius, enemyMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hits.Length; i++)
        {
            var t = hits[i].transform;

            var slowable = t.GetComponentInParent<ISlowable>();
            if (slowable != null)
            {
                slowable.AddSlow(slowPercent, slowSeconds);
                continue;
            }

            if (t.CompareTag("Enemy") || (t.root != null && t.root.CompareTag("Enemy")))
                t.SendMessage("AddSlow", new object[] { slowPercent, slowSeconds }, SendMessageOptions.DontRequireReceiver);

        }

    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.3f, 0.5f, 1f, 0.25f);
        Gizmos.DrawSphere(transform.position, radius);
    }
#endif
}
