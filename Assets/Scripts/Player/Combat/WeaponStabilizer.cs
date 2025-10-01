using UnityEngine;

/// <summary>
/// Stabilizes weapon rotation to prevent strange rotations during hand animations
/// </summary>
public class WeaponStabilizer : MonoBehaviour
{
    [Header("Stabilization Settings")]
    [Tooltip("How much to stabilize rotation (0 = no stabilization, 1 = full stabilization)")]
    [Range(0f, 1f)]
    public float stabilizationStrength = 0.8f;
    
    [Tooltip("Reference transform for stable rotation (usually the player root)")]
    public Transform referenceTransform;
    
    [Tooltip("Should the weapon maintain its world rotation?")]
    public bool maintainWorldRotation = true;
    
    [Tooltip("Custom rotation offset for the weapon")]
    public Vector3 rotationOffset = Vector3.zero;
    
    [Header("Smoothing")]
    [Tooltip("How smoothly to apply rotation corrections")]
    public float smoothingSpeed = 10f;
    
    private Quaternion targetRotation;
    private Quaternion initialLocalRotation;
    private Transform parentTransform;
    
    void Start()
    {
        // If no reference transform is set, use the player root
        if (referenceTransform == null)
        {
            referenceTransform = transform.root;
        }
        
        parentTransform = transform.parent;
        initialLocalRotation = transform.localRotation;
        
        // Calculate initial target rotation
        UpdateTargetRotation();
    }
    
    void LateUpdate()
    {
        UpdateTargetRotation();
        ApplyStabilization();
    }
    
    void UpdateTargetRotation()
    {
        if (maintainWorldRotation)
        {
            // Maintain a stable world rotation based on reference transform
            Vector3 referenceForward = referenceTransform.forward;
            Vector3 referenceUp = referenceTransform.up;
            
            // Apply custom rotation offset
            Quaternion offsetRotation = Quaternion.Euler(rotationOffset);
            targetRotation = Quaternion.LookRotation(referenceForward, referenceUp) * offsetRotation;
        }
        else
        {
            // Use the initial local rotation relative to parent
            targetRotation = parentTransform.rotation * initialLocalRotation;
        }
    }
    
    void ApplyStabilization()
    {
        if (stabilizationStrength <= 0f)
            return;
            
        // Calculate current rotation and target rotation
        Quaternion currentRotation = transform.rotation;
        
        // Lerp between current rotation and target rotation based on stabilization strength
        Quaternion stabilizedRotation = Quaternion.Lerp(currentRotation, targetRotation, stabilizationStrength);
        
        // Apply smoothing
        transform.rotation = Quaternion.Lerp(currentRotation, stabilizedRotation, smoothingSpeed * Time.deltaTime);
    }
    
    /// <summary>
    /// Reset the weapon to its initial rotation
    /// </summary>
    public void ResetRotation()
    {
        transform.localRotation = initialLocalRotation;
        UpdateTargetRotation();
    }
    
    /// <summary>
    /// Temporarily disable stabilization (useful for special attacks)
    /// </summary>
    public void SetStabilizationEnabled(bool enabled)
    {
        this.enabled = enabled;
    }
}