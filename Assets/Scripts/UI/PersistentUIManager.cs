using UnityEngine;

public class PersistentUIManager : MonoBehaviour
{
    public static PersistentUIManager Instance { get; private set; }
    
    [Header("Settings")]
    [SerializeField] private bool persistAcrossScenes = true;
    
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
            Debug.Log($"PersistentUIManager: Marked {gameObject.name} as DontDestroyOnLoad");
        }
    }
    
    public T FindUIComponent<T>() where T : Component
    {
        return GetComponentInChildren<T>(true);
    }
}
