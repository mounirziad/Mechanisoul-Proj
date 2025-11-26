using UnityEngine;
using UnityEngine.AI;

public class AiAfterMeleeAttackState : AiState
{
    private Vector3 patrolTarget;
    private Vector3 spawnPosition;
    private bool hasReachedDestination;
    private float stateEnterTime;
    private float stateDuration = 1f; // 1 seconds in this state

    public AiStateId GetId()
    {
        return AiStateId.AfterMeleeAttack;
    }

    public void Enter(AiAgent agent)
    {
        //Debug.Log($"Entering {GetId()} state");
        spawnPosition = agent.transform.position;
        hasReachedDestination = true;
        stateEnterTime = Time.time;

        // Re-enable NavMeshAgent for patrolling
        if (agent.navMeshAgent != null && !agent.navMeshAgent.enabled)
        {
            agent.navMeshAgent.enabled = true;
        }

        // Make sure root motion is disabled
        Animator animator = agent.GetComponent<Animator>();
        if (animator != null)
        {
            animator.applyRootMotion = false;
        }
    }

    public void Update(AiAgent agent)
    {
        if (agent.isDead)
            return;

        if (agent.playertransform != null)
        {
            float distanceToPlayer = Vector3.Distance(agent.transform.position, agent.playertransform.position);
            
            if (distanceToPlayer <= agent.config.meleeAttackRange)
            {
                agent.stateMachine.ChangeState(AiStateId.MeleeAttack);
                return;
            }
            
            if (distanceToPlayer <= agent.config.maxSightDistance)
            {
                agent.stateMachine.ChangeState(AiStateId.ChasePlayer);
                return;
            }
        }

        if (Time.time >= stateEnterTime + stateDuration)
        {
            agent.stateMachine.ChangeState(AiStateId.Idle);
            return;
        }

        Patrol(agent);
    }

    public void Exit(AiAgent agent)
    {
        Debug.Log($"Exiting {GetId()} state");

        if (agent.navMeshAgent != null && agent.navMeshAgent.isActiveAndEnabled)
        {
            agent.navMeshAgent.ResetPath();
            agent.navMeshAgent.velocity = Vector3.zero;
        }
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