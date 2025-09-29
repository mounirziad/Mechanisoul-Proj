using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 3f;
    public float damage = 10f;

    private Vector3 direction;
    private RangedModifiers mods;
    private PlayerCombat owner;

    public void Initialize(Vector3 shootDirection, float projectileDamage, RangedModifiers rangedMods, PlayerCombat owningCombat = null)
    {
        direction = shootDirection.normalized;
        damage = projectileDamage; // Joy damage already applied by PlayerCombat
        mods = rangedMods;
        owner = owningCombat;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        Vector3 hitPos = transform.position;

        var enemyHealth = other.GetComponentInParent<BasicEnemyHealth>();
        if (enemyHealth)
        {
            enemyHealth.TakeDamage(damage, direction);
            hitPos = enemyHealth.transform.position;
        }

        // ANGER: explosion/DoT on impact
        if (mods.angerExplosionOnHit && owner != null && owner.angerExplosionPrefab != null)
        {
            var aoe = Instantiate(owner.angerExplosionPrefab, hitPos, Quaternion.identity);
            var dot = aoe.GetComponent<FireDoTZone>(); // expected component
            if (dot != null) dot.Configure(Mathf.Max(0f, mods.angerAOEPercent));
        }

        Destroy(gameObject);
    }
}
