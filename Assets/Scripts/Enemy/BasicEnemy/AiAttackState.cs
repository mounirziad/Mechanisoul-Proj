using UnityEngine;

public class AiAttackState : AiState
{
    public AiStateId GetId()
    {
        return AiStateId.Attack;
    }
    private bool hasAimed = false;

    public void Enter(AiAgent agent)
    {

        if (agent.navMeshAgent != null && agent.navMeshAgent.isActiveAndEnabled && agent.navMeshAgent.isOnNavMesh)
        {
            agent.navMeshAgent.isStopped = true;
        }
        Animator anim = agent.weapons.GetComponent<Animator>();
        if (anim != null && !hasAimed)
        {
            anim.SetTrigger("Aim");
            anim.SetBool("IsAiming", true);
            hasAimed = true;
        }
    }

    public void Exit(AiAgent agent)
    {
        Animator anim = agent.weapons.GetComponent<Animator>();

        if (agent.navMeshAgent != null && agent.navMeshAgent.isActiveAndEnabled && agent.navMeshAgent.isOnNavMesh)
        {
            agent.navMeshAgent.isStopped = false;
        }

        if (anim != null)
        {
            anim.SetBool("IsAiming", false);
        }

        hasAimed = false; // reset for next time
    }

    public void Update(AiAgent agent)
    {
        // Get health/status
        BasicEnemyHealth health = agent.GetComponent<BasicEnemyHealth>();
        if (health != null)
        {
            // If stunned, skip movement completely
            if (health.IsStunned())
                return;

            // If slowed, movement still works, but speed is already reduced in EnemyHealth.Update()
        }

        if (!agent.weapons.HasWeapon())
        {
            agent.stateMachine.ChangeState(AiStateId.FindWeapon);
            return;
        }

        Transform player = agent.playertransform;
        if (player == null) return;

        // If player out of range, go back to Chase
        float distanceToPlayer = Vector3.Distance(agent.transform.position, player.position);
        if (distanceToPlayer > 15f) 
        {
            agent.stateMachine.ChangeState(AiStateId.ChasePlayer);
            return;
        }

        // Rotate AI body toward player (horizontal only)
        Vector3 dir = (player.position - agent.transform.position).normalized;
        dir.y = 0;
        Quaternion lookRot = Quaternion.LookRotation(dir);
        agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, lookRot, Time.deltaTime * 5f);

        // Aim weapon firePoint directly at player
        RaycastWeapon weapon = agent.weapons.CurrentWeapon;
        if (weapon != null && weapon.firePoint != null)
        {
            Vector3 aimDir = (player.position + Vector3.up * 1.5f) - weapon.firePoint.position; // aim near chest/head
            weapon.firePoint.rotation = Quaternion.Lerp(
                weapon.firePoint.rotation,
                Quaternion.LookRotation(aimDir),
                Time.deltaTime * 10f
            );

            // Fire if reasonably aligned
            float dot = Vector3.Dot(weapon.firePoint.forward, aimDir.normalized);
            if (dot > 0.95f) // alignment threshold
            {
                // Play firing animation
                Animator anim = agent.weapons.GetComponent<Animator>();
                if (anim != null)
                {
                    anim.SetTrigger("Fire");
                }

                // Shoot weapon
                weapon.Fire();
            }
        }
    }
}
