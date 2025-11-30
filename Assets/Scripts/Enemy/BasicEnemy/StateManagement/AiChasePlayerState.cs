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

        if (!agent.enabled)
            return;

        if (PlayerHealth.IsPlayerDead)
            return;

        Transform target = agent.GetCurrentTarget();
        if (target == null)
        {
            agent.stateMachine.ChangeState(AiStateId.Idle);
            return;
        }

        BasicEnemyHealth health = agent.GetComponent<BasicEnemyHealth>();
        if (health != null && health.IsStunned())
            return;

        if (agent.navMeshAgent != null && agent.navMeshAgent.enabled)
        {
            timer -= Time.deltaTime;
            if (timer < 0.0f)
            {
                float sqdistance = (target.position - agent.navMeshAgent.destination).sqrMagnitude;
                if (sqdistance > agent.config.maxDistance * agent.config.maxDistance)
                {
                    agent.navMeshAgent.destination = target.position;
                }
                timer = agent.config.maxTime;
            }
        }

        if (agent.weapons.HasWeapon())
        {
            float distanceToTarget = Vector3.Distance(agent.transform.position, target.position);
            if (distanceToTarget < 15f)
            {
                agent.stateMachine.ChangeState(AiStateId.Attack);
                return;
            }
        }
        else
        {
            float distanceToTarget = Vector3.Distance(agent.transform.position, target.position);
            if (distanceToTarget < agent.config.meleeAttackRange)
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