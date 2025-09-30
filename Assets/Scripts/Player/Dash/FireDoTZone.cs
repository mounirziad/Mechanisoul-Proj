using System.Collections;
using UnityEngine;

public class FireDoTZone : MonoBehaviour
{
    [Header("Lifetime")]
    public float lifetime = 4f;
    public float tickInterval = 0.5f;

    [Header("Area")]
    public float radius = 2.0f;
    [Tooltip("Which layers count as enemies. Set this to your Enemy layer.")]
    public LayerMask enemyMask = ~0;

    [Header("Damage")]
    [Tooltip("Base damage dealt each tick before aoePercent scaling.")]
    public float baseTickDamage = 5f;

    [Header("Physics Safety")]
    [Tooltip("If true, converts any colliders to triggers & makes RB kinematic so this zone never blocks the player.")]
    public bool forceNonBlocking = true;

    // Set by DashAbility.Configure
    float aoePercent = 0.1f; // 0.15 = 15%

    public void Configure(float percentOfDamage)
    {
        aoePercent = Mathf.Max(0f, percentOfDamage);
    }

    void OnEnable()
    {
        if (forceNonBlocking) MakeNonBlocking();
        StartCoroutine(DoTLoop());
        if (lifetime > 0f) Destroy(gameObject, lifetime + 0.05f);
    }

    void MakeNonBlocking()
    {
        // Make all colliders triggers (and convex if MeshCollider)
        var cols = GetComponentsInChildren<Collider>(true);
        foreach (var col in cols)
        {
            if (col is MeshCollider mc) mc.convex = true;
            col.isTrigger = true;
        }
        // Ensure any RB won't push things around
        var rb = GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;
    }

    IEnumerator DoTLoop()
    {
        var wait = new WaitForSeconds(tickInterval);
        while (true)
        {
            ApplyDamageTick();
            yield return wait;
        }
    }

    void ApplyDamageTick()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, radius, enemyMask, QueryTriggerInteraction.Ignore);
        float dmg = baseTickDamage * Mathf.Max(0.01f, 1f + aoePercent);

        for (int i = 0; i < hits.Length; i++)
        {
            var h = hits[i];
            var health = h.GetComponentInParent<BasicEnemyHealth>();
            if (health != null)
            {
                health.TakeDamage(dmg, (health.transform.position - transform.position).normalized);
                continue;
            }

            if (h.CompareTag("Enemy") || (h.transform.root != null && h.transform.root.CompareTag("Enemy")))
                h.SendMessage("ApplyDamageFromAOE", dmg, SendMessageOptions.DontRequireReceiver);
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.25f);
        Gizmos.DrawSphere(transform.position, radius);
    }
#endif
}
