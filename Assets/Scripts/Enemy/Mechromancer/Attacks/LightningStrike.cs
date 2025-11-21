using UnityEngine;

public class LightningStrike : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 25f;
    [SerializeField] private float maxLifetime = 3f;
    [SerializeField] private float hitRadius = 1.5f;

    private Vector3 targetPosition;
    private bool hasTarget = false;
    private float lifetime;

    private LightningController controller;

    public void Initialize(Vector3 target, LightningController controllerRef)
    {
        targetPosition = target;
        controller = controllerRef;
        hasTarget = true;
    }

    private void Update()
    {
        if (!hasTarget) return;

        lifetime += Time.deltaTime;

        if (lifetime > maxLifetime)
        {
            NotifyHit();
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < hitRadius)
        {
            NotifyHit();
        }
    }

    private void NotifyHit()
    {
        if (controller != null)
        {
            controller.OnStrike(targetPosition);
        }

        Destroy(gameObject);
    }
}
