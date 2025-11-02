using UnityEngine;

public class LightningStrike : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 25f;
    [SerializeField] private float maxLifetime = 3f;
    [SerializeField] private float hitRadius = 1.5f;

    private Vector3 targetPosition;
    private bool hasTarget;
    private float lifetime;

    public void Initialize(Vector3 target)
    {
        targetPosition = target;
        hasTarget = true;
    }

    private void Update()
    {
        if (!hasTarget) return;

        lifetime += Time.deltaTime;

        if (lifetime > maxLifetime)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < hitRadius)
        {
            LightningDamage();
        }
    }

    private void LightningDamage()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, hitRadius);
        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerHealth health = hit.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.TakeDamage(15f);
                    Debug.Log("Player hit by lightning");
                }
            }
        }

        //Play sound or impact VFX
        Destroy(gameObject);
    }
}
