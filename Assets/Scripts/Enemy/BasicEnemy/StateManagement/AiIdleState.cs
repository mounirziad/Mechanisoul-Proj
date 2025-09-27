using UnityEngine;

public class AiIdleState : AiState
{
    public void Enter(AiAgent agent)
    {
    }

    public void Exit(AiAgent agent)
    {
    }

    public AiStateId GetId()
    {
        return AiStateId.Idle;
    }

    public void Update(AiAgent agent)
    {
        Vector3 playerDirection = agent.playertransform.position - agent.transform.position;
        if(playerDirection.magnitude > agent.config.maxSightDistance)
        {
            return;
        }

        Vector3 agentDirection = agent.transform.forward;

        playerDirection.Normalize();

        float dotProduct = Vector3.Dot(playerDirection, agentDirection);
        if(dotProduct > 0.0f)
        {
            agent.stateMachine.ChangeState(AiStateId.ChasePlayer);
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
}
