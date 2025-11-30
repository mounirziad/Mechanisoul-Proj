using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SetupEnemySeparation : MonoBehaviour
{
#if UNITY_EDITOR
    [ContextMenu("Setup Separation Components")]
    public void SetupComponents()
    {
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            agent.avoidancePriority = 50;
            agent.radius = 0.5f;
            Debug.Log($"Configured NavMeshAgent on {gameObject.name}");
            EditorUtility.SetDirty(gameObject);
        }
        
        if (GetComponent<EnemySeparation>() == null)
        {
            EnemySeparation separation = gameObject.AddComponent<EnemySeparation>();
            Debug.Log($"Added EnemySeparation to {gameObject.name}");
            EditorUtility.SetDirty(gameObject);
        }
        
        if (GetComponent<EnemyNavMeshConfig>() == null)
        {
            EnemyNavMeshConfig config = gameObject.AddComponent<EnemyNavMeshConfig>();
            Debug.Log($"Added EnemyNavMeshConfig to {gameObject.name}");
            EditorUtility.SetDirty(gameObject);
        }
        
        Debug.Log($"Enemy separation setup complete for {gameObject.name}!");
    }
    
    [ContextMenu("Setup All Enemies in Scene")]
    public void SetupAllEnemiesInScene()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        int count = 0;
        
        foreach (GameObject enemy in enemies)
        {
            SetupEnemySeparation setup = enemy.GetComponent<SetupEnemySeparation>();
            if (setup != null)
            {
                setup.SetupComponents();
                count++;
            }
        }
        
        Debug.Log($"Setup {count} enemies in the scene!");
    }
#endif
}
