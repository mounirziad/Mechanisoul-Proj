using UnityEngine;

public class AiFindWeaponState : AiState
{
    public void Enter(AiAgent agent)
    {
        WeaponPickup pickup = FindClosestWeapon(agent);
        if (pickup != null)
        {
            agent.navMeshAgent.destination = pickup.transform.position;
            agent.navMeshAgent.speed = 5;
        }
        else
        {
            Debug.LogWarning("No weapon pickups found!");
        }
    }

    public void Exit(AiAgent agent) { }

    public AiStateId GetId()
    {
        return AiStateId.FindWeapon;
    }

    public void Update(AiAgent agent) 
    {
        if (agent.weapons.HasWeapon()) 
        {
            agent.weapons.ActivateWeapon();
        }
    }

    private WeaponPickup FindClosestWeapon(AiAgent agent)
    {
        WeaponPickup[] weapons = Object.FindObjectsOfType<WeaponPickup>();
        WeaponPickup closestWeapon = null;
        float closestDistance = float.MaxValue;

        foreach (var weapon in weapons)
        {
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
