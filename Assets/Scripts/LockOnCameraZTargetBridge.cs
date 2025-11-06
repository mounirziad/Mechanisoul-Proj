using UnityEngine;

public class LockOnCameraZTargetBridge : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LockOnCamera lockOnCamera;
    [SerializeField] private ZTargetingSystem zTargeting;
    
    [Header("Target Point")]
    [SerializeField] private Transform lockTargetPoint;
    [SerializeField] private float targetHeightOffset = 1f;
    [SerializeField] private float followSpeed = 10f;
    
    private void Awake()
    {
        if (lockOnCamera == null)
            lockOnCamera = GetComponent<LockOnCamera>();
        
        if (zTargeting == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                zTargeting = player.GetComponent<ZTargetingSystem>();
        }
        
        if (lockTargetPoint == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                lockTargetPoint = player.transform.Find("LockTargetPoint");
                
                if (lockTargetPoint == null)
                {
                    GameObject go = new GameObject("LockTargetPoint");
                    go.transform.SetParent(player.transform);
                    go.transform.localPosition = Vector3.zero;
                    lockTargetPoint = go.transform;
                }
            }
        }
    }
    
    private void Update()
    {
        if (zTargeting == null || lockTargetPoint == null)
            return;
        
        if (zTargeting.IsLocked && zTargeting.CurrentTarget != null)
        {
            Vector3 targetPosition = zTargeting.CurrentTarget.position + Vector3.up * targetHeightOffset;
            lockTargetPoint.position = Vector3.Lerp(
                lockTargetPoint.position,
                targetPosition,
                followSpeed * Time.deltaTime
            );
        }
        else
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                lockTargetPoint.position = Vector3.Lerp(
                    lockTargetPoint.position,
                    player.transform.position + Vector3.up * targetHeightOffset,
                    followSpeed * Time.deltaTime
                );
            }
        }
    }
    
    public bool IsLocked()
    {
        return zTargeting != null && zTargeting.IsLocked;
    }
    
    public Transform GetCurrentTarget()
    {
        return zTargeting != null ? zTargeting.CurrentTarget : null;
    }
}
