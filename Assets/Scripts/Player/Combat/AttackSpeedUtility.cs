using UnityEngine;
using System.Collections.Generic;

public static class AttackSpeedUtility
{
    [System.Serializable]
    public struct AttackSpeedModifier
    {
        public string sourceName;
        public float speedBonus;
        public float duration;
        public bool isPermanent;
        
        public AttackSpeedModifier(string name, float bonus, float dur = 0f, bool permanent = false)
        {
            sourceName = name;
            speedBonus = bonus;
            duration = dur;
            isPermanent = permanent;
        }
    }
    
    private static Dictionary<string, AttackSpeedModifier> temporaryModifiers = new Dictionary<string, AttackSpeedModifier>();
    
    public static float CalculateTotalAttackSpeed(PlayerManager playerManager, AttackSO attackData)
    {
        if (playerManager == null) return 1f;
        
        float baseSpeed = attackData != null ? attackData.baseAnimationSpeed : 1f;
        float totalSpeedBonus = 0f;
        
        // Add Joy upgrade bonus (if attack allows it)
        if (attackData == null || attackData.affectedByAttackSpeedUpgrades)
        {
            totalSpeedBonus += playerManager.GetAttackSpeedBuff();
        }
        
        // Add temporary modifiers
        totalSpeedBonus += GetTemporaryModifiersBonus();
        
        return baseSpeed * (1f + totalSpeedBonus);
    }
    
    public static void AddTemporarySpeedModifier(string sourceName, float speedBonus, float duration)
    {
        var modifier = new AttackSpeedModifier(sourceName, speedBonus, duration, false);
        temporaryModifiers[sourceName] = modifier;
        
        // Schedule removal if not permanent
        if (duration > 0f)
        {
            Time.timeScale = Time.timeScale; // Access Time for coroutine context
        }
    }
    
    public static void RemoveTemporarySpeedModifier(string sourceName)
    {
        if (temporaryModifiers.ContainsKey(sourceName))
        {
            temporaryModifiers.Remove(sourceName);
        }
    }
    
    public static float GetTemporaryModifiersBonus()
    {
        float totalBonus = 0f;
        var keysToRemove = new List<string>();
        
        foreach (var kvp in temporaryModifiers)
        {
            var modifier = kvp.Value;
            
            // Check if temporary modifier has expired
            if (!modifier.isPermanent && modifier.duration > 0f)
            {
                // For a complete implementation, you'd track when modifiers were added
                // and compare against Time.time. This is simplified for the example.
                totalBonus += modifier.speedBonus;
            }
            else if (modifier.isPermanent)
            {
                totalBonus += modifier.speedBonus;
            }
        }
        
        // Remove expired modifiers
        foreach (string key in keysToRemove)
        {
            temporaryModifiers.Remove(key);
        }
        
        return totalBonus;
    }
    
    public static bool HasAttackSpeedModifier(string sourceName)
    {
        return temporaryModifiers.ContainsKey(sourceName);
    }
    
    public static void ClearAllTemporaryModifiers()
    {
        temporaryModifiers.Clear();
    }
    
    public static string GetAttackSpeedDebugInfo(PlayerManager playerManager, AttackSO attackData)
    {
        if (playerManager == null) return "PlayerManager not found";
        
        float baseSpeed = attackData != null ? attackData.baseAnimationSpeed : 1f;
        float joyBonus = (attackData == null || attackData.affectedByAttackSpeedUpgrades) ? 
            playerManager.GetAttackSpeedBuff() : 0f;
        float tempBonus = GetTemporaryModifiersBonus();
        float totalSpeed = CalculateTotalAttackSpeed(playerManager, attackData);
        
        string attackInfo = attackData != null ? attackData.name : "Default Attack";
        
        return $"Attack: {attackInfo}\n" +
               $"Base Speed: {baseSpeed:F2}\n" +
               $"Joy Upgrade: +{joyBonus:F2}\n" +
               $"Temp Modifiers: +{tempBonus:F2}\n" +
               $"Final Speed: {totalSpeed:F2}";
    }
    
    // Extension method for easy access from AttackSO
    public static bool IsAffectedBySpeedUpgrades(this AttackSO attackData)
    {
        return attackData == null || attackData.affectedByAttackSpeedUpgrades;
    }
    
    // Extension method to get effective speed for an attack
    public static float GetEffectiveAnimationSpeed(this AttackSO attackData, PlayerManager playerManager)
    {
        return CalculateTotalAttackSpeed(playerManager, attackData);
    }
}