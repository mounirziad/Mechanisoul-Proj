using UnityEngine;
using UnityEngine.AI;

public class AiChasePlayerState : AiState
{
    float timer = 0.0f;
    float lostSightTimer = 0.0f;
    const float LOST_SIGHT_TIMEOUT = 3.0f;

    public AiStateId GetId()
    {
        return AiStateId.ChasePlayer;
    }

    public void Enter(AiAgent agent)
    {
        Debug.Log($"Entering {GetId()} state");

        lostSightTimer = 0.0f;

        if (agent.navMeshAgent != null && !agent.navMeshAgent.enabled)
        {
            agent.navMeshAgent.enabled = true;
        }
    }

    public void Update(AiAgent agent)
    {
        if (agent.isDead)
            return;

        if (PlayerHealth.IsPlayerDead)
            return;

        BasicEnemyHealth health = agent.GetComponent<BasicEnemyHealth>();
        if (health != null && health.IsStunned())
            return;

        Transform target = null;
        EnemyCharm charm = agent.GetComponent<EnemyCharm>();

        if (charm != null && charm.ShouldIgnorePlayerAndFightEnemies())
        {
            target = charm.GetCharmAttackTarget(agent.transform);
            if (target == null)
                return;
        }
        else
        {
            target = agent.playertransform;
        }

        if (target == null)
            return;

        float distanceToTarget = Vector3.Distance(agent.transform.position, target.position);

        if (distanceToTarget > agent.config.maxSightDistance)
        {
            agent.stateMachine.ChangeState(AiStateId.Idle);
            return;
        }

        bool hasLineOfSight = HasLineOfSight(agent, target);
        
        if (!hasLineOfSight)
        {
            lostSightTimer += Time.deltaTime;
            
            if (lostSightTimer >= LOST_SIGHT_TIMEOUT)
            {
                agent.stateMachine.ChangeState(AiStateId.Idle);
                return;
            }
        }
        else
        {
            lostSightTimer = 0.0f;
        }

        if (agent.navMeshAgent != null && agent.navMeshAgent.enabled)
        {
            timer -= Time.deltaTime;
            if (timer < 0.0f)
            {
                float sqdistance = (target.position - agent.navMeshAgent.destination).sqrMagnitude;
                if (sqdistance > agent.config.maxDistance * agent.config.maxDistance)
                {
                    Vector3 destination = target.position;
                    
                    EnemySeparation separation = agent.GetComponent<EnemySeparation>();
                    if (separation != null)
                    {
                        destination = separation.GetDestinationWithSeparation(destination);
                    }
                    
                    agent.navMeshAgent.destination = destination;
                }
                timer = agent.config.maxTime;
            }
        }

        if (agent.weapons.HasWeapon())
        {
            if (distanceToTarget < 15f && hasLineOfSight)
            {
                agent.stateMachine.ChangeState(AiStateId.Attack);
                return;
            }
        }
        else
        {
            if (distanceToTarget < agent.config.meleeAttackRange && hasLineOfSight)
            {
                agent.stateMachine.ChangeState(AiStateId.MeleeAttack);
                return;
            }
        }
    }

    private bool HasLineOfSight(AiAgent agent, Transform target)
    {
        Vector3 origin = agent.transform.position + Vector3.up * 1.5f;
        Vector3 targetPos = target.position + Vector3.up * 1.5f;
        Vector3 direction = targetPos - origin;
        float distance = direction.magnitude;

        LayerMask obstacleMask = LayerMask.GetMask("Default", "Floors");
        
        if (Physics.Raycast(origin, direction.normalized, out RaycastHit hit, distance, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawRay(origin, direction.normalized * hit.distance, Color.red, 0.5f);
            return false;
        }

        Debug.DrawRay(origin, direction.normalized * distance, Color.green, 0.5f);
        return true;
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