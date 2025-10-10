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
        Vector3 hitPos = transform.position;
        bool hitEnemy = false;

        // Prefer health component if present
        var enemyHealth = other.GetComponentInParent<BasicEnemyHealth>();
        if (enemyHealth)
        {
            enemyHealth.TakeDamage(damage, direction);
            hitPos = enemyHealth.transform.position;
            hitEnemy = true;
        }
        else
        {
            // Fallback: tag check
            if (other.CompareTag("Enemy") || (other.transform.root != null && other.transform.root.CompareTag("Enemy")))
            {
                hitEnemy = true;
                hitPos = other.ClosestPoint(transform.position);
            }
        }

        // ANGER (gameplay): DoT AoE on impact
        if (mods.angerExplosionOnHit && owner != null && owner.angerExplosionPrefab != null)
        {
            var aoe = Instantiate(owner.angerExplosionPrefab, hitPos, Quaternion.identity);
            var dot = aoe.GetComponent<FireDoTZone>();
            if (dot != null) dot.Configure(Mathf.Max(0f, mods.angerAOEPercent));
            else aoe.SendMessage("Configure", mods.angerAOEPercent, SendMessageOptions.DontRequireReceiver);
        }

        // JOY (visual): pop VFX on enemy contact when synergy is active
        if (hitEnemy && mods.joyExplosionOnHit && owner != null && owner.joyExplosionPrefab != null)
        {
            var vfx = Instantiate(owner.joyExplosionPrefab, hitPos, Quaternion.identity);
            AutoDestroyVFX(vfx);
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
