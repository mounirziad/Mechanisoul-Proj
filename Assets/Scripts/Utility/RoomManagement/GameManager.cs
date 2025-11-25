using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Player Settings")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private bool spawnPlayerOnStart = true;
    [SerializeField] private string playerTag = "Player";
    
    [Header("Scene Management")]
    [SerializeField] private bool persistAcrossScenes = true;
    
    private GameObject currentPlayer;

    public GameObject loseScreen;
    
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
            DontDestroyOnLoad(gameObject);
        }
    }
    
    private void Start()
    {
        if (spawnPlayerOnStart)
        {
            EnsurePlayerExists();
        }
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
        EnsurePlayerExists();
    }
    
    private void EnsurePlayerExists()
    {
        GameObject existingPlayer = GameObject.FindGameObjectWithTag(playerTag);
        
        if (existingPlayer != null)
        {
            currentPlayer = existingPlayer;
            Debug.Log($"Found existing player: {existingPlayer.name}");
            return;
        }
        
        if (playerPrefab != null)
        {
            SpawnPlayer();
        }
        else
        {
            Debug.LogWarning("No player prefab assigned to GameManager and no player found in scene!");
        }
    }
    
    private void SpawnPlayer()
    {
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawn");
        
        Vector3 spawnPosition = Vector3.zero;
        Quaternion spawnRotation = Quaternion.identity;
        
        if (spawnPoint != null)
        {
            spawnPosition = spawnPoint.transform.position;
            spawnRotation = spawnPoint.transform.rotation;
        }
        else
        {
            Debug.LogWarning($"No spawn point found in scene: {SceneManager.GetActiveScene().name}. Spawning player at world origin.");
        }
        
        currentPlayer = Instantiate(playerPrefab, spawnPosition, spawnRotation);
        currentPlayer.name = "Player";
        
        Debug.Log($"Spawned player at {spawnPosition}");
    }
    
    public GameObject GetPlayer()
    {
        return currentPlayer;
    }
    
    public Transform GetPlayerTransform()
    {
        return currentPlayer != null ? currentPlayer.transform : null;
    }

    public void TriggerLoseState()
    {
        Debug.Log("Player lost");
        Time.timeScale = 0f;
        loseScreen.SetActive(true);
    }

    public void Respawn()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
