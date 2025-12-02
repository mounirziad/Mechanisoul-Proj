using UnityEngine;

public class LightningStrike : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float speed = 25f;
    [SerializeField] private float groundOffset = 8f;

    private Vector3 targetPosition;
    private bool hasTarget = false;

    private LightningController controller;

    public void Initialize(Vector3 target, LightningController controllerRef)
    {
        controller = controllerRef;
        targetPosition = target;

        transform.position = target + Vector3.up * groundOffset;

        hasTarget = true;
    }

    private void Update()
    {
        if (!hasTarget) return;

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.25f)
        {
            controller?.OnStrike(targetPosition);
            Destroy(gameObject);
        }
    }
}
