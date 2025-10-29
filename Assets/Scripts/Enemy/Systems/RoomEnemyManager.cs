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
        if (doorToDisable != null)
        {
            doorToDisable.SetActive(false);
            doorsDisabled = true;
            Debug.Log($"Room cleared! Doors {doorToDisable.name} have been disabled.");
        }
        else
        {
            Debug.LogWarning("Door to disable is not assigned in RoomEnemyManager!", this);
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
