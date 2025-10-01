using UnityEngine;

/// <summary>
/// Weapon socket system that maintains weapon orientation independent of hand animations
/// </summary>
public class WeaponSocket : MonoBehaviour
{
    [Header("Socket Settings")]
    [Tooltip("The hand bone that this socket follows")]
    public Transform handBone;
    
    [Tooltip("Position offset from the hand bone")]
    public Vector3 positionOffset = Vector3.zero;
    
    [Tooltip("Rotation offset from the character's forward direction")]
    public Vector3 rotationOffset = Vector3.zero;
    
    [Tooltip("Should position follow the hand bone?")]
    public bool followHandPosition = true;
    
    [Tooltip("Should rotation be independent of hand bone rotation?")]
    public bool independentRotation = true;
    
    [Header("Smoothing")]
    [Tooltip("How smoothly to follow position changes")]
    public float positionSmoothness = 15f;
    
    [Tooltip("How smoothly to apply rotation changes")]
    public float rotationSmoothness = 10f;
    
    private Transform characterRoot;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    
    void Start()
    {
        // Get the character root (player transform)
        characterRoot = transform.root;
        
        // If no hand bone is assigned, try to find it
        if (handBone == null)
        {
            handBone = transform.parent;
        }
        
        UpdateTargets();
    }
    
    void LateUpdate()
    {
        UpdateTargets();
        ApplyTransforms();
    }
    
    void UpdateTargets()
    {
        if (handBone == null || characterRoot == null)
            return;
            
        // Calculate target position
        if (followHandPosition)
        {
            // Follow hand position with offset
            Vector3 worldOffset = characterRoot.TransformDirection(positionOffset);
            targetPosition = handBone.position + worldOffset;
        }
        else
        {
            targetPosition = transform.position;
        }
        
        // Calculate target rotation
        if (independentRotation)
        {
            // Use character's forward direction with custom offset
            Vector3 characterForward = characterRoot.forward;
            Vector3 characterUp = characterRoot.up;
            
            Quaternion baseRotation = Quaternion.LookRotation(characterForward, characterUp);
            Quaternion offsetRotation = Quaternion.Euler(rotationOffset);
            targetRotation = baseRotation * offsetRotation;
        }
        else
        {
            // Follow hand bone rotation with offset
            targetRotation = handBone.rotation * Quaternion.Euler(rotationOffset);
        }
    }
    
    void ApplyTransforms()
    {
        // Smoothly move to target position
        if (followHandPosition)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, positionSmoothness * Time.deltaTime);
        }
        
        // Smoothly rotate to target rotation
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSmoothness * Time.deltaTime);
    }
    
    /// <summary>
    /// Attach a weapon to this socket
    /// </summary>
    public void AttachWeapon(GameObject weapon)
    {
        if (weapon != null)
        {
            weapon.transform.SetParent(transform);
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;
        }
    }
    
    /// <summary>
    /// Detach weapon from this socket
    /// </summary>
    public void DetachWeapon()
    {
        if (transform.childCount > 0)
        {
            Transform weapon = transform.GetChild(0);
            weapon.SetParent(null);
        }
    }
}