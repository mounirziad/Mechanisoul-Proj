using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class AiMeleeAttackState : AiState
{
    private float lastAttackTime = 0f;
    private Vector3 positionOnEnter;
    private bool wasNavMeshAgentEnabled;
    private bool isAttacking = false;
    private float attackCommitTime = 0f;
    private Coroutine attackCoroutine; // ADD THIS MISSING VARIABLE

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

        BasicEnemyHealth health = agent.GetComponent<BasicEnemyHealth>();
        if (health != null && health.IsStunned())
            return;

        Transform player = agent.playertransform;
        if (player == null)
        {
            agent.stateMachine.ChangeState(AiStateId.ChasePlayer);
            return;
        }

        if (agent.weapons.HasWeapon())
        {
            agent.stateMachine.ChangeState(AiStateId.Attack);
            return;
        }

        float distanceToPlayer = Vector3.Distance(agent.transform.position, player.position);
        
        Animator animator = agent.GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetBool("IsInMeleeRange", distanceToPlayer <= agent.config.meleeAttackRange);
        }

        if (distanceToPlayer > agent.config.meleeAttackRange)
        {
            agent.stateMachine.ChangeState(AiStateId.ChasePlayer);
            return;
        }

        if (isAttacking)
        {
            if (Time.time >= attackCommitTime + agent.config.meleeAttackCommitTime)
            {
                isAttacking = false;
            }
        }

        Vector3 directionToPlayer = (player.position - agent.transform.position).normalized;
        directionToPlayer.y = 0;
        if (directionToPlayer != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
            agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, lookRotation, Time.deltaTime * 8f);
        }

        if (!isAttacking && Time.time >= lastAttackTime + agent.config.meleeAttackCooldown)
        {
            StartAttack(agent);
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
        attackCommitTime = Time.time;
        lastAttackTime = Time.time;

        // Update the anchor position to current position before attack
        positionOnEnter = agent.transform.position;

        PerformMeleeAttack(agent);

        // Start coroutine to handle attack completion
        attackCoroutine = agent.StartCoroutine(HandleAttackCompletion(agent));
    }

    private IEnumerator HandleAttackCompletion(AiAgent agent)
    {
        // Wait for the attack commitment time
        yield return new WaitForSeconds(agent.config.meleeAttackCommitTime);

        // Update position anchor to wherever we ended up after attack animation
        positionOnEnter = agent.transform.position;
        isAttacking = false;
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