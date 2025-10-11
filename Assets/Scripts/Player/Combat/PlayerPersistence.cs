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
        StartCoroutine(MoveToSpawnPoint());
    }

    private System.Collections.IEnumerator MoveToSpawnPoint()
    {
        // Wait one frame so all scene objects load
        yield return null;

        GameObject spawnPoint = GameObject.FindWithTag("PlayerSpawn");
        if (spawnPoint != null)
        {
            transform.position = spawnPoint.transform.position;
            transform.rotation = spawnPoint.transform.rotation;
        }
    }
}
