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

        // Enable root motion
        Animator animator = agent.GetComponent<Animator>();
        if (animator != null)
        {
            animator.applyRootMotion = true;
        }

        // Configure NavMeshAgent for root motion
        if (agent.navMeshAgent != null)
        {
            wasNavMeshAgentEnabled = agent.navMeshAgent.enabled;

            // Keep NavMeshAgent enabled but configure it for root motion
            agent.navMeshAgent.isStopped = true;
            agent.navMeshAgent.ResetPath();
            agent.navMeshAgent.velocity = Vector3.zero;

            // These are correct for root motion:
            agent.navMeshAgent.updatePosition = false;
            agent.navMeshAgent.updateRotation = false;
        }

        // Start attack immediately
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

        // ONLY check for weapon changes during attack, not distance
        if (agent.weapons.HasWeapon())
        {
            agent.stateMachine.ChangeState(AiStateId.Attack);
            return;
        }

        // If we're currently in an attack animation, COMMIT to it
        if (isAttacking)
        {
            // Wait for attack to complete before checking distance again
            if (Time.time >= attackCommitTime + agent.config.meleeAttackCommitTime)
            {
                isAttacking = false;

                // NOW check if player moved away
                float distanceToPlayer = Vector3.Distance(agent.transform.position, player.position);
                if (distanceToPlayer > agent.config.meleeAttackRange)
                {
                    agent.stateMachine.ChangeState(AiStateId.ChasePlayer);
                    return;
                }
            }
        }
        else
        {
            // Not currently attacking, check distance normally
            float distanceToPlayer = Vector3.Distance(agent.transform.position, player.position);
            if (distanceToPlayer > agent.config.meleeAttackRange)
            {
                agent.stateMachine.ChangeState(AiStateId.ChasePlayer);
                return;
            }
        }

        // Only rotate towards player, NO MOVEMENT
        Vector3 directionToPlayer = (player.position - agent.transform.position).normalized;
        directionToPlayer.y = 0;
        if (directionToPlayer != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(directionToPlayer);
            agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, lookRotation, Time.deltaTime * 8f);
        }

        // REMOVE the hard position lock since we're using root motion
        // agent.transform.position = positionOnEnter; // COMMENT THIS OUT

        // Only start new attack if not currently attacking and cooldown is ready
        if (!isAttacking && Time.time >= lastAttackTime + agent.config.meleeAttackCooldown)
        {
            StartAttack(agent);
        }
    }

    public void Exit(AiAgent agent)
    {
        Debug.Log($"Exiting {GetId()} state");
        isAttacking = false;

        // Disable root motion
        Animator animator = agent.GetComponent<Animator>();
        if (animator != null)
        {
            animator.applyRootMotion = false;
        }

        // Stop any running coroutines
        if (attackCoroutine != null)
        {
            agent.StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        // Re-enable NavMeshAgent properly
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