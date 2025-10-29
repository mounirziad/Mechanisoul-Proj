using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RoomTriggerEnemyDetector : MonoBehaviour
{
    [Header("Room Configuration")]
    [Tooltip("The doors to disable when all enemies are defeated")]
    public GameObject doorToDisable;

    [Tooltip("Tag to identify enemies (default: Enemy)")]
    public string enemyTag = "Enemy";

    [Header("Auto-Detection")]
    [Tooltip("If true, automatically detect enemies on Start. If false, use manual list.")]
    public bool autoDetectEnemies = true;

    [Tooltip("Manually assign enemies (used if autoDetectEnemies is false)")]
    public List<GameObject> manualEnemyList = new List<GameObject>();

    [Header("Door Behavior")]
    [Tooltip("Enable doors at start (they'll be disabled when enemies are defeated)")]
    public bool enableDoorsAtStart = true;

    [Header("Debug")]
    [SerializeField] private int totalEnemies;
    [SerializeField] private int remainingEnemies;
    [SerializeField] private bool roomCleared = false;

    private HashSet<BasicEnemyHealth> trackedEnemies = new HashSet<BasicEnemyHealth>();
    private Collider roomTrigger;

    private void Awake()
    {
        roomTrigger = GetComponent<Collider>();
        if (roomTrigger != null)
        {
            roomTrigger.isTrigger = true;
        }
    }

    private void Start()
    {
        if (enableDoorsAtStart && doorToDisable != null)
        {
            doorToDisable.SetActive(true);
        }

        if (autoDetectEnemies)
        {
            DetectEnemiesInTrigger();
        }
        else
        {
            RegisterManualEnemies();
        }

        totalEnemies = trackedEnemies.Count;
        UpdateRemainingEnemiesCount();
    }

    private void DetectEnemiesInTrigger()
    {
        GameObject[] allEnemies = GameObject.FindGameObjectsWithTag(enemyTag);
        
        foreach (GameObject enemyObj in allEnemies)
        {
            if (IsEnemyInTrigger(enemyObj))
            {
                RegisterEnemy(enemyObj);
            }
        }

        Debug.Log($"Room {gameObject.name}: Auto-detected {trackedEnemies.Count} enemies");
    }

    private bool IsEnemyInTrigger(GameObject enemyObj)
    {
        if (roomTrigger == null) return false;

        Collider enemyCollider = enemyObj.GetComponent<Collider>();
        if (enemyCollider != null)
        {
            return roomTrigger.bounds.Intersects(enemyCollider.bounds);
        }
        else
        {
            return roomTrigger.bounds.Contains(enemyObj.transform.position);
        }
    }

    private void RegisterManualEnemies()
    {
        foreach (GameObject enemyObj in manualEnemyList)
        {
            RegisterEnemy(enemyObj);
        }
    }

    private void RegisterEnemy(GameObject enemyObj)
    {
        if (enemyObj == null) return;

        BasicEnemyHealth enemyHealth = enemyObj.GetComponent<BasicEnemyHealth>();
        if (enemyHealth != null && !trackedEnemies.Contains(enemyHealth))
        {
            trackedEnemies.Add(enemyHealth);
            enemyHealth.OnDeath += OnEnemyDefeated;
        }
    }

    private void OnEnemyDefeated()
    {
        UpdateRemainingEnemiesCount();
        CheckAllEnemiesDefeated();
    }

    private void UpdateRemainingEnemiesCount()
    {
        remainingEnemies = 0;
        foreach (BasicEnemyHealth enemy in trackedEnemies)
        {
            if (enemy != null && enemy.currentHealth > 0)
            {
                remainingEnemies++;
            }
        }
    }

    private void CheckAllEnemiesDefeated()
    {
        if (roomCleared) return;

        if (remainingEnemies <= 0 && totalEnemies > 0)
        {
            DisableDoors();
        }
    }

    private void DisableDoors()
    {
        if (doorToDisable != null)
        {
            doorToDisable.SetActive(false);
            roomCleared = true;
            Debug.Log($"Room {gameObject.name} cleared! Doors {doorToDisable.name} have been disabled.");
        }
    }

    private void OnDestroy()
    {
        foreach (BasicEnemyHealth enemy in trackedEnemies)
        {
            if (enemy != null)
            {
                enemy.OnDeath -= OnEnemyDefeated;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Collider trigger = GetComponent<Collider>();
        if (trigger != null)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
            Gizmos.matrix = transform.localToWorldMatrix;
            
            if (trigger is BoxCollider boxCollider)
            {
                Gizmos.DrawCube(boxCollider.center, boxCollider.size);
            }
            else if (trigger is SphereCollider sphereCollider)
            {
                Gizmos.DrawSphere(sphereCollider.center, sphereCollider.radius);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Collider trigger = GetComponent<Collider>();
        if (trigger != null)
        {
            Gizmos.color = Color.red;
            Gizmos.matrix = transform.localToWorldMatrix;
            
            if (trigger is BoxCollider boxCollider)
            {
                Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
            }
            else if (trigger is SphereCollider sphereCollider)
            {
                Gizmos.DrawWireSphere(sphereCollider.center, sphereCollider.radius);
            }
        }

        if (doorToDisable != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, doorToDisable.transform.position);
            Gizmos.DrawWireCube(doorToDisable.transform.position, Vector3.one * 2f);
        }
    }
}
