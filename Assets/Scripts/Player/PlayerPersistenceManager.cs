using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;


public class PlayerPersistenceManager : MonoBehaviour
{
    public static PlayerPersistenceManager Instance { get; private set; }
    
    [Header("Persistence Settings")]
    [SerializeField] private bool persistAcrossScenes = true;
    [SerializeField] private bool respawnAtSpawnPoints = true;
    
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    
    private bool isInitialized = false;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        
        if (persistAcrossScenes)
        {
            Transform root = transform.root;
            DontDestroyOnLoad(root.gameObject);
            isInitialized = true;
            Debug.Log($"PlayerPersistenceManager: Marked {root.name} as DontDestroyOnLoad");
        }
        
        if (playerTransform == null)
        {
            playerTransform = transform.root;
        }
        
        Debug.Log($"PlayerPersistenceManager initialized on {playerTransform.name}");
    }
    
    private void Start()
    {
        Debug.Log($"PlayerPersistenceManager Start - Player position: {playerTransform.position}, Scene: {SceneManager.GetActiveScene().name}");
    }
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!isInitialized || !respawnAtSpawnPoints)
        {
            return;
        }
        
        StartCoroutine(SpawnPlayerAfterSceneReady());
    }
    
    private IEnumerator SpawnPlayerAfterSceneReady()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForFixedUpdate();
        
        MoveToSpawnPoint();
        NotifyCameras();
    }
    
    private void MoveToSpawnPoint()
    {
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawn");
        
        if (spawnPoint != null && playerTransform != null)
        {
            CharacterController characterController = playerTransform.GetComponent<CharacterController>();
            
            if (characterController != null)
            {
                characterController.enabled = false;
            }
            
            playerTransform.position = spawnPoint.transform.position;
            playerTransform.rotation = spawnPoint.transform.rotation;
            
            if (characterController != null)
            {
                characterController.enabled = true;
            }
            
            Rigidbody rb = playerTransform.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            
            PlayerLocomotion playerLocomotion = playerTransform.GetComponent<PlayerLocomotion>();
            if (playerLocomotion != null)
            {
                playerLocomotion.inAirTimer = 0f;
                playerLocomotion.isGrounded = true;
                playerLocomotion.isJumping = false;
            }
            
            Debug.Log($"Player moved to spawn point: {spawnPoint.name} at position {spawnPoint.transform.position}");
        }
        else if (spawnPoint == null)
        {
            Debug.LogWarning($"No spawn point found in scene: {SceneManager.GetActiveScene().name}. Player will remain at current position.");
        }
    }
    
    private void NotifyCameras()
    {
        FreeLookCamera freeLookCam = FindAnyObjectByType<FreeLookCamera>();
        if (freeLookCam != null && playerTransform != null)
        {
            freeLookCam.SetTarget(playerTransform);
        }
        
        AimCamera aimCam = FindAnyObjectByType<AimCamera>();
        if (aimCam != null && playerTransform != null)
        {
            aimCam.SetTarget(playerTransform);
        }
        
        LockOnCamera lockOnCam = FindAnyObjectByType<LockOnCamera>();
        if (lockOnCam != null && playerTransform != null)
        {
            lockOnCam.SetTarget(playerTransform);
        }
        
        ZTargetingCamera zTargetingCam = FindAnyObjectByType<ZTargetingCamera>();
        if (zTargetingCam != null && playerTransform != null)
        {
            zTargetingCam.SetTarget(playerTransform);
        }
        
        CustomCameraController cameraController = playerTransform.GetComponent<CustomCameraController>();
        if (cameraController != null)
        {
            cameraController.RefreshCameraReferences();
        }
        
        Debug.Log("Cameras notified of player reference.");
    }
    
    public Transform GetPlayerTransform()
    {
        return playerTransform;
    }
    
    public void SetSpawnPosition(Vector3 position, Quaternion rotation)
    {
        if (playerTransform != null)
        {
            CharacterController characterController = playerTransform.GetComponent<CharacterController>();
            
            if (characterController != null)
            {
                characterController.enabled = false;
            }
            
            playerTransform.position = position;
            playerTransform.rotation = rotation;
            
            if (characterController != null)
            {
                characterController.enabled = true;
            }
            
            Rigidbody rb = playerTransform.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            
            PlayerLocomotion playerLocomotion = playerTransform.GetComponent<PlayerLocomotion>();
            if (playerLocomotion != null)
            {
                playerLocomotion.inAirTimer = 0f;
                playerLocomotion.isGrounded = true;
                playerLocomotion.isJumping = false;
            }
            
            Debug.Log($"Player manually moved to position: {position}");
        }
    }
    
    public void ForceRespawn()
    {
        StartCoroutine(ForceRespawnCoroutine());
    }
    
    private IEnumerator ForceRespawnCoroutine()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForFixedUpdate();
        
        MoveToSpawnPoint();
        NotifyCameras();
    }
}
