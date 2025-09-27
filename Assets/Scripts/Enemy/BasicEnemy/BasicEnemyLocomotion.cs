using UnityEngine;
using UnityEngine.AI;
public class BasicEnemyLocomotion : MonoBehaviour
{
    NavMeshAgent agent;

    Animator animator;


    // Knockback settings
    private Vector3 knockbackVelocity = Vector3.zero;
    private float knockbackTime = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        AiAgent aiAgent = GetComponent<AiAgent>();
        BasicEnemyHealth health = GetComponent<BasicEnemyHealth>();
        if (health != null && aiAgent.isDead) return; // Skip updates when dead
       
        animator.SetFloat("Speed", agent.velocity.magnitude);

        // Apply knockback velocity if active
        if (knockbackTime > 0f)
        {
            agent.Move(knockbackVelocity * Time.deltaTime);
            knockbackTime -= Time.deltaTime;
        }
    }

    public void ApplyKnockback(Vector3 direction, float force, float duration)
    {
        knockbackVelocity = direction.normalized * force;
        knockbackTime = duration;
    }

    public void DisableNavMeshAgent()
    {
        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }
    }
}
