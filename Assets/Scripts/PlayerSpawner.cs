using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : MonoBehaviour
{
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject player = GameObject.FindWithTag("Player");
        GameObject spawn = GameObject.FindWithTag("PlayerSpawn");

        if (player && spawn)
        {
            player.transform.position = spawn.transform.position;
            player.transform.rotation = spawn.transform.rotation;
        }
    }
}
