using System.Collections.Generic;
using UnityEngine;

public class RoomEnemyManager : MonoBehaviour
{
    [Header("Room Configuration")]
    [Tooltip("All enemies that belong to this room")]
    public List<GameObject> enemiesInRoom = new List<GameObject>();
    
    [Tooltip("The doors to disable when all enemies are defeated")]
    public GameObject doorToDisable;

    [Header("Debug")]
    [SerializeField] private int remainingEnemies;

    [Header("XP Reward")]
    [SerializeField] private int xpRewardOnClear = 25;
    [SerializeField] private XPManager xpManager;

    private HashSet<BasicEnemyHealth> trackedEnemies = new HashSet<BasicEnemyHealth>();
    private bool doorsDisabled = false;

    private void Start()
    {
        RegisterEnemies();
        UpdateRemainingEnemiesCount();
    }

    private void RegisterEnemies()
    {
        foreach (GameObject enemyObj in enemiesInRoom)
        {
            if (enemyObj == null) continue;

            BasicEnemyHealth enemyHealth = enemyObj.GetComponent<BasicEnemyHealth>();
            if (enemyHealth != null)
            {
                trackedEnemies.Add(enemyHealth);
                enemyHealth.OnDeath += OnEnemyDefeated;
            }
            else
            {
                Debug.LogWarning($"Enemy {enemyObj.name} doesn't have BasicEnemyHealth component!", enemyObj);
            }
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
        if (doorsDisabled) return;

        if (remainingEnemies <= 0)
        {
            DisableDoors();
        }
    }

    private void DisableDoors()
    {
        doorsDisabled = true;

        if (doorToDisable != null)
        {
            doorToDisable.SetActive(false);
            Debug.Log($"Room cleared! Doors {doorToDisable.name} have been disabled.");
        }
        else
        {
            Debug.LogWarning("Door to disable is not assigned in RoomEnemyManager!", this);
        }

        // award XP once when the room is considered cleared
        AwardRoomClearXP();
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
            Debug.LogWarning($"RoomEnemyManager on {gameObject.name}: no XPManager found, cannot award XP.");
            return;
        }

        xpManager.AddXP(xpRewardOnClear);
        Debug.Log($"RoomEnemyManager: awarded {xpRewardOnClear} XP for clearing room {gameObject.name}.");
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

    private void OnDrawGizmosSelected()
    {
        if (enemiesInRoom == null) return;

        Gizmos.color = Color.red;
        foreach (GameObject enemy in enemiesInRoom)
        {
            if (enemy != null)
            {
                Gizmos.DrawLine(transform.position, enemy.transform.position);
                Gizmos.DrawWireSphere(enemy.transform.position, 0.5f);
            }
        }

        if (doorToDisable != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, doorToDisable.transform.position);
            Gizmos.DrawWireCube(doorToDisable.transform.position, Vector3.one);
        }
    }
}
