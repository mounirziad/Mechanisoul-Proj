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
    private float ignorePlayerCollisionTime = 0.1f;
    private float spawnTime;



    public void Initialize(Vector3 shootDirection, float projectileDamage, RangedModifiers rangedMods, PlayerCombat owningCombat = null, float targetDistance = 0f)
    {
        direction = shootDirection.normalized;
        damage = projectileDamage;
        mods = rangedMods;
        owner = owningCombat;
        spawnTime = Time.time;
        
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
        
        float calculatedLifetime = lifetime;
        if (targetDistance > 0f)
        {
            calculatedLifetime = (targetDistance / speed) + 1f;
            calculatedLifetime = Mathf.Max(calculatedLifetime, lifetime);
        }
        
        Destroy(gameObject, calculatedLifetime);
    }

    public void InitializeWithTarget(Vector3 targetPos, float projectileDamage, RangedModifiers rangedMods, PlayerCombat owningCombat = null)
    {
        targetPosition = targetPos;
        hasTarget = true;
        direction = (targetPosition - transform.position).normalized;
        damage = projectileDamage;
        mods = rangedMods;
        owner = owningCombat;
        spawnTime = Time.time;
        
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
        float distanceTraveled = Vector3.Distance(transform.position, owner != null ? owner.transform.position : Vector3.zero);
        Debug.Log($"[PlayerProjectile] Hit {other.gameObject.name} (Layer: {LayerMask.LayerToName(other.gameObject.layer)}) at distance: {distanceTraveled:F2}m");
        
        if (other.CompareTag("Player"))
        {
            if (Time.time - spawnTime < ignorePlayerCollisionTime)
            {
                Debug.Log($"[PlayerProjectile] Ignoring player collision (too soon after spawn)");
                return;
            }
            Destroy(gameObject);
            return;
        }
        
        Vector3 hitPos = transform.position;
        bool hitEnemy = false;

        var enemyHealth = other.GetComponentInParent<BasicEnemyHealth>();
        if (enemyHealth != null)
        {
            hitPos = other.ClosestPoint(transform.position);
            enemyHealth.TakeDamage(damage, direction);
            hitEnemy = true;
            Debug.Log($"[PlayerProjectile] Dealt {damage} damage to {other.gameObject.name} via BasicEnemyHealth");
        }
        else
        {
            var damageable = other.GetComponentInParent<IDamage>();
            if (damageable != null)
            {
                hitPos = other.ClosestPoint(transform.position);
                damageable.TakeDamage(damage);
                hitEnemy = true;
                Debug.Log($"[PlayerProjectile] Dealt {damage} damage to {other.gameObject.name} via IDamage");
            }
            else if (other.CompareTag("Enemy") || (other.transform.root != null && other.transform.root.CompareTag("Enemy")))
            {
                hitPos = other.ClosestPoint(transform.position);
                hitEnemy = true;
                Debug.LogWarning($"[PlayerProjectile] Hit enemy {other.gameObject.name} but no damage component found!");
            }
        }

        if (hitEnemy)
        {
            if (mods.angerExplosionOnHit && owner != null && owner.angerExplosionPrefab != null)
            {
                var aoe = Instantiate(owner.angerExplosionPrefab, hitPos, Quaternion.identity);
                
                var cameraLookAt = aoe.GetComponent<VFXCameraLookAt>();
                if (cameraLookAt != null)
                {
                    cameraLookAt.enabled = false;
                }
                
                var dot = aoe.GetComponent<AngerBurstZone>();
                if (dot != null)
                    dot.Configure(Mathf.Max(0f, mods.angerAOEPercent));
                else
                    aoe.SendMessage("Configure", mods.angerAOEPercent, SendMessageOptions.DontRequireReceiver);
            }

            if (mods.joyExplosionOnHit && owner != null && owner.joyExplosionPrefab != null)
            {
                var vfx = Instantiate(owner.joyExplosionPrefab, hitPos, Quaternion.identity);
                
                var cameraLookAt = vfx.GetComponent<VFXCameraLookAt>();
                if (cameraLookAt != null)
                {
                    cameraLookAt.enabled = false;
                }
                
                AutoDestroyVFX(vfx);
            }
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

