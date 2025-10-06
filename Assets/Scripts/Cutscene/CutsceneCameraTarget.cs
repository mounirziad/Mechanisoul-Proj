using UnityEngine;

public class CutsceneCameraTarget : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private string targetName = "Camera Target";
    [SerializeField] private Color gizmoColor = Color.yellow;
    [SerializeField] private float gizmoSize = 1f;
    [SerializeField] private bool showFieldOfView = true;
    [SerializeField] private float fieldOfView = 60f;
    
    [Header("Preview")]
    [SerializeField] private bool previewInSceneView = true;
    [SerializeField] private Camera previewCamera;
    
    public string TargetName => targetName;
    public float FieldOfView => fieldOfView;
    
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(targetName))
        {
            targetName = $"Camera Target {name}";
        }
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        
        // Draw camera position indicator
        Gizmos.DrawWireCube(transform.position, Vector3.one * gizmoSize);
        
        // Draw forward direction
        Gizmos.color = gizmoColor * 0.8f;
        Gizmos.DrawRay(transform.position, transform.forward * gizmoSize * 2f);
        
        // Draw field of view cone if enabled
        if (showFieldOfView)
        {
            DrawFieldOfViewGizmo();
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = gizmoColor;
        
        // Draw more detailed gizmos when selected
        Gizmos.DrawWireCube(transform.position, Vector3.one * gizmoSize * 1.5f);
        
        // Draw coordinate axes
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.right * gizmoSize);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.up * gizmoSize);
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * gizmoSize);
        
        if (showFieldOfView)
        {
            DrawFieldOfViewGizmo();
        }
    }
    
    private void DrawFieldOfViewGizmo()
    {
        float halfFOV = fieldOfView * 0.5f;
        float distance = gizmoSize * 3f;
        
        // Calculate field of view corners
        Vector3 rightDir = Quaternion.AngleAxis(halfFOV, transform.up) * transform.forward;
        Vector3 leftDir = Quaternion.AngleAxis(-halfFOV, transform.up) * transform.forward;
        Vector3 upDir = Quaternion.AngleAxis(halfFOV, transform.right) * transform.forward;
        Vector3 downDir = Quaternion.AngleAxis(-halfFOV, transform.right) * transform.forward;
        
        Gizmos.color = gizmoColor * 0.3f;
        
        // Draw FOV lines
        Gizmos.DrawRay(transform.position, rightDir * distance);
        Gizmos.DrawRay(transform.position, leftDir * distance);
        Gizmos.DrawRay(transform.position, upDir * distance);
        Gizmos.DrawRay(transform.position, downDir * distance);
        
        // Draw FOV frame
        Vector3 rightPoint = transform.position + rightDir * distance;
        Vector3 leftPoint = transform.position + leftDir * distance;
        Vector3 upPoint = transform.position + upDir * distance;
        Vector3 downPoint = transform.position + downDir * distance;
        
        Gizmos.DrawLine(rightPoint, upPoint);
        Gizmos.DrawLine(upPoint, leftPoint);
        Gizmos.DrawLine(leftPoint, downPoint);
        Gizmos.DrawLine(downPoint, rightPoint);
    }
    
    [ContextMenu("Set As Main Camera Position")]
    public void SetAsMainCameraPosition()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            transform.position = mainCam.transform.position;
            transform.rotation = mainCam.transform.rotation;
            fieldOfView = mainCam.fieldOfView;
            
            Debug.Log($"Set {targetName} to Main Camera position");
        }
        else
        {
            Debug.LogWarning("Main Camera not found!");
        }
    }
    
    [ContextMenu("Move Main Camera Here")]
    public void MoveMainCameraHere()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.transform.position = transform.position;
            mainCam.transform.rotation = transform.rotation;
            mainCam.fieldOfView = fieldOfView;
            
            Debug.Log($"Moved Main Camera to {targetName} position");
        }
        else
        {
            Debug.LogWarning("Main Camera not found!");
        }
    }
    
    [ContextMenu("Preview This Shot")]
    public void PreviewShot()
    {
        if (previewCamera == null)
        {
            previewCamera = Camera.main;
        }
        
        if (previewCamera != null)
        {
            previewCamera.transform.position = transform.position;
            previewCamera.transform.rotation = transform.rotation;
            previewCamera.fieldOfView = fieldOfView;
            
            Debug.Log($"Previewing shot: {targetName}");
        }
    }
}