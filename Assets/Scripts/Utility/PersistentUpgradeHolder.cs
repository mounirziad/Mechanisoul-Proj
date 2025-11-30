using UnityEngine;

public class PersistentUpgradeHolder : MonoBehaviour
{
    public static PersistentUpgradeHolder Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log($"PersistentUpgradeHolder: Marked {gameObject.name} as DontDestroyOnLoad");
        }
        else
        {
            Debug.Log($"PersistentUpgradeHolder: Duplicate found, destroying {gameObject.name}");
            Destroy(gameObject);
        }
    }
}