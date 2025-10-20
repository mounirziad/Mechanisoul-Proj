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
    
    [Header("Audio Settings")]
    [SerializeField] private float hitSoundCooldown = 0.2f;
    private float lastHitSoundTime = -999f;
    
    [Header("Hit Tracking")]
    private WeaponHitTracker hitTracker;
    
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
        
        // Initialize hit tracker
        hitTracker = GetComponent<WeaponHitTracker>();
        if (hitTracker == null)
        {
            hitTracker = gameObject.AddComponent<WeaponHitTracker>();
        }
        
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
        if (!other.gameObject.CompareTag("Enemy")) return;

        // Still keep Mechromancer (if that’s another type of enemy)
        Mechromancer mechromancer = other.GetComponent<Mechromancer>();

        // Check if this is a new enemy hit using the hit tracker
        bool isNewHit = hitTracker != null ? hitTracker.TryHitEnemy(other.gameObject) : true;

        // Play hit sound effect only for new hits with cooldown
        if (isNewHit && SoundManager.Instance != null && Time.time - lastHitSoundTime >= hitSoundCooldown)
        {
            SoundManager.Instance.PlayHitSound();
            lastHitSoundTime = Time.time;
            Debug.Log($"Playing hit sound for {other.name}");
        }
        else if (!isNewHit)
        {
            Debug.Log($"Skipping hit sound for {other.name} - already hit this enemy");
        }

        // Calculate damage with player upgrades / buffs
        float finalDamage = damage; // base damage
        if (playerManager != null)
        {
            //Debug.Log($"pre buff damage: {finalDamage}");
            finalDamage *= 1f + playerManager.GetDamageBuffIncrease(); // dynamic buff multiplier
        }

        // Apply damage to Mechromancer if present
        if (mechromancer != null)
        {
            Debug.Log("Hit Mechromancer");
            mechromancer.TakeDamage(finalDamage);
        }

        // Spawn hit VFX at contact point only for new hits
        if (hitVFX != null && isNewHit)
        {
            Vector3 contactPoint = other.ClosestPoint(transform.position);
            GameObject vfxInstance = Instantiate(hitVFX, contactPoint, Quaternion.identity);
            vfxInstance.GetComponent<VFXCameraLookAt>().distanceFromCamera =
                Vector3.Distance(contactPoint, Camera.main.transform.position) - 0.1f;
        }

        // Forward hit to HitBox if present
        HitBox hitBox = other.GetComponent<HitBox>();
        if (hitBox != null)
        {
            Vector3 dir = (other.transform.position - transform.position).normalized;
            hitBox.OnRayCastHit(this, dir);
        }

        // Apply damage to BasicEnemyHealth if present
        BasicEnemyHealth basicEnemy = other.GetComponent<BasicEnemyHealth>();
        if (basicEnemy != null)
        {
            Debug.Log("Hit basic enemy");
            Vector3 direction = (other.transform.position - transform.position).normalized;
            basicEnemy.TakeDamage(finalDamage, direction);
            playerManager.PerformLifesteal(finalDamage);
            playerManager.ApplyMoveSpeedBuff();
            //Debug.Log($"Damage Dealt: {finalDamage}");

            

            // Optional: old upgrade system relics commented out
            /*
            basicEnemy.ApplyModifiers(
                playerManager.GetSlowAmount(),
                playerManager.GetSlowLength(),
                playerManager.GetStunLength()
            );
            playerManager.ApplyLifesteal(finalDamage);
            */
        }

        // Update buff stacks only for new hits
        if (playerManager != null && isNewHit)
            playerManager.AddBuffStack();
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
    /// Reset the hit sound cooldown to allow a new hit sound to play
    /// This should be called when a new attack starts
    /// </summary>
    public void ResetHitSoundCooldown()
    {
        lastHitSoundTime = -999f;
        
        // Also start new attack tracking
        if (hitTracker != null)
        {
            hitTracker.StartNewAttack();
        }
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
