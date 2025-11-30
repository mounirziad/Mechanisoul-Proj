using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyNavMeshConfig : MonoBehaviour
{
    [Header("Avoidance Settings")]
    [SerializeField] private ObstacleAvoidanceType avoidanceQuality = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
    [SerializeField] private int avoidancePriority = 50;
    [SerializeField] private float radius = 0.5f;
    
    void Start()
    {
        ConfigureNavMeshAgent();
    }
    
    private void ConfigureNavMeshAgent()
    {
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        
        if (agent != null)
        {
            agent.obstacleAvoidanceType = avoidanceQuality;
            agent.avoidancePriority = avoidancePriority;
            agent.radius = radius;
        }
    }
    
    void OnValidate()
    {
        if (Application.isPlaying)
        {
            ConfigureNavMeshAgent();
        }
    }
}
