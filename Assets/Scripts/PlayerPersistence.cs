using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPersistence : MonoBehaviour
{
    private static PlayerPersistence instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
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
        PlayerLocomotion locomotion = GetComponent<PlayerLocomotion>();
        if (locomotion != null)
        {
            locomotion.RefreshReferences();
        }
    }
}
