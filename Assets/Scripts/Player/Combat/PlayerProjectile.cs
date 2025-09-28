using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    public float speed = 20f;
    public float lifetime = 3f;
    public float damage = 10f;

    private Vector3 direction;

    public void Initialize(Vector3 shootDirection, float projectileDamage)
    {
        direction = shootDirection.normalized;
        damage = projectileDamage;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Try to get the enemy health component
        BasicEnemyHealth enemyHealth = other.GetComponentInParent<BasicEnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(damage, direction);
        }

        Destroy(gameObject); // Destroy projectile on impact
    }
}