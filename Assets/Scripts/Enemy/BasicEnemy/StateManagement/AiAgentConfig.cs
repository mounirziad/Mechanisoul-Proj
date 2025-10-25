using UnityEngine;

[CreateAssetMenu()]
public class AiAgentConfig : ScriptableObject
{
    public float maxTime = 1.0f;
    public float maxDistance = 1.0f;
    public float dieForce = 10f;
    public float maxSightDistance = 5.0f;

    [Header("Melee Attack Settings")]
    public float meleeAttackRange = 2.5f;
    public float meleeAttackCooldown = 1.5f;
    public float meleeDamage = 20f;
    public float meleeAttackCommitTime = 5.5f; // How long to commit to the attack

}
