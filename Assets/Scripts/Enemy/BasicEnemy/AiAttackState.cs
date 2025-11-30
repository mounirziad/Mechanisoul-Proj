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
        if (agent.isDead)
            return;

        if (PlayerHealth.IsPlayerDead)
            return;

        BasicEnemyHealth health = agent.GetComponent<BasicEnemyHealth>();
        if (health != null)
        {
            if (health.IsStunned())
                return;
        }

        if (!agent.weapons.HasWeapon())
        {
            agent.stateMachine.ChangeState(AiStateId.FindWeapon);
            return;
        }

        // pick target: charmed enemy or player
        Transform target = null;
        EnemyCharm charm = agent.GetComponent<EnemyCharm>();

        if (charm != null && charm.ShouldIgnorePlayerAndFightEnemies())
        {
            target = charm.GetCharmAttackTarget(agent.transform);
            if (target == null)
                return; // charmed but no enemy to shoot
        }
        else
        {
            target = agent.playertransform;
        }

        if (target == null)
        {
            agent.stateMachine.ChangeState(AiStateId.ChasePlayer);
            return;
        }

        float distanceToTarget = Vector3.Distance(agent.transform.position, target.position);
        if (distanceToTarget > 15f)
        {
            agent.stateMachine.ChangeState(AiStateId.ChasePlayer);
            return;
        }

        Vector3 dir = (target.position - agent.transform.position).normalized;
        dir.y = 0;
        Quaternion lookRot = Quaternion.LookRotation(dir);
        agent.transform.rotation = Quaternion.Slerp(agent.transform.rotation, lookRot, Time.deltaTime * 5f);

        RaycastWeapon weapon = agent.weapons.CurrentWeapon;
        if (weapon != null && weapon.firePoint != null)
        {
            Vector3 aimDir = (target.position + Vector3.up * 1.5f) - weapon.firePoint.position;
            weapon.firePoint.rotation = Quaternion.Lerp(
                weapon.firePoint.rotation,
                Quaternion.LookRotation(aimDir),
                Time.deltaTime * 10f
            );

            float dot = Vector3.Dot(weapon.firePoint.forward, aimDir.normalized);
            if (dot > 0.95f)
            {
                Animator anim = agent.weapons.GetComponent<Animator>();
                if (anim != null)
                {
                    anim.SetTrigger("Fire");
                }

                weapon.Fire();
            }
        }

    }
}
