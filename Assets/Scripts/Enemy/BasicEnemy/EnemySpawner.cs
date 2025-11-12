using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public float respawnDelay = 2f;

    private GameObject currentEnemy;
    private BasicEnemyHealth currentEnemyHealth;
    private Coroutine respawnCoroutine;

    void Start()
    {
        SpawnEnemy();
    }

    void SpawnEnemy()
    {
        if (currentEnemy != null)
        {
            UnsubscribeFromCurrentEnemy();
            Destroy(currentEnemy);
        }

        currentEnemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        currentEnemyHealth = currentEnemy.GetComponent<BasicEnemyHealth>();

        if (currentEnemyHealth != null)
        {
            currentEnemyHealth.OnDeath += OnEnemyDeath;
        }
    }

    void OnEnemyDeath()
    {
        UnsubscribeFromCurrentEnemy();

        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
        }

        respawnCoroutine = StartCoroutine(RespawnAfterDelay());
    }

    IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);

        if (this != null && gameObject != null && gameObject.activeInHierarchy)
        {
            SpawnEnemy();
        }

        respawnCoroutine = null;
    }

    void UnsubscribeFromCurrentEnemy()
    {
        if (currentEnemyHealth != null)
        {
            currentEnemyHealth.OnDeath -= OnEnemyDeath;
            currentEnemyHealth = null;
        }
    }

    void OnDestroy()
    {
        UnsubscribeFromCurrentEnemy();

        if (respawnCoroutine != null)
        {
            StopCoroutine(respawnCoroutine);
            respawnCoroutine = null;
        }
    }
}
