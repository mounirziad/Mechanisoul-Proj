using UnityEngine;
using UnityEngine.AI;

public class ConveyorBeltEnemyActivator : MonoBehaviour
{
    [Header("Ground Detection")]
    [Tooltip("Layer mask for ground detection")]
    public LayerMask groundLayer = 1 << 9;

    [Tooltip("Minimum collision impact to activate (prevents premature activation)")]
    public float minImpactVelocity = 0.1f;

    [Tooltip("Distance to check below the enemy for ground")]
    public float groundCheckDistance = 0.5f;

    [Tooltip("Maximum time before forcing activation if grounded")]
    public float maxFallTime = 3f;

    [Tooltip("Enable debug logs")]
    public bool debugMode = true;

    private bool isActivated = false;
    private Rigidbody rb;
    private NavMeshAgent navMeshAgent;
    private float spawnTime;
    private bool hasCollided = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        spawnTime = Time.time;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isActivated)
            return;

        hasCollided = true;

        bool isGroundLayer = (groundLayer.value & (1 << collision.gameObject.layer)) != 0;

        if (debugMode)
        {
        }

        if (!isGroundLayer)
            return;

        ActivateEnemy();
    }

    void OnCollisionStay(Collision collision)
    {
        if (isActivated || !hasCollided)
            return;

        bool isGroundLayer = (groundLayer.value & (1 << collision.gameObject.layer)) != 0;
        if (isGroundLayer && rb != null && rb.linearVelocity.magnitude < 0.5f)
        {
            if (debugMode)
                Debug.Log($"ConveyorBeltEnemyActivator: {gameObject.name} detected as settled on ground");
            ActivateEnemy();
        }
    }

    void FixedUpdate()
    {
        if (isActivated)
            return;

        if (Time.time - spawnTime > maxFallTime)
        {
            if (debugMode)
                Debug.LogWarning($"ConveyorBeltEnemyActivator: {gameObject.name} max fall time reached, force activating");
            ActivateEnemy();
            return;
        }

        if (IsGrounded() && rb != null && rb.linearVelocity.y < 0.1f && rb.linearVelocity.y > -0.1f)
        {
            if (debugMode)
                Debug.Log($"ConveyorBeltEnemyActivator: {gameObject.name} grounded via raycast");
            ActivateEnemy();
        }
    }

    bool IsGrounded()
    {
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        
        if (Physics.Raycast(origin, Vector3.down, out hit, groundCheckDistance, groundLayer))
        {
            return true;
        }

        return false;
    }

    void ActivateEnemy()
    {
        if (isActivated)
            return;

        isActivated = true;

        if (rb != null)
        {
            rb.mass = 1f;
            rb.freezeRotation = false;
        }

        if (navMeshAgent != null)
        {
            navMeshAgent.enabled = true;
        }

        EnableComponent<BasicEnemyLocomotion>();
        EnableComponent<DebugNavMeshAgent>();
        EnableComponent<Ragdoll>();
        EnableComponent<EnemySlowStacks>();
        EnableComponent<EnemyCharm>();
        EnableComponent<EnemyStun>();
        EnableComponent<BasicEnemyHealth>();
        EnableComponent<TagChildrenWithParentTag>();
        EnableComponent<AiAgent>();
        EnableComponent<AiWeapons>();
        EnableComponent<EnemyMeleeDamage>();
        EnableComponent<AiRootMotionController>();

        Debug.Log($"ConveyorBeltEnemyActivator: Enemy {gameObject.name} activated on ground impact");

        Destroy(this);
    }

    void EnableComponent<T>() where T : Behaviour
    {
        T component = GetComponent<T>();
        if (component != null)
        {
            component.enabled = true;
        }
    }
}
