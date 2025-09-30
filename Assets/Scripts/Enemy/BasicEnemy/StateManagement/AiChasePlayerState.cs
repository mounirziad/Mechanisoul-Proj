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

            // If slowed, movement still works, but speed is already reduced in EnemyHealth.Update()
        }

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

        if (agent.weapons.HasWeapon())
        {
            float distanceToPlayer = Vector3.Distance(agent.transform.position, agent.playertransform.position);
            if (distanceToPlayer < 15f) // arbitrary attack range
            {
                agent.stateMachine.ChangeState(AiStateId.Attack);
            }
        }
    }

    public void Exit(AiAgent agent)
    {
    }
}
