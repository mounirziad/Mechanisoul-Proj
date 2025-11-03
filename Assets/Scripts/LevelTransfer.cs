using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransfer : MonoBehaviour
{
    [Header("Scene Settings")]
    [Tooltip("Name of the scene to load. Leave empty to load the next scene in build settings.")]
    public string sceneName;
    
    [Tooltip("If true, loads the next scene in build order. If false, uses sceneName.")]
    public bool loadNextScene = true;
    
    [Header("Optional Settings")]
    [Tooltip("Delay in seconds before loading the scene")]
    public float loadDelay = 0f;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;
        
        if (other.CompareTag("Player"))
        {
            hasTriggered = true;
            
            if (loadDelay > 0f)
            {
                Invoke(nameof(LoadLevel), loadDelay);
            }
            else
            {
                LoadLevel();
            }
        }
    }

    private void LoadLevel()
    {
        if (loadNextScene)
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            int nextSceneIndex = currentSceneIndex + 1;
            
            if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
            {
                SceneManager.LoadScene(nextSceneIndex);
            }
            else
            {
                Debug.LogWarning("No next scene available in build settings. Reached the last scene.");
            }
        }
        else
        {
            if (!string.IsNullOrEmpty(sceneName))
            {
                SceneManager.LoadScene(sceneName);
            }
            else
            {
                Debug.LogError("Scene name is empty! Please specify a scene name or enable 'Load Next Scene'.");
            }
        }
    }
}
