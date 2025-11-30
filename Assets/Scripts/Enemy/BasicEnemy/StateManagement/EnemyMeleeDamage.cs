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

    private void OnMeleeDamage()
    {
        if (agent == null || agent.config == null)
            return;

        // pick target: charmed enemy if applicable, otherwise player
        Transform target = null;

        EnemyCharm charm = GetComponent<EnemyCharm>();
        if (charm != null && charm.ShouldIgnorePlayerAndFightEnemies())
        {
            // your helper that picks an enemy target while charmed
            target = charm.GetCharmAttackTarget(transform);
        }

        // fallback to player if not charmed or no enemy target found
        if (target == null)
        {
            if (agent.playertransform != null)
            {
                target = agent.playertransform;
            }
            else
            {
                target = GameObject.FindGameObjectWithTag("Player")?.transform;
            }
        }

        if (target == null)
            return;

        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget > agent.config.meleeAttackRange)
            return;

        Vector3 directionToTarget = (target.position - transform.position).normalized;
        float dot = Vector3.Dot(transform.forward, directionToTarget);
        if (dot <= 0.5f)
            return;

        // first try to damage player, otherwise try an enemy
        PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(agent.config.meleeDamage);
            return;
        }

        BasicEnemyHealth enemyHealth = target.GetComponent<BasicEnemyHealth>();
        if (enemyHealth != null && enemyHealth != GetComponent<BasicEnemyHealth>())
        {
            enemyHealth.TakeDamage(agent.config.meleeDamage, directionToTarget);
        }

    }

}
