using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPersistenceDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool logEveryFrame = false;
    [SerializeField] private KeyCode forceRespawnKey = KeyCode.R;
    
    private Transform playerTransform;
    
    private void Start()
    {
        playerTransform = transform.root;
        Debug.Log($"[Debugger] Player root: {playerTransform.name}");
        Debug.Log($"[Debugger] This component on: {gameObject.name}");
        Debug.Log($"[Debugger] Full path: {GetFullPath(transform)}");
        Debug.Log($"[Debugger] Current position: {playerTransform.position}");
        Debug.Log($"[Debugger] Current scene: {SceneManager.GetActiveScene().name}");
        
        GameObject spawnPoint = GameObject.FindGameObjectWithTag("PlayerSpawn");
        if (spawnPoint != null)
        {
            Debug.Log($"[Debugger] Found spawn point: {spawnPoint.name} at {spawnPoint.transform.position}");
        }
        else
        {
            Debug.LogWarning("[Debugger] No spawn point found!");
        }
        
        CharacterController cc = playerTransform.GetComponent<CharacterController>();
        Debug.Log($"[Debugger] Has CharacterController: {cc != null}");
        
        Rigidbody rb = playerTransform.GetComponent<Rigidbody>();
        Debug.Log($"[Debugger] Has Rigidbody: {rb != null}");
    }
    
    private void Update()
    {
        if (logEveryFrame)
        {
            Debug.Log($"[Debugger] Position: {playerTransform.position}");
        }
        
        if (Input.GetKeyDown(forceRespawnKey))
        {
            Debug.Log("[Debugger] Force respawn triggered!");
            if (PlayerPersistenceManager.Instance != null)
            {
                PlayerPersistenceManager.Instance.ForceRespawn();
            }
        }
    }
    
    private string GetFullPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}
