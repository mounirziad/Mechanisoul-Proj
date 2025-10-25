using UnityEngine;

public class EnemyMeleeDamage : MonoBehaviour
{
    [Header("Melee Settings")]
    public LayerMask playerLayer;

    private Transform playerTransform;
    private AiAgent agent;

    private void Start()
    {
        agent = GetComponent<AiAgent>();
        if (agent != null)
        {
            playerTransform = agent.playertransform;
        }
    }

    public void DealMeleeDamage()
    {
        if (agent == null || agent.config == null)
            return;

        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (playerTransform == null) return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        
        if (distanceToPlayer <= agent.config.meleeAttackRange)
        {
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
            float dotProduct = Vector3.Dot(transform.forward, directionToPlayer);
            
            if (dotProduct > 0.5f)
            {
                PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(agent.config.meleeDamage);
                }
            }
        }
    }
}
