using UnityEngine;

public class StableMovementReference : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LockOnCamera lockOnCamera;
    [SerializeField] private Transform playerTransform;
    
    [Header("Stability Settings")]
    [SerializeField] private float smoothSpeed = 15f;
    [SerializeField] private float minUpdateInterval = 0.05f;
    [SerializeField] private bool freezeDuringCollision = true;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugGizmos = false;
    
    private Vector3 stableForward;
    private Vector3 stableRight;
    private float lastUpdateTime;
    private bool isInitialized = false;
    
    private void Start()
    {
        if (lockOnCamera == null)
        {
            lockOnCamera = FindFirstObjectByType<LockOnCamera>();
        }
        
        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
        
        InitializeStableVectors();
    }
    
    private void InitializeStableVectors()
    {
        if (lockOnCamera != null)
        {
            Vector3 cameraForward = lockOnCamera.transform.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();
            
            stableForward = cameraForward;
            stableRight = Vector3.Cross(Vector3.up, stableForward).normalized;
        }
        else
        {
            stableForward = Vector3.forward;
            stableRight = Vector3.right;
        }
        
        isInitialized = true;
    }
    
    private void LateUpdate()
    {
        if (!isInitialized)
        {
            InitializeStableVectors();
            return;
        }
        
        if (lockOnCamera == null) return;
        
        bool inCollision = lockOnCamera.IsInCollision();
        
        if (freezeDuringCollision && inCollision)
        {
            return;
        }
        
        float timeSinceLastUpdate = Time.time - lastUpdateTime;
        if (timeSinceLastUpdate < minUpdateInterval)
        {
            return;
        }
        
        Vector3 cameraForward = lockOnCamera.transform.forward;
        cameraForward.y = 0f;
        
        if (cameraForward.sqrMagnitude < 0.01f)
        {
            return;
        }
        
        cameraForward.Normalize();
        Vector3 cameraRight = Vector3.Cross(Vector3.up, cameraForward).normalized;
        
        stableForward = Vector3.Slerp(stableForward, cameraForward, smoothSpeed * Time.deltaTime);
        stableRight = Vector3.Cross(Vector3.up, stableForward).normalized;
        
        lastUpdateTime = Time.time;
    }
    
    public Vector3 GetStableForward()
    {
        if (!isInitialized)
        {
            InitializeStableVectors();
        }
        return stableForward;
    }
    
    public Vector3 GetStableRight()
    {
        if (!isInitialized)
        {
            InitializeStableVectors();
        }
        return stableRight;
    }
    
    public bool IsInCollision()
    {
        return lockOnCamera != null && lockOnCamera.IsInCollision();
    }
    
    private void OnDrawGizmos()
    {
        if (!showDebugGizmos || !Application.isPlaying || !isInitialized) return;
        
        Vector3 origin = playerTransform != null ? playerTransform.position + Vector3.up * 2f : transform.position;
        
        Gizmos.color = Color.green;
        Gizmos.DrawRay(origin, stableForward * 2f);
        
        Gizmos.color = Color.red;
        Gizmos.DrawRay(origin, stableRight * 2f);
        
        if (lockOnCamera != null)
        {
            Vector3 cameraForward = lockOnCamera.transform.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();
            
            Gizmos.color = lockOnCamera.IsInCollision() ? Color.yellow : Color.cyan;
            Gizmos.DrawRay(origin + Vector3.up * 0.5f, cameraForward * 1.5f);
        }
    }
}
