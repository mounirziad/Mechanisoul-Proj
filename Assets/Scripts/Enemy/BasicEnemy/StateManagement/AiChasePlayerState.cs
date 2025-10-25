using UnityEngine;
using UnityEngine.AI;

public class AiChasePlayerState : AiState
{
    float timer = 0.0f;

    public AiStateId GetId()
    {
        return AiStateId.ChasePlayer;
    }

    public void Enter(AiAgent agent)
    {
        Debug.Log($"Entering {GetId()} state");

        // Ensure NavMeshAgent is enabled when entering chase state
        if (agent.navMeshAgent != null && !agent.navMeshAgent.enabled)
        {
            agent.navMeshAgent.enabled = true;
        }
    }

    public void Update(AiAgent agent)
    {
        if (agent.isDead)
            return;

        if (!agent.enabled) return;

        // Get health/status
        BasicEnemyHealth health = agent.GetComponent<BasicEnemyHealth>();
        if (health != null)
        {
            // If stunned, skip movement completely
            if (health.IsStunned())
                return;
        }

        // Only update navigation if NavMeshAgent is enabled
        if (agent.navMeshAgent != null && agent.navMeshAgent.enabled)
        {
            timer -= Time.deltaTime;
            if (timer < 0.0f)
            {
                float sqdistance = (agent.playertransform.position - agent.navMeshAgent.destination).sqrMagnitude;
                if (sqdistance > agent.config.maxDistance * agent.config.maxDistance)
                {
                    agent.navMeshAgent.destination = agent.playertransform.position;
                }
                timer = agent.config.maxTime;
            }
        }

        if (agent.weapons.HasWeapon())
        {
            float distanceToPlayer = Vector3.Distance(agent.transform.position, agent.playertransform.position);
            if (distanceToPlayer < 15f)
            {
                agent.stateMachine.ChangeState(AiStateId.Attack);
                return;
            }
        }
        else
        {
            float distanceToPlayer = Vector3.Distance(agent.transform.position, agent.playertransform.position);
            if (distanceToPlayer < agent.config.meleeAttackRange)
            {
                agent.stateMachine.ChangeState(AiStateId.MeleeAttack);
                return;
            }
        }
    }

    public void Exit(AiAgent agent)
    {
        Debug.Log($"Exiting {GetId()} state");
        // Don't disable NavMeshAgent here - let the next state handle it
        if (agent.navMeshAgent != null && agent.navMeshAgent.isActiveAndEnabled)
        {
            agent.navMeshAgent.ResetPath();
            agent.navMeshAgent.velocity = Vector3.zero;
        }
    }
}