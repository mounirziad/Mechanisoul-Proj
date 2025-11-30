using UnityEngine;
using UnityEngine.AI;
using UnityEditor;

public class EnemySeparationSetupEditor : EditorWindow
{
    [MenuItem("Tools/Enemy Setup/Configure Enemy Separation")]
    public static void ShowWindow()
    {
        GetWindow<EnemySeparationSetupEditor>("Enemy Separation Setup");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Enemy Separation Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "This tool will configure all enemies in the scene with proper separation behavior:\n\n" +
            "1. Enable NavMeshAgent obstacle avoidance\n" +
            "2. Add EnemySeparation component\n" +
            "3. Add EnemyNavMeshConfig component",
            MessageType.Info
        );
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Setup All Enemies in Scene", GUILayout.Height(30)))
        {
            SetupAllEnemies();
        }
        
        GUILayout.Space(10);
        
        if (GUILayout.Button("Setup Selected Enemy", GUILayout.Height(30)))
        {
            SetupSelectedEnemy();
        }
    }
    
    private void SetupAllEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        int count = 0;
        
        foreach (GameObject enemy in enemies)
        {
            if (SetupEnemy(enemy))
            {
                count++;
            }
        }
        
        Debug.Log($"Successfully configured {count} enemies with separation behavior!");
        EditorUtility.DisplayDialog("Setup Complete", $"Configured {count} enemies!", "OK");
    }
    
    private void SetupSelectedEnemy()
    {
        if (Selection.activeGameObject == null)
        {
            EditorUtility.DisplayDialog("No Selection", "Please select an enemy GameObject first.", "OK");
            return;
        }
        
        if (SetupEnemy(Selection.activeGameObject))
        {
            Debug.Log($"Successfully configured {Selection.activeGameObject.name}");
            EditorUtility.DisplayDialog("Setup Complete", $"Configured {Selection.activeGameObject.name}!", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Setup Failed", "Could not configure the selected GameObject.", "OK");
        }
    }
    
    private bool SetupEnemy(GameObject enemy)
    {
        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            Debug.LogWarning($"{enemy.name} does not have a NavMeshAgent component!");
            return false;
        }
        
        Undo.RecordObject(agent, "Configure NavMeshAgent");
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        agent.avoidancePriority = 50;
        
        if (enemy.GetComponent<EnemySeparation>() == null)
        {
            Undo.AddComponent<EnemySeparation>(enemy);
        }
        
        if (enemy.GetComponent<EnemyNavMeshConfig>() == null)
        {
            Undo.AddComponent<EnemyNavMeshConfig>(enemy);
        }
        
        EditorUtility.SetDirty(enemy);
        
        return true;
    }
}
