using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject enemyPrefab;      // The prefab to spawn
    public Transform spawnPoint;        // Where to spawn enemies
    public float respawnDelay = 2f;     // Time after death to respawn

    private GameObject currentEnemy;

    void Start()
    {
        SpawnEnemy();
    }

    void SpawnEnemy()
    {
        currentEnemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

        // Listen for death
        BasicEnemyHealth health = currentEnemy.GetComponent<BasicEnemyHealth>();
        if (health != null)
        {
            health.OnDeath += OnEnemyDeath;
        }
    }

    void OnEnemyDeath()
    {
        // Delay respawn
        Invoke(nameof(SpawnEnemy), respawnDelay);
    }
}
