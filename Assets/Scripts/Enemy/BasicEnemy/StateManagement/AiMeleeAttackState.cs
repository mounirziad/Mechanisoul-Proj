using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class AiMeleeAttackState : AiState
{
    private float lastAttackTime = 0f;
    private Vector3 positionOnEnter;
    private bool wasNavMeshAgentEnabled;
    private bool isAttacking = false;
    private Coroutine attackCoroutine;

    public AiStateId GetId()
    {
        return AiStateId.MeleeAttack;
    }

    public void Enter(AiAgent agent)
    {
        Debug.Log($"Entering {GetId()} state");
        positionOnEnter = agent.transform.position;
        isAttacking = false;

        Animator animator = agent.GetComponent<Animator>();
        if (animator != null)
        {
            animator.applyRootMotion = true;
            animator.SetBool("IsInMeleeRange", true);
        }

        if (agent.navMeshAgent != null)
        {
            wasNavMeshAgentEnabled = agent.navMeshAgent.enabled;
            agent.navMeshAgent.isStopped = true;
            agent.navMeshAgent.ResetPath();
            agent.navMeshAgent.velocity = Vector3.zero;
            agent.navMeshAgent.updatePosition = false;
            agent.navMeshAgent.updateRotation = false;
        }

        if (Time.time >= lastAttackTime + agent.config.meleeAttackCooldown)
        {
            StartAttack(agent);
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

        Transform player = agent.playertransform;
        if (player == null)
        {
            agent.stateMachine.ChangeState(AiStateId.Idle);
            return;
        }

        if (agent.weapons.HasWeapon())
        {
            agent.stateMachine.ChangeState(AiStateId.Attack);
            return;
        }

        if (isAttacking)
        {
            Vector3 directionToPlayer = (player.position - agent.transform.position).normalized;
            directionToPlayer.y = 0;
            if (directionToPlayer != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
                agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, lookRotation, Time.deltaTime * 8f);
            }
            return;
        }

        if (!isAttacking && Time.time >= lastAttackTime + agent.config.meleeAttackCooldown)
        {
            StartAttack(agent);
        }

        Vector3 directionToPlayerNormal = (player.position - agent.transform.position).normalized;
        directionToPlayerNormal.y = 0;
        if (directionToPlayerNormal != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(directionToPlayerNormal);
            agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, lookRotation, Time.deltaTime * 8f);
        }
    }

    public void Exit(AiAgent agent)
    {
        Debug.Log($"Exiting {GetId()} state");
        isAttacking = false;

        Animator animator = agent.GetComponent<Animator>();
        if (animator != null)
        {
            animator.applyRootMotion = false;
            animator.SetBool("IsInMeleeRange", false);
        }

        if (attackCoroutine != null)
        {
            agent.StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        if (agent.navMeshAgent != null && wasNavMeshAgentEnabled)
        {
            agent.navMeshAgent.updatePosition = true;
            agent.navMeshAgent.updateRotation = true;
            agent.navMeshAgent.enabled = true;
            if (agent.navMeshAgent.isOnNavMesh)
            {
                agent.navMeshAgent.Warp(agent.transform.position);
            }
        }
    }

    private void StartAttack(AiAgent agent)
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        PerformMeleeAttack(agent);

        attackCoroutine = agent.StartCoroutine(WaitForAttackCompletion(agent));
    }

    private IEnumerator WaitForAttackCompletion(AiAgent agent)
    {
        Animator animator = agent.GetComponent<Animator>();

        // Wait for the attack animation to start
        yield return null;

        // Get the current animation clip length
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float animationLength = stateInfo.length;

        // Wait for the animation to complete (plus a small buffer)
        yield return new WaitForSeconds(animationLength + 1.1f);

        // Now transition to the after attack state
        isAttacking = false;
        agent.stateMachine.ChangeState(AiStateId.AfterMeleeAttack);
    }

    private void PerformMeleeAttack(AiAgent agent)
    {
        Animator animator = agent.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("MeleeAttack");
        }
    }
}