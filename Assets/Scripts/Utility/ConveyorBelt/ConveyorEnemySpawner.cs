using System.Collections;
using UnityEngine;

public class ConveyorEnemySpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    [Tooltip("Enemy prefab to spawn - will be automatically configured for conveyor spawning")]
    public GameObject enemyPrefab;

    [Tooltip("Point where enemies spawn on the conveyor")]
    public Transform spawnPoint;

    [Tooltip("Time between spawns")]
    public float spawnInterval = 5f;

    [Tooltip("Maximum number of active enemies from this spawner")]
    public int maxActiveEnemies = 3;

    [Header("Conveyor Setup")]
    [Tooltip("Ground layer for enemy ground detection")]
    public LayerMask groundLayer = 1 << 9;

    [Tooltip("Should spawning start automatically")]
    public bool autoStart = true;

    private int activeEnemyCount = 0;
    private Coroutine spawnCoroutine;

    void Start()
    {
        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }

        if (autoStart)
        {
            StartSpawning();
        }
    }

    public void StartSpawning()
    {
        if (spawnCoroutine == null)
        {
            spawnCoroutine = StartCoroutine(SpawnRoutine());
        }
    }

    public void StopSpawning()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            if (activeEnemyCount < maxActiveEnemies)
            {
                SpawnEnemy();
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError($"ConveyorEnemySpawner: No enemy prefab assigned on {gameObject.name}");
            return;
        }

        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

        Rigidbody rb = enemy.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = enemy.AddComponent<Rigidbody>();
        }

        rb.isKinematic = false;
        rb.useGravity = true;

        

        activeEnemyCount++;

        Debug.Log($"ConveyorEnemySpawner: Spawned enemy {enemy.name}. Active count: {activeEnemyCount}");
    }

    void OnEnemyDeath(GameObject enemy)
    {
        if (enemy != null)
        {
            activeEnemyCount = Mathf.Max(0, activeEnemyCount - 1);
        }
    }

    void OnDestroy()
    {
        StopSpawning();
    }

    void OnDrawGizmosSelected()
    {
        if (spawnPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
            Gizmos.DrawLine(spawnPoint.position, spawnPoint.position + spawnPoint.forward * 2f);
        }
    }
}
