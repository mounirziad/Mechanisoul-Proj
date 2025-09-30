using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraPlayerAssigner : MonoBehaviour
{
    public CinemachineCamera freeLookCamera;

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
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null && freeLookCamera != null)
        {
            freeLookCamera.Follow = player.transform;
            freeLookCamera.LookAt = player.transform;
        }
    }
}
