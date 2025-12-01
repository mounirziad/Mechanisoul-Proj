using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RoomTriggerEnemyDetector : MonoBehaviour
{
    [Header("Room Configuration")]
    [Tooltip("The door to open when all enemies are defeated")]
    public GameObject doorToDisable;

    [Tooltip("Tag to identify enemies (default: Enemy)")]
    public string enemyTag = "Enemy";

    [Header("Auto-Detection")]
    [Tooltip("If true, automatically detect enemies on Start. If false, use manual list.")]
    public bool autoDetectEnemies = true;

    [Tooltip("Manually assign enemies (used if autoDetectEnemies is false)")]
    public List<GameObject> manualEnemyList = new List<GameObject>();

    [Header("Door Behavior")]
    [Tooltip("Enable doors at start (they'll open when enemies are defeated)")]
    public bool enableDoorsAtStart = true;

    [Header("Combat Camera")]
    [Tooltip("The combat camera controller to trigger when player enters the room")]
    public CombatCameraController combatCameraController;

    [Header("Debug")]
    [SerializeField] private int totalEnemies;
    [SerializeField] private int remainingEnemies;
    [SerializeField] private bool roomCleared = false;

    [Header("XP Reward")]
    [SerializeField] private int xpRewardOnClear = 150;
    [SerializeField] private XPManager xpManager;


    private HashSet<BasicEnemyHealth> trackedEnemies = new HashSet<BasicEnemyHealth>();
    private Collider roomTrigger;
    private bool playerInRoom = false;

    private void Awake()
    {
        roomTrigger = GetComponent<Collider>();
        if (roomTrigger != null)
        {
            roomTrigger.isTrigger = true;
        }

        if (xpManager == null && XPManager.Instance != null)
        {
            xpManager = XPManager.Instance;
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
        // mark room as cleared for XP / camera logic
        if (!roomCleared)
        {
            roomCleared = true;
        }

        if (doorToDisable != null)
        {
            DoorNavMeshControl doorControl = doorToDisable.GetComponent<DoorNavMeshControl>();
            if (doorControl != null)
            {
                doorControl.OpenDoor();
                Debug.Log($"Room {gameObject.name} cleared! Door {doorToDisable.name} opened via DoorNavMeshControl.");
            }
            else
            {
                Animator doorAnimator = doorToDisable.GetComponent<Animator>();
                if (doorAnimator != null)
                {
                    doorAnimator.SetBool("IsDoorOpen", true);
                    Debug.Log($"Room {gameObject.name} cleared! Door {doorToDisable.name} animation triggered.");
                }
                else
                {
                    Debug.LogWarning($"No DoorNavMeshControl or Animator found on door: {doorToDisable.name}");
                }
            }
        }
        else
        {
            Debug.Log($"Room {gameObject.name} cleared with no door assigned.");
        }

        // give XP exactly once per clear
        AwardRoomClearXP();

        if (combatCameraController != null && playerInRoom)
        {
            combatCameraController.ExitCombatZone();
        }

        if (doorToDisable != null)
        {
            DoorOutline doorOutline = doorToDisable.GetComponent<DoorOutline>();
            if (doorOutline != null)
            {
                doorOutline.EnableOutline();
            }
        }
    }

    private void AwardRoomClearXP()
    {
        if (xpRewardOnClear <= 0) return;

        if (xpManager == null)
        {
            xpManager = FindObjectOfType<XPManager>();
        }

        if (xpManager == null)
        {
            Debug.LogWarning($"Room {gameObject.name} cleared, but no XPManager found. No XP awarded.");
            return;
        }

        xpManager.AddXP(xpRewardOnClear);
        Debug.Log($"Room {gameObject.name}: awarded {xpRewardOnClear} XP for clearing the room.");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRoom = true;

            if (!roomCleared && combatCameraController != null)
            {
                combatCameraController.EnterCombatZone();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRoom = false;

            if (combatCameraController != null)
            {
                combatCameraController.ExitCombatZone();
            }
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
