using UnityEngine;
using UnityEditor;
using UnityEngine.AI;

public class FixEnemySetup : EditorWindow
{
    [MenuItem("Tools/Enemy Setup/Fix All Enemies in Scene")]
    public static void FixAllEnemiesInScene()
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        int fixedCount = 0;

        foreach (GameObject obj in allObjects)
        {
            if (obj.CompareTag("Enemy"))
            {
                bool wasFixed = FixEnemy(obj);
                if (wasFixed)
                {
                    fixedCount++;
                }
            }
        }

        Debug.Log($"Fixed {fixedCount} enemies in the scene.");
    }

    private static bool FixEnemy(GameObject enemy)
    {
        bool changed = false;

        NavMeshAgent navAgent = enemy.GetComponent<NavMeshAgent>();
        if (navAgent != null)
        {
            if (navAgent.radius < 0.6f)
            {
                navAgent.radius = 0.6f;
                changed = true;
            }
            
            if (navAgent.obstacleAvoidanceType != ObstacleAvoidanceType.HighQualityObstacleAvoidance)
            {
                navAgent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
                changed = true;
            }
        }

        EnemySeparation[] separations = enemy.GetComponents<EnemySeparation>();
        
        if (separations.Length > 1)
        {
            Debug.Log($"Removing duplicate EnemySeparation components from {enemy.name}");
            for (int i = 1; i < separations.Length; i++)
            {
                DestroyImmediate(separations[i]);
            }
            changed = true;
        }
        else if (separations.Length == 0)
        {
            enemy.AddComponent<EnemySeparation>();
            Debug.Log($"Added EnemySeparation to {enemy.name}");
            changed = true;
        }

        EnemySeparation separation = enemy.GetComponent<EnemySeparation>();
        if (separation != null)
        {
            SerializedObject so = new SerializedObject(separation);
            SerializedProperty radiusProp = so.FindProperty("separationRadius");
            SerializedProperty forceProp = so.FindProperty("separationForce");
            SerializedProperty layerProp = so.FindProperty("enemyLayer");

            if (radiusProp.floatValue < 3.5f)
            {
                radiusProp.floatValue = 3.5f;
                changed = true;
            }

            if (forceProp.floatValue < 5.0f)
            {
                forceProp.floatValue = 5.0f;
                changed = true;
            }

            if (layerProp.intValue == 0)
            {
                layerProp.intValue = LayerMask.GetMask("Character");
                changed = true;
            }

            so.ApplyModifiedProperties();
        }

        if (changed)
        {
            EditorUtility.SetDirty(enemy);
        }

        return changed;
    }
}
