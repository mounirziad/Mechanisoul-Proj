using UnityEngine;

public class LightningStrike : MonoBehaviour
{
    public float damage = 18f;
    public float lifetime = 2f;

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerHealth>()?.TakeDamage(damage);
        }
    }
}
