using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed = 50f;
    public float damage = 10f;
    public float lifeTime = 5f;

    private float spawnTime;

    private void Start()
    {
        spawnTime = Time.time;
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            return;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Character"))
        {
            return;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("PickUp"))
        {
            return;
        }

        if (other.CompareTag("Weapon"))
        {
            return;
        }

        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}