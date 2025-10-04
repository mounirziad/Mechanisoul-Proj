using UnityEngine;
using System.Collections.Generic;

public class WeaponHitTracker : MonoBehaviour
{
    [Header("Hit Tracking Settings")]
    [SerializeField] private float hitTrackingDuration = 0.5f;
    
    private HashSet<GameObject> hitEnemiesThisAttack = new HashSet<GameObject>();
    private float currentAttackStartTime = -999f;
    
    /// <summary>
    /// Call this when a new attack begins to reset the hit tracking
    /// </summary>
    public void StartNewAttack()
    {
        hitEnemiesThisAttack.Clear();
        currentAttackStartTime = Time.time;
    }
    
    /// <summary>
    /// Check if this enemy has already been hit during the current attack
    /// </summary>
    /// <param name="enemy">The enemy GameObject to check</param>
    /// <returns>True if this is the first hit on this enemy, false if already hit</returns>
    public bool TryHitEnemy(GameObject enemy)
    {
        // Clear old hits if enough time has passed (safety cleanup)
        if (Time.time - currentAttackStartTime > hitTrackingDuration)
        {
            hitEnemiesThisAttack.Clear();
        }
        
        // Get the root enemy GameObject (in case we hit a child collider)
        GameObject rootEnemy = GetRootEnemyObject(enemy);
        
        if (hitEnemiesThisAttack.Contains(rootEnemy))
        {
            return false; // Already hit this enemy
        }
        
        hitEnemiesThisAttack.Add(rootEnemy);
        return true; // First hit on this enemy
    }
    
    /// <summary>
    /// Get the root enemy GameObject, handling ragdoll hierarchies
    /// </summary>
    private GameObject GetRootEnemyObject(GameObject hitObject)
    {
        // Try to find the root by looking for common enemy components
        Transform current = hitObject.transform;
        
        // Look up the hierarchy for the main enemy GameObject
        while (current != null)
        {
            // Check for common enemy components that would indicate the root
            if (current.GetComponent<BasicEnemyHealth>() != null ||
                current.GetComponent<Mechromancer>() != null ||
                current.GetComponent<PlayerManager>() != null) // In case it's the player
            {
                return current.gameObject;
            }
            
            current = current.parent;
        }
        
        // If no specific enemy component found, use the original object
        return hitObject;
    }
    
    /// <summary>
    /// Get the number of enemies hit during the current attack
    /// </summary>
    public int GetHitEnemyCount()
    {
        return hitEnemiesThisAttack.Count;
    }
    
    /// <summary>
    /// Check if any enemies have been hit during the current attack
    /// </summary>
    public bool HasHitAnyEnemies()
    {
        return hitEnemiesThisAttack.Count > 0;
    }
}