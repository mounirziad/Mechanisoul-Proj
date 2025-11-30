using UnityEngine;
using UnityEngine.AI;

public class EnemySeparation : MonoBehaviour
{
    [Header("Separation Settings")]
    [SerializeField] private float separationRadius = 2.0f;
    [SerializeField] private float separationForce = 1.5f;
    [SerializeField] private LayerMask enemyLayer;
    
    private NavMeshAgent navMeshAgent;
    private Vector3 separationOffset = Vector3.zero;
    
    private const float UPDATE_INTERVAL = 0.1f;
    private float updateTimer = 0f;
    
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        
        if (enemyLayer == 0)
        {
            enemyLayer = LayerMask.GetMask("Character");
        }
        
        updateTimer = Random.Range(0f, UPDATE_INTERVAL);
    }
    
    void Update()
    {
        updateTimer -= Time.deltaTime;
        
        if (updateTimer <= 0f)
        {
            CalculateSeparation();
            updateTimer = UPDATE_INTERVAL;
        }
    }
    
    private void CalculateSeparation()
    {
        if (navMeshAgent == null || !navMeshAgent.enabled)
        {
            separationOffset = Vector3.zero;
            return;
        }
        
        Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, separationRadius, enemyLayer);
        
        if (nearbyEnemies.Length <= 1)
        {
            separationOffset = Vector3.zero;
            return;
        }
        
        Vector3 separation = Vector3.zero;
        int count = 0;
        
        foreach (Collider other in nearbyEnemies)
        {
            if (other.gameObject == gameObject)
                continue;
                
            NavMeshAgent otherAgent = other.GetComponent<NavMeshAgent>();
            if (otherAgent == null || !otherAgent.enabled)
                continue;
            
            Vector3 directionAway = transform.position - other.transform.position;
            float distance = directionAway.magnitude;
            
            if (distance > 0.01f && distance < separationRadius)
            {
                directionAway.y = 0;
                directionAway.Normalize();
                separation += directionAway / distance;
                count++;
            }
        }
        
        if (count > 0)
        {
            separation /= count;
            separation.y = 0;
            separationOffset = separation.normalized * separationForce;
        }
        else
        {
            separationOffset = Vector3.zero;
        }
    }
    
    public Vector3 GetDestinationWithSeparation(Vector3 targetDestination)
    {
        if (separationOffset.magnitude < 0.01f)
            return targetDestination;
        
        Vector3 adjustedDestination = targetDestination + separationOffset;
        
        NavMeshHit hit;
        if (NavMesh.SamplePosition(adjustedDestination, out hit, 2.0f, NavMesh.AllAreas))
        {
            return hit.position;
        }
        
        return targetDestination;
    }
    
    public Vector3 GetSeparationOffset()
    {
        return separationOffset;
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, separationRadius);
        
        if (separationOffset.magnitude > 0.01f)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, separationOffset * 2f);
        }
    }
}
