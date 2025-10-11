using System;
using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 3f;
    public float damage = 10f;

    private Vector3 direction;
    private RangedModifiers mods;
    private PlayerCombat owner;
    private Vector3 targetPosition;
    private bool hasTarget = false;



    public void Initialize(Vector3 shootDirection, float projectileDamage, RangedModifiers rangedMods, PlayerCombat owningCombat = null)
    {
        direction = shootDirection.normalized;
        damage = projectileDamage;
        mods = rangedMods;
        owner = owningCombat;
        
        // Align the projectile's rotation to the direction
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
        
        Destroy(gameObject, lifetime);
    }

    public void InitializeWithTarget(Vector3 targetPos, float projectileDamage, RangedModifiers rangedMods, PlayerCombat owningCombat = null)
    {
        targetPosition = targetPos;
        hasTarget = true;
        direction = (targetPosition - transform.position).normalized;
        damage = projectileDamage;
        mods = rangedMods;
        owner = owningCombat;
        
        // Align the projectile's rotation to the direction
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
        
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        // Debug: Print detailed collision info
        Debug.Log($"=== PROJECTILE COLLISION DEBUG ===");
        Debug.Log($"Projectile position: {transform.position}");
        Debug.Log($"Other collider name: {other.name}");
        Debug.Log($"Other collider tag: {other.tag}");
        Debug.Log($"Other collider layer: {LayerMask.LayerToName(other.gameObject.layer)}");
        Debug.Log($"Other transform position: {other.transform.position}");
        
        // Check if other is the player
        if (other.CompareTag("Player"))
        {
            Debug.LogError("PROJECTILE HIT PLAYER! This should not happen!");
            Debug.LogError($"Player position: {other.transform.position}");
            Debug.LogError($"Projectile position: {transform.position}");
            // Don't process player hits
            Destroy(gameObject);
            return;
        }
        
        Vector3 hitPos = transform.position; // Default to projectile position
        bool hitEnemy = false;

        // Check if we hit an enemy using the most reliable method first
        var enemyHealth = other.GetComponentInParent<BasicEnemyHealth>();
        if (enemyHealth != null)
        {
            // Use the exact collision point for better accuracy
            hitPos = other.ClosestPoint(transform.position);
            Debug.Log($"Calculated hit position: {hitPos}");
            Debug.Log($"Enemy health component found on: {enemyHealth.name}");
            Debug.Log($"Enemy position: {enemyHealth.transform.position}");
            
            enemyHealth.TakeDamage(damage, direction);
            hitEnemy = true;
        }
        else
        {
            // Fallback: tag check
            if (other.CompareTag("Enemy") || (other.transform.root != null && other.transform.root.CompareTag("Enemy")))
            {
                hitPos = other.ClosestPoint(transform.position);
                Debug.Log($"Calculated hit position (tag check): {hitPos}");
                hitEnemy = true;
            }
        }

        // Only spawn VFX if we actually hit an enemy
        if (hitEnemy)
        {
            Debug.Log($"=== SPAWNING VFX AT POSITION: {hitPos} ===");
            
            // ANGER (gameplay): DoT AoE on impact
            if (mods.angerExplosionOnHit && owner != null && owner.angerExplosionPrefab != null)
            {
                Debug.Log($"About to spawn Anger VFX at: {hitPos}");
                var aoe = Instantiate(owner.angerExplosionPrefab, hitPos, Quaternion.identity);
                
                // Disable VFXCameraLookAt component that incorrectly moves VFX to camera
                var cameraLookAt = aoe.GetComponent<VFXCameraLookAt>();
                if (cameraLookAt != null)
                {
                    cameraLookAt.enabled = false;
                    Debug.Log("Disabled VFXCameraLookAt component on Anger VFX");
                }
                
                Debug.Log($"Anger VFX actual spawned position: {aoe.transform.position}");
                
                var dot = aoe.GetComponent<FireDoTZone>();
                if (dot != null)
                    dot.Configure(Mathf.Max(0f, mods.angerAOEPercent));
                else
                    aoe.SendMessage("Configure", mods.angerAOEPercent, SendMessageOptions.DontRequireReceiver);
            }

            // JOY (visual): pop VFX on enemy contact when synergy is active
            if (mods.joyExplosionOnHit && owner != null && owner.joyExplosionPrefab != null)
            {
                Debug.Log($"About to spawn Joy VFX at: {hitPos}");
                var vfx = Instantiate(owner.joyExplosionPrefab, hitPos, Quaternion.identity);
                
                // Disable VFXCameraLookAt component that incorrectly moves VFX to camera
                var cameraLookAt = vfx.GetComponent<VFXCameraLookAt>();
                if (cameraLookAt != null)
                {
                    cameraLookAt.enabled = false;
                    Debug.Log("Disabled VFXCameraLookAt component on Joy VFX");
                }
                
                Debug.Log($"Joy VFX actual spawned position: {vfx.transform.position}");
                AutoDestroyVFX(vfx);
            }
        }
        else
        {
            Debug.Log($"Hit non-enemy object: {other.name} at position: {hitPos}");
        }

        Destroy(gameObject);
    }


    void AutoDestroyVFX(GameObject go)
    {
        float fallback = 2f;
        float maxTime = 0f;

        var psList = go.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in psList)
        {
            var m = ps.main;
            float dur = m.duration + m.startLifetime.constantMax;
            if (dur > maxTime) maxTime = dur;
        }

        Destroy(go, maxTime > 0.05f ? maxTime : fallback);
    }


}

