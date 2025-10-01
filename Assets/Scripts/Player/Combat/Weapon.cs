using Unity.Cinemachine;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float damage;
    public float attackRange;

    public GameObject hitVFX;
    private BoxCollider triggerBox;

    public Camera cam;

    PlayerManager playerManager;
    
    [Header("Rotation Stabilization")]
    [Tooltip("Enable rotation stabilization to prevent weird rotations during hand animations")]
    public bool enableRotationStabilization = true;
    
    [Tooltip("How much to stabilize rotation (0 = no stabilization, 1 = full stabilization)")]
    [Range(0f, 1f)]
    public float stabilizationStrength = 0.7f;
    
    [Tooltip("How smoothly to apply rotation corrections")]
    public float rotationSmoothness = 8f;
    
    private Transform characterRoot;
    private Quaternion initialLocalRotation;
    private Quaternion targetRotation;

    private void Awake()
    {
        playerManager = transform.root.gameObject.GetComponent<PlayerManager>();
        triggerBox = GetComponent<BoxCollider>();
        triggerBox.isTrigger = true; // make sure it's set as a trigger
        
        // Initialize stabilization
        characterRoot = transform.root;
        initialLocalRotation = transform.localRotation;
    }
    
    private void Start()
    {
        UpdateTargetRotation();
    }
    
    private void LateUpdate()
    {
        if (enableRotationStabilization)
        {
            ApplyRotationStabilization();
        }
    }
    
    private void UpdateTargetRotation()
    {
        if (characterRoot != null)
        {
            // Create a stable rotation based on character's forward direction
            Vector3 characterForward = characterRoot.forward;
            Vector3 characterUp = characterRoot.up;
            
            // Keep the weapon aligned with character's orientation
            Quaternion baseRotation = Quaternion.LookRotation(characterForward, characterUp);
            
            // Apply the initial local rotation as an offset
            targetRotation = baseRotation * initialLocalRotation;
        }
    }
    
    private void ApplyRotationStabilization()
    {
        if (stabilizationStrength <= 0f || characterRoot == null)
            return;
            
        UpdateTargetRotation();
        
        // Lerp between current rotation and target rotation
        Quaternion currentRotation = transform.rotation;
        Quaternion stabilizedRotation = Quaternion.Lerp(currentRotation, targetRotation, stabilizationStrength);
        
        // Apply smoothing
        transform.rotation = Quaternion.Lerp(currentRotation, stabilizedRotation, rotationSmoothness * Time.deltaTime);
    }

    

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
                // Still keep Mechromancer (if that’s another type of enemy)
                Mechromancer enemy = other.GetComponent<Mechromancer>();
                if (enemy != null)
                {
                    Debug.Log("Hit mechromancer");
                    enemy.TakeDamage(damage);
                }

                // spawn hit VFX
                Vector3 contactPoint = other.ClosestPoint(transform.position);
            if (hitVFX != null)
            {
                GameObject vfxInstance = Instantiate(hitVFX, contactPoint, Quaternion.identity);
                vfxInstance.GetComponent<VFXCameraLookAt>().distanceFromCamera =
                    Vector3.Distance(contactPoint, Camera.main.transform.position) - 0.1f;
            }

            // hitbox damage forwarding
            Vector3 dir = (other.transform.position - transform.position).normalized;
            HitBox hitBox = other.GetComponent<HitBox>();
            if (hitBox != null)
            {
                hitBox.OnRayCastHit(this, dir);
            }
           
            BasicEnemyHealth basicEEnemy = other.GetComponent<BasicEnemyHealth>();
            if (basicEEnemy != null)
            {
                Debug.Log("Hit basic enemy");
                Vector3 direction = (other.transform.position - transform.position).normalized;
                basicEEnemy.TakeDamage(damage, direction);

                basicEEnemy.ApplyModifiers(
                     playerManager.GetSlowAmount(),
                     playerManager.GetSlowLength(),
                     playerManager.GetStunLength()
                 );
                playerManager.ApplyLifesteal(damage);
            }
        }
    }

    public void EnableTriggerBox()
    {
        triggerBox.enabled = true;
    }

    public void DisableTriggerBox()
    {
        triggerBox.enabled = false;
    }
    
    /// <summary>
    /// Enable or disable rotation stabilization
    /// </summary>
    public void SetRotationStabilization(bool enabled)
    {
        enableRotationStabilization = enabled;
    }
    
    /// <summary>
    /// Set the stabilization strength (0-1)
    /// </summary>
    public void SetStabilizationStrength(float strength)
    {
        stabilizationStrength = Mathf.Clamp01(strength);
    }
    
    /// <summary>
    /// Reset weapon to its stabilized rotation
    /// </summary>
    public void ResetToStableRotation()
    {
        if (characterRoot != null)
        {
            UpdateTargetRotation();
            transform.rotation = targetRotation;
        }
    }
}
