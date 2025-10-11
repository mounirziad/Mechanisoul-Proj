using UnityEngine;

public class JoyCritZone : MonoBehaviour
{
    [Header("One-shot burst")]
    public float radius = 3f;
    [Range(0f, 1f)] public float critChance = 0.4f;
    public float critDamage = 35f;
    public LayerMask enemyMask = ~0;

    public void Configure(float chance, float r, float dmg)
    {
        critChance = Mathf.Clamp01(chance);
        radius = Mathf.Max(0.1f, r);
        critDamage = Mathf.Max(0f, dmg);
    }

    void OnEnable()
    {
        // do an immediate overlap then self-destroy
        Collider[] hits = Physics.OverlapSphere(transform.position, radius, enemyMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hits.Length; i++)
        {
            if (Random.value <= critChance)
            {
                var health = hits[i].GetComponentInParent<BasicEnemyHealth>();
                if (health != null)
                {
                    health.TakeDamage(critDamage, (health.transform.position - transform.position).normalized);
                    continue;
                }
                hits[i].SendMessage("ApplyDamageFromAOE", critDamage, SendMessageOptions.DontRequireReceiver);
            }
        }
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.9f, 0.2f, 0.25f);
        Gizmos.DrawSphere(transform.position, radius);
    }
#endif
}
