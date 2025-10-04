using UnityEngine;
using System.Collections;

public class AiDeathState : AiState
{
    public Vector3 direction;
    private const float FLASH_DELAY = 2f;
    private const float FLASH_DURATION = 1.5f;
    private const float FLASH_INTERVAL = 0.2f;

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
        agent.weapons.DropWeapon();

        // Start flashing effect after a delay
        agent.StartCoroutine(FlashAndDestroy(agent));
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

    /// <summary>Handles the flashing and destruction effect for dead enemies</summary>
    private IEnumerator FlashAndDestroy(AiAgent agent)
    {
        // Wait before starting the flash effect
        yield return new WaitForSeconds(FLASH_DELAY);

        // Get the renderer for flashing
        SkinnedMeshRenderer renderer = agent.skinnedMeshRenderer;
        if (renderer == null)
        {
            renderer = agent.GetComponentInChildren<SkinnedMeshRenderer>();
        }

        if (renderer != null)
        {
            float elapsedTime = 0f;
            bool isVisible = true;

            // Flash the enemy for the specified duration
            while (elapsedTime < FLASH_DURATION)
            {
                // Toggle visibility
                isVisible = !isVisible;
                renderer.enabled = isVisible;

                // Wait for the flash interval
                yield return new WaitForSeconds(FLASH_INTERVAL);
                elapsedTime += FLASH_INTERVAL;
            }

            // Ensure the renderer is disabled at the end
            renderer.enabled = false;
        }

        // Destroy the GameObject after flashing is complete
        Object.Destroy(agent.gameObject);
    }
}
