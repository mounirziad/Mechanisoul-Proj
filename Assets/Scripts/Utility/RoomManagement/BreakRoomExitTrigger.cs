using UnityEngine;
using UnityEngine.SceneManagement;

public class BreakRoomExitTrigger : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool isInitialBreakRoom = true;
    [SerializeField] private string targetSceneName;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (isInitialBreakRoom && GameProgressionManager.Instance != null)
            {
                if (!GameProgressionManager.Instance.HasExitedInitialBreakRoom())
                {
                    GameProgressionManager.Instance.SetExitedInitialBreakRoom(true);
                    string sceneToLoad = string.IsNullOrEmpty(targetSceneName) 
                        ? GameProgressionManager.Instance.GetBreakRoomExitScene() 
                        : targetSceneName;
                    
                    Debug.Log($"BreakRoomExitTrigger: Loading first level scene: {sceneToLoad}");
                    SceneManager.LoadScene(sceneToLoad);
                }
                else
                {
                    string sceneToLoad = string.IsNullOrEmpty(targetSceneName) 
                        ? GameProgressionManager.Instance.GetBreakRoomExitScene() 
                        : targetSceneName;
                    
                    Debug.Log($"BreakRoomExitTrigger: Loading scene: {sceneToLoad}");
                    SceneManager.LoadScene(sceneToLoad);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(targetSceneName))
                {
                    Debug.Log($"BreakRoomExitTrigger: Loading scene: {targetSceneName}");
                    SceneManager.LoadScene(targetSceneName);
                }
                else
                {
                    Debug.LogError("BreakRoomExitTrigger: No target scene specified!");
                }
            }
        }
    }
}
