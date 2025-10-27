using UnityEngine;

[CreateAssetMenu()]
public class AiAgentConfig : ScriptableObject
{
    public float maxTime = 1.0f;
    public float maxDistance = 1.0f;
    public float dieForce = 10f;
    public float maxSightDistance = 20.0f;

    [Header("Melee Attack Settings")]
    public float meleeAttackRange = 5.0f;
    public float meleeAttackCooldown = 0f;
    public float meleeDamage = 20f;
    public float meleeAttackCommitTime = 0f;

    [Header("Patrol Settings")]
    public float patrolRadius = 10f;
    public float patrolWaitTime = 2f;
    public float waypointReachedDistance = 0.5f;
}
