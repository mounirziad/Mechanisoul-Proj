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
        if (other.CompareTag("Player"))
        {
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
        }
        else
        {
            if (other.CompareTag("Enemy") || (other.transform.root != null && other.transform.root.CompareTag("Enemy")))
            {
                hitPos = other.ClosestPoint(transform.position);
                hitEnemy = true;
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

