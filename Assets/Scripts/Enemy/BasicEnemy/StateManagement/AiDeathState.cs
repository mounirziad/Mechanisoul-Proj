using UnityEngine;
using System.Collections;

public class AiDeathState : AiState
{
    public Vector3 direction;
    private const float FLASH_DELAY = 2f;
    private const float FLASH_DURATION = 1.5f;
    private const float FLASH_INTERVAL = 0.2f;
    private Coroutine flashCoroutine;

    public void Enter(AiAgent agent)
    {
        if (agent == null || agent.isDead) return;

        agent.isDead = true;

        BasicEnemyLocomotion locomotion = agent.GetComponent<BasicEnemyLocomotion>();
        if (locomotion != null)
        {
            locomotion.DisableNavMeshAgent();
        }

        DisableHitBoxesAndCollision(agent);

        if (agent.ragdoll != null)
        {
            agent.ragdoll.ActivateRagdoll();
            direction.y = 1;
            agent.ragdoll.ApplyForce(direction * agent.config.dieForce);
        }

        if (agent.weapons != null)
        {
            agent.weapons.DropWeapon();
        }

        if (flashCoroutine != null)
        {
            agent.StopCoroutine(flashCoroutine);
        }
        flashCoroutine = agent.StartCoroutine(FlashAndDestroy(agent));
    }

    public void Exit(AiAgent agent)
    {
        if (agent != null && flashCoroutine != null)
        {
            agent.StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }
    }

    public AiStateId GetId()
    {
        return AiStateId.Death;
    }

    public void Update(AiAgent agent)
    {
    }

    private IEnumerator FlashAndDestroy(AiAgent agent)
    {
        if (agent == null) yield break;

        yield return new WaitForSeconds(FLASH_DELAY);

        if (agent == null || agent.gameObject == null) yield break;

        SkinnedMeshRenderer renderer = agent.skinnedMeshRenderer;
        if (renderer == null)
        {
            renderer = agent.GetComponentInChildren<SkinnedMeshRenderer>();
        }

        if (renderer != null)
        {
            float elapsedTime = 0f;
            bool isVisible = true;

            while (elapsedTime < FLASH_DURATION)
            {
                if (renderer == null || agent == null) yield break;

                isVisible = !isVisible;
                renderer.enabled = isVisible;

                yield return new WaitForSeconds(FLASH_INTERVAL);
                elapsedTime += FLASH_INTERVAL;
            }

            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }

        if (agent != null && agent.gameObject != null)
        {
            Object.Destroy(agent.gameObject);
        }

        flashCoroutine = null;
    }

    private void DisableHitBoxesAndCollision(AiAgent agent)
    {
        if (agent == null) return;

        HitBox[] hitBoxes = agent.GetComponentsInChildren<HitBox>();
        foreach (HitBox hitBox in hitBoxes)
        {
            if (hitBox != null)
            {
                hitBox.enabled = false;
            }
        }

        SetLayerRecursively(agent.gameObject, LayerMask.NameToLayer("Ignore Raycast"));
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        if (obj == null) return;

        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            if (child != null)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }
    }
}
