using UnityEngine;

public class AiDeathState : AiState
{
    public Vector3 direction;
    public void Enter(AiAgent agent)
    {
        agent.isDead = true;

        // Stop NavMeshAgent movement
        BasicEnemyLocomotion locomotion = agent.GetComponent<BasicEnemyLocomotion>();
        if (locomotion != null)
        {
            locomotion.DisableNavMeshAgent();
        }

        agent.ragdoll.ActivateRagdoll();
        direction.y = 1;
        agent.ragdoll.ApplyForce(direction * agent.config.dieForce);
    }

    public void Exit(AiAgent agent)
    {
    }

    public AiStateId GetId()
    {
        return AiStateId.Death;
    }

    public void Update(AiAgent agent)
    {
    }
}
