using UnityEngine;

public class EnemyMeleeDamage : MonoBehaviour
{
    [Header("Melee Settings")]
    public LayerMask playerLayer; 

    private AiAgent agent;

    private void Start()
    {
        agent = GetComponent<AiAgent>();
    }

    public void DealMeleeDamage()
    {
        if (agent == null || agent.config == null)
            return;

        Transform target = agent.GetCurrentTarget();
        if (target == null)
            return;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget <= agent.config.meleeAttackRange)
        {
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            float dotProduct = Vector3.Dot(transform.forward, directionToTarget);

            if (dotProduct > 0.5f)
            {
                // try player first
                PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(agent.config.meleeDamage);
                    return;
                }

                // if not player, try enemy
                BasicEnemyHealth enemyHealth = target.GetComponent<BasicEnemyHealth>();
                if (enemyHealth != null)
                {
                    Vector3 knockDir = (enemyHealth.transform.position - transform.position).normalized;
                    enemyHealth.TakeDamage(agent.config.meleeDamage, knockDir);
                }
            }
        }
    }
}
