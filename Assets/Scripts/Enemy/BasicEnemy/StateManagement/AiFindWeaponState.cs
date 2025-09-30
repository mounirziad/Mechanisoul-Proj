using UnityEngine;

public class AiFindWeaponState : AiState
{
    public void Enter(AiAgent agent)
    {
        WeaponPickup pickup = FindClosestWeapon(agent);

        if (pickup != null)
        {
            if (agent.navMeshAgent != null && agent.navMeshAgent.isActiveAndEnabled && agent.navMeshAgent.isOnNavMesh)
            {
                agent.navMeshAgent.destination = pickup.transform.position;
                agent.navMeshAgent.speed = 5;
            }
        }
        else
        {
            Debug.LogWarning("No available weapons found — returning to Idle.");
            agent.stateMachine.ChangeState(AiStateId.Idle); // <-- push back to Idle
        }
    }

    public void Exit(AiAgent agent) { }

    public AiStateId GetId()
    {
        return AiStateId.FindWeapon;
    }

    public void Update(AiAgent agent) 
    {
        WeaponPickup pickup = FindClosestWeapon(agent);
        if (pickup == null)
        {
            agent.stateMachine.ChangeState(AiStateId.Idle);
            return;
        }

        if (agent.weapons.HasWeapon())
        {
            agent.weapons.ActivateWeapon();
        }

        if (agent.weapons.HasWeapon())
        {
            float distanceToPlayer = Vector3.Distance(agent.transform.position, agent.playertransform.position);
            if (distanceToPlayer < 15f)
            {
                agent.stateMachine.ChangeState(AiStateId.Attack);
            }
        }
    }

    private WeaponPickup FindClosestWeapon(AiAgent agent)
    {
        WeaponPickup[] weapons = Object.FindObjectsOfType<WeaponPickup>();
        WeaponPickup closestWeapon = null;
        float closestDistance = float.MaxValue;

        foreach (var weapon in weapons)
        {
            if (weapon.isTaken) continue; // skip taken weapons

            float distanceToWeapon = Vector3.Distance(agent.transform.position, weapon.transform.position);
            if (distanceToWeapon < closestDistance)
            {
                closestDistance = distanceToWeapon;
                closestWeapon = weapon;
            }
        }

        return closestWeapon;
    }
}
