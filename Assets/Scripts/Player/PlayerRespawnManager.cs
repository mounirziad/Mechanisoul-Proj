using UnityEngine;
using System.Collections;

public class PlayerRespawnManager : MonoBehaviour
{
    public static PlayerRespawnManager Instance { get; private set; }

    [Header("Respawn Settings")]
    [SerializeField] private float respawnHealth = 100f;
    [SerializeField] private bool resetVelocity = true;

    private GameObject player;
    private PlayerHealth playerHealth;
    private PlayerRagdoll playerRagdoll;
    private Rigidbody playerRigidbody;
    private PlayerLocomotion playerLocomotion;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        FindPlayer();
    }

    private void FindPlayer()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            playerRagdoll = player.GetComponent<PlayerRagdoll>();
            playerRigidbody = player.GetComponent<Rigidbody>();
            playerLocomotion = player.GetComponent<PlayerLocomotion>();
        }
        else
        {
            Debug.LogWarning("PlayerRespawnManager: Player not found! Make sure the player has the 'Player' tag.");
        }
    }

    public void RespawnPlayer()
    {
        StartCoroutine(RespawnPlayerCoroutine());
    }

    private IEnumerator RespawnPlayerCoroutine()
    {
        if (player == null)
        {
            FindPlayer();
        }

        if (player == null)
        {
            Debug.LogError("PlayerRespawnManager: Cannot respawn - player not found!");
            yield break;
        }

        GameObject spawnPoint = FindNearestSpawnPoint();
        
        if (spawnPoint == null)
        {
            Debug.LogError("PlayerRespawnManager: No spawn point found in current scene!");
            yield break;
        }

        SpawnPoint spawnPointScript = spawnPoint.GetComponent<SpawnPoint>();
        Vector3 spawnPosition = spawnPointScript != null ? spawnPointScript.GetSpawnPosition() : spawnPoint.transform.position;
        Quaternion spawnRotation = spawnPointScript != null ? spawnPointScript.GetSpawnRotation() : spawnPoint.transform.rotation;

        Debug.Log($"Player current position: {player.transform.position}");
        Debug.Log($"Target spawn position: {spawnPosition}");

        if (playerRagdoll != null)
        {
            playerRagdoll.DeactivateRagdoll();
        }

        yield return null;

        CharacterController characterController = player.GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = false;
        }

        if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = true;
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }

        player.transform.position = spawnPosition;
        player.transform.rotation = spawnRotation;

        yield return new WaitForFixedUpdate();

        if (characterController != null)
        {
            characterController.enabled = true;
        }

        if (playerRigidbody != null && !resetVelocity)
        {
            playerRigidbody.isKinematic = false;
        }
        else if (playerRigidbody != null)
        {
            playerRigidbody.isKinematic = false;
        }

        if (playerLocomotion != null)
        {
            playerLocomotion.inAirTimer = 0f;
            playerLocomotion.isGrounded = true;
            playerLocomotion.isJumping = false;
        }

        if (playerHealth != null)
        {
            playerHealth.Heal(respawnHealth);
        }

        yield return null;

        Debug.Log($"Player respawned at {spawnPoint.name} - Position: {spawnPosition}");
        Debug.Log($"Player actual position after respawn: {player.transform.position}");
    }

    private GameObject FindNearestSpawnPoint()
    {
        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("PlayerSpawn");
        
        Debug.Log($"PlayerRespawnManager: Found {spawnPoints.Length} spawn points with 'PlayerSpawn' tag");
        
        if (spawnPoints.Length == 0)
        {
            return null;
        }

        GameObject defaultSpawn = null;
        GameObject nearestSpawn = null;
        float nearestDistance = float.MaxValue;

        foreach (GameObject spawnPoint in spawnPoints)
        {
            SpawnPoint spawnScript = spawnPoint.GetComponent<SpawnPoint>();
            
            Debug.Log($"Checking spawn point: {spawnPoint.name} at position {spawnPoint.transform.position}");
            
            if (spawnScript != null && spawnScript.IsDefaultSpawn())
            {
                defaultSpawn = spawnPoint;
                Debug.Log($"Found default spawn: {spawnPoint.name}");
            }

            if (player != null)
            {
                float distance = Vector3.Distance(player.transform.position, spawnPoint.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestSpawn = spawnPoint;
                }
            }
        }

        GameObject selectedSpawn = defaultSpawn != null ? defaultSpawn : nearestSpawn;
        Debug.Log($"Selected spawn point: {(selectedSpawn != null ? selectedSpawn.name : "NULL")}");
        
        return selectedSpawn;
    }
}
