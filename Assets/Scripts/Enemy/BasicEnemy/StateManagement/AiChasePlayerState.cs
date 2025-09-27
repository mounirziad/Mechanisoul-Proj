using UnityEngine;
using UnityEngine.AI;

public class AiChasePlayerState : AiState
{
    public Transform playertransform;

    float timer = 0.0f;
  
    public AiStateId GetId()
    {
        return AiStateId.ChasePlayer;
    }

    public void Enter(AiAgent agent)
    {
        playertransform = GameObject.FindGameObjectWithTag("Player").transform;
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
            float sqdistance = (playertransform.position - agent.navMeshAgent.destination).sqrMagnitude;
            if (sqdistance > agent.config.maxDistance * agent.config.maxDistance)
            {
                agent.navMeshAgent.destination = playertransform.position;
            }
            timer = agent.config.maxTime;
        }
    }

    public void Exit(AiAgent agent)
    {
    }
}
