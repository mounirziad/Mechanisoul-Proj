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

        if (PlayerHealth.IsPlayerDead)
            return;

        BasicEnemyHealth health = agent.GetComponent<BasicEnemyHealth>();
        if (health != null && health.IsStunned())
            return;

        // pick chase target: charmed enemy or player
        Transform target = null;
        EnemyCharm charm = agent.GetComponent<EnemyCharm>();

        if (charm != null && charm.ShouldIgnorePlayerAndFightEnemies())
        {
            target = charm.GetCharmAttackTarget(agent.transform);
            // if charmed but no enemy found, just idle instead of chasing the player
            if (target == null)
                return;
        }
        else
        {
            target = agent.playertransform;
        }

        if (target == null)
            return;

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

        float distanceToTarget = Vector3.Distance(agent.transform.position, target.position);

        if (agent.weapons.HasWeapon())
        {
            if (distanceToTarget < 15f)
            {
                agent.stateMachine.ChangeState(AiStateId.Attack);
                return;
            }
        }
        else
        {
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