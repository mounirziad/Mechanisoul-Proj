using UnityEditor;
using UnityEngine;

public class VFXCameraLookAt : MonoBehaviour
{
    public float distanceFromCamera;
    private Camera mainCamera;
    
    void Start()
    {
        mainCamera = Camera.main;
        PositionInScreenSpace();
    }
    
    void Update()
    {
        PositionInScreenSpace();
    }
    
    void PositionInScreenSpace()
    {
        if (mainCamera != null)
        {
            // Convert world position to screen point
            Vector3 screenPos = mainCamera.WorldToScreenPoint(transform.position);
            
            // Create a position a fixed distance from camera in the viewing direction
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(
                new Vector3(screenPos.x, screenPos.y, distanceFromCamera));
            
            transform.position = worldPos;
            
            // Always face camera
            transform.rotation = Quaternion.LookRotation(
                mainCamera.transform.forward, 
                Vector3.up
            );
        }
    }
}
