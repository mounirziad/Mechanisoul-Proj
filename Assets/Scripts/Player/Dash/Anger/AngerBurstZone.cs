using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AngerBurstZone : MonoBehaviour
{
    [Header("Area")]
    public float radius = 2.0f;
    [Tooltip("Which layers count as enemies. Set to Enemy layer(s)")]
    public LayerMask enemyMask = ~0;
    [Tooltip("Include Trigger colliders in the overlap (recommended).")]
    public bool includeTriggers = true;

    [Header("Damage")]
    [Tooltip("Base damage dealt by the burst before scaling.")]
    public float baseBurstDamage = 20f;
    [Tooltip("Extra percent scaling (0.15 = +15%). Set via Configure().")]
    public float percentScale = 0.0f;

    [Header("Knockback")]
    [Tooltip("Horizontal impulse")]
    public float knockbackImpulse = 8f;
    [Tooltip("Upward component of impulse")]
    public float knockupImpulse = 2f;

    [Header("Lifecycle/Debug")]
    public float autoDestroyDelay = 0.05f;
    public bool debugLogs = true;

    public void Configure(float percentOfDamage)
    {
        percentScale = Mathf.Max(0f, percentOfDamage);
        if (debugLogs) Debug.Log($"[AngerBurstZone] Configure percentScale={percentScale}", this);
    }

    void OnEnable()
    {
        DoBurst();
        if (autoDestroyDelay >= 0f)
            Destroy(gameObject, autoDestroyDelay);
    }

    void DoBurst()
    {
        var qti = includeTriggers ? QueryTriggerInteraction.Collide : QueryTriggerInteraction.Ignore;
        Collider[] hits = Physics.OverlapSphere(transform.position, radius, enemyMask, qti);
        float dmg = baseBurstDamage * Mathf.Max(0.01f, 1f + percentScale);

        if (debugLogs) Debug.Log($"[AngerBurstZone] Burst @ {transform.position}  hits={hits.Length}  dmg={dmg:F1}", this);

        var processed = new HashSet<Transform>();

        for (int i = 0; i < hits.Length; i++)
        {
            var col = hits[i];
            var root = col.transform.root != null ? col.transform.root : col.transform;

            // avoid double-hitting a rig with many colliders
            if (processed.Contains(root)) continue;
            processed.Add(root);

            Vector3 dir = (root.position - transform.position);
            dir.y = 0f;
            if (dir.sqrMagnitude < 1e-6f) dir = Vector3.forward; else dir.Normalize();
            Vector3 impulse = dir * knockbackImpulse + Vector3.up * knockupImpulse;

            // DAMAGE
            var health = col.GetComponentInParent<BasicEnemyHealth>();
            if (health != null)
            {
                health.TakeDamage(dmg, dir);
            }
            else
            {
                if (root.CompareTag("Enemy") || col.CompareTag("Enemy"))
                    root.SendMessage("ApplyDamageFromAOE", dmg, SendMessageOptions.DontRequireReceiver);
            }

            // KNOCKBACK (3 paths)
            var rb = root.GetComponent<Rigidbody>();
            if (rb != null && rb.isKinematic == false)
            {
                rb.AddForce(impulse, ForceMode.Impulse);
            }
            else
            {
                var agent = root.GetComponent<NavMeshAgent>();
                if (agent != null && agent.isOnNavMesh)
                {
                    // quick shove; most agents will react
                    agent.velocity = dir * Mathf.Max(knockbackImpulse, 2f);
                }
                // project-specific fallback hook
                root.SendMessage("ApplyKnockback", impulse, SendMessageOptions.DontRequireReceiver);
            }

            if (debugLogs) Debug.Log($"[AngerBurstZone] -> {root.name}  impulse={impulse}", this);
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
