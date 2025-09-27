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
        if (!agent.enabled)
        {
            return;
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
    }

    public void Exit(AiAgent agent)
    {
    }
}
