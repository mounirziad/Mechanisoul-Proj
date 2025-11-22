using UnityEngine;

public class CameraManagerAdapter : MonoBehaviour
{
    private CustomCameraController customCameraController;
    private ThirdPersonAimCameraManager oldCameraManager;

    private void Awake()
    {
        customCameraController = GetComponent<CustomCameraController>();
        oldCameraManager = GetComponent<ThirdPersonAimCameraManager>();
        
        if (customCameraController != null && oldCameraManager != null)
        {
            Debug.LogWarning("Both CustomCameraController and ThirdPersonAimCameraManager are present. CustomCameraController will take priority. Consider removing ThirdPersonAimCameraManager.");
        }
    }

    public Vector3 GetAimTarget()
    {
        if (customCameraController != null)
        {
            return customCameraController.GetAimTarget();
        }
        else if (oldCameraManager != null)
        {
            return oldCameraManager.GetAimTarget();
        }
        return Vector3.zero;
    }

    public Vector3 GetAimDirection()
    {
        if (customCameraController != null)
        {
            return customCameraController.GetAimDirection();
        }
        else if (oldCameraManager != null)
        {
            return oldCameraManager.GetAimDirection();
        }
        return Vector3.forward;
    }

    public bool IsAimCameraActive()
    {
        if (customCameraController != null)
        {
            return customCameraController.IsAimCameraActive();
        }
        else if (oldCameraManager != null)
        {
            return oldCameraManager.IsAimCameraActive();
        }
        return false;
    }
    
    public Transform GetAimTargetTransform()
    {
        if (customCameraController != null)
        {
            return customCameraController.GetAimTargetTransform();
        }
        return null;
    }

    public static CameraManagerAdapter GetFromPlayer(GameObject player)
    {
        CameraManagerAdapter adapter = player.GetComponent<CameraManagerAdapter>();
        if (adapter == null)
        {
            adapter = player.AddComponent<CameraManagerAdapter>();
        }
        return adapter;
    }
    
    public bool UsingCustomCameraSystem()
    {
        return customCameraController != null;
    }
    
    public bool UsingCinemachineSystem()
    {
        return oldCameraManager != null && customCameraController == null;
    }
}
