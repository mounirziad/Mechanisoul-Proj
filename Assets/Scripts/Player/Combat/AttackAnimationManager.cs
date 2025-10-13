using UnityEngine;
using System.Collections;

public class AttackAnimationManager : MonoBehaviour
{
    private Animator animator;
    private PlayerManager playerManager;
    private bool isApplyingAttackSpeed = false;
    private AttackSO currentAttackData;
    
    [Header("Animation Speed Settings")]
    [Tooltip("Minimum attack speed multiplier to prevent animations from becoming too slow")]
    public float minSpeedMultiplier = 0.5f;
    
    [Tooltip("Maximum attack speed multiplier to prevent animations from becoming too fast")]
    public float maxSpeedMultiplier = 3f;
    
    [Tooltip("Tags of animator states that should have attack speed applied")]
    public string[] attackStateTags = { "Attack", "Combo", "MeleeAttack" };

    void Awake()
    {
        animator = GetComponent<Animator>();
        playerManager = GetComponent<PlayerManager>();
    }

    public void ApplyAttackSpeedToAnimation(AttackSO attackData = null)
    {
        if (animator == null || playerManager == null) return;
        
        currentAttackData = attackData;
        
        // Calculate total speed multiplier
        float speedMultiplier = CalculateAttackSpeedMultiplier(attackData);
        
        StartCoroutine(ApplySpeedDuringAttackState(speedMultiplier));
    }

    private float CalculateAttackSpeedMultiplier(AttackSO attackData)
    {
        // Use the utility method for consistent calculation
        float finalSpeed = AttackSpeedUtility.CalculateTotalAttackSpeed(playerManager, attackData);
        
        // Clamp to reasonable bounds
        return Mathf.Clamp(finalSpeed, minSpeedMultiplier, maxSpeedMultiplier);
    }

    private IEnumerator ApplySpeedDuringAttackState(float speedMultiplier)
    {
        isApplyingAttackSpeed = true;
        animator.speed = speedMultiplier;
        
        // Debug info for testing
        if (currentAttackData != null)
        {
            float upgradeBonus = currentAttackData.affectedByAttackSpeedUpgrades ? playerManager.GetAttackSpeedBuff() : 0f;
            Debug.Log($"Attack Speed Applied: Base={currentAttackData.baseAnimationSpeed:F2}, " +
                     $"Joy Upgrade={upgradeBonus:F2}, Final={speedMultiplier:F2}");
        }
        
        // Wait until we're no longer in an attack state
        while (IsInAttackState())
        {
            yield return null;
        }
        
        // Reset speed when attack is complete
        ResetAnimationSpeed();
    }

    private bool IsInAttackState()
    {
        if (animator == null) return false;
        
        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
        
        foreach (string tag in attackStateTags)
        {
            if (currentState.IsTag(tag))
            {
                return true;
            }
        }
        
        return false;
    }

    public void ResetAnimationSpeed()
    {
        if (animator != null)
        {
            animator.speed = 1f;
            isApplyingAttackSpeed = false;
            currentAttackData = null;
        }
    }

    public bool IsApplyingAttackSpeed()
    {
        return isApplyingAttackSpeed;
    }

    public float GetCurrentSpeedMultiplier()
    {
        return CalculateAttackSpeedMultiplier(currentAttackData);
    }

    public AttackSO GetCurrentAttackData()
    {
        return currentAttackData;
    }

    // Utility method to get attack speed info for debugging/UI
    public string GetAttackSpeedDebugInfo()
    {
        return AttackSpeedUtility.GetAttackSpeedDebugInfo(playerManager, currentAttackData);
    }

    void OnDisable()
    {
        ResetAnimationSpeed();
        StopAllCoroutines();
    }
}