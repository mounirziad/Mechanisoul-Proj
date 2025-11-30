using UnityEngine;

public class DebugLineOfSight : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private Color lineOfSightColor = Color.green;
    [SerializeField] private Color blockedSightColor = Color.red;

    private AiAgent agent;

    void Start()
    {
        agent = GetComponent<AiAgent>();
    }

    void Update()
    {
        if (!showDebugInfo || agent == null)
            return;

        Transform target = agent.GetCurrentTarget();
        if (target == null)
            return;

        Vector3 origin = transform.position + Vector3.up * 1.5f;
        Vector3 targetPos = target.position + Vector3.up * 1.5f;
        Vector3 direction = targetPos - origin;
        float distance = direction.magnitude;

        LayerMask obstacleMask = LayerMask.GetMask("Default", "Floors");

        if (Physics.Raycast(origin, direction.normalized, out RaycastHit hit, distance, obstacleMask, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawRay(origin, direction.normalized * hit.distance, blockedSightColor);
            
            if (showDebugInfo)
            {
                Debug.Log($"{gameObject.name}: Line of sight BLOCKED by {hit.collider.gameObject.name} on layer {LayerMask.LayerToName(hit.collider.gameObject.layer)} at distance {hit.distance:F2}m");
            }
        }
        else
        {
            Debug.DrawRay(origin, direction.normalized * distance, lineOfSightColor);
        }
    }

    void OnDrawGizmos()
    {
        if (!showDebugInfo || agent == null)
            return;

        Transform target = agent.GetCurrentTarget();
        if (target == null)
            return;

        Vector3 origin = transform.position + Vector3.up * 1.5f;
        Vector3 targetPos = target.position + Vector3.up * 1.5f;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, 0.2f);
        Gizmos.DrawLine(origin, targetPos);
    }
}
