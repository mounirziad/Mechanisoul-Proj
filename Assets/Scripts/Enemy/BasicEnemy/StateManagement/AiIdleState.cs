using UnityEngine;
using UnityEngine.AI;

public class AiIdleState : AiState
{
    private Vector3 patrolTarget;
    private Vector3 spawnPosition;
    private bool hasReachedDestination;
    private float lastAttackTime = 0f; // ADD THIS

    public void Enter(AiAgent agent)
    {
        spawnPosition = agent.transform.position;
        hasReachedDestination = true;

        // Store the time when we enter idle state
        lastAttackTime = Time.time; // ADD THIS

        if (agent.navMeshAgent != null && !agent.navMeshAgent.enabled)
        {
            agent.navMeshAgent.enabled = true;
        }
    }

    public void Exit(AiAgent agent)
    {
        if (agent.navMeshAgent != null && agent.navMeshAgent.isActiveAndEnabled)
        {
            agent.navMeshAgent.ResetPath();
            agent.navMeshAgent.velocity = Vector3.zero;
        }
    }

    public AiStateId GetId()
    {
        return AiStateId.Idle;
    }

    public void Update(AiAgent agent)
    {
        if (CheckForPlayer(agent))
        {
            return;
        }

        Patrol(agent);
    }

    private bool CheckForPlayer(AiAgent agent)
    {
        // ADD COOLDOWN CHECK - ignore player for a period after attacking
        if (Time.time < lastAttackTime + agent.config.meleeAttackCooldown)
        {
            return false; // Still in cooldown, ignore player
        }

        Vector3 playerDirection = agent.playertransform.position - agent.transform.position;
        if (playerDirection.magnitude > agent.config.maxSightDistance)
        {
            return false;
        }

        Vector3 agentDirection = agent.transform.forward;
        playerDirection.Normalize();

        float dotProduct = Vector3.Dot(playerDirection, agentDirection);
        if (dotProduct > -0.707f)
        {
            agent.stateMachine.ChangeState(AiStateId.ChasePlayer);
            return true;
        }

        if (agent.weapons.HasWeapon())
        {
            float distanceToPlayer = Vector3.Distance(agent.transform.position, agent.playertransform.position);
            if (distanceToPlayer < 15f)
            {
                agent.stateMachine.ChangeState(AiStateId.Attack);
                return true;
            }
        }

        return false;
    }

    private void Patrol(AiAgent agent)
    {
        if (agent.navMeshAgent == null || !agent.navMeshAgent.enabled)
            return;

        if (hasReachedDestination || !agent.navMeshAgent.hasPath || agent.navMeshAgent.remainingDistance < agent.config.waypointReachedDistance)
        {
            SetNewPatrolTarget(agent);
            hasReachedDestination = false;
        }
    }

    private void SetNewPatrolTarget(AiAgent agent)
    {
        Vector2 randomPoint = Random.insideUnitCircle * agent.config.patrolRadius;
        Vector3 targetPosition = spawnPosition + new Vector3(randomPoint.x, 0f, randomPoint.y);

        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, agent.config.patrolRadius, NavMesh.AllAreas))
        {
            patrolTarget = hit.position;
            agent.navMeshAgent.SetDestination(patrolTarget);
        }
        else
        {
            agent.navMeshAgent.SetDestination(spawnPosition);
        }
    }
}