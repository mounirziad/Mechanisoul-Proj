using Unity.Behavior;
using UnityEngine;

public class MechLunge : MonoBehaviour
{
    public Transform playerTransform;
    public float range = 4f;
    public float force = 8f;
    public float maxDistance = 4f;
    public float lungeDamage = 8f;
    public float pauseDuration = 2.5f;
    
    private Rigidbody rb;
    private Animator animator;
    private Vector3 startPosition;
    private Vector3 lastKnownLocation;
    private float currentLungeDistance;
    private bool isLunging;
    private bool hasHitPlayer;
    private float lungeCooldownTimer;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (isLunging || lungeCooldownTimer > 0f) return;

        var player = GameObject.FindWithTag("Player");
        playerTransform = player.transform;

        if (player == null)
        {
            Debug.LogError("Player not found in scene");
            return;
        }

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance < range && !isLunging)
        {
            StartLunge();
        }
    }

    public void StartLunge()
    {
        if (isLunging) return;

        isLunging = true;
        hasHitPlayer = false;
        lungeCooldownTimer = pauseDuration;

        lastKnownLocation = playerTransform.position;
        startPosition = transform.position;
        currentLungeDistance = 0f;

        rb.constraints = RigidbodyConstraints.FreezeRotation;

        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;

        animator.SetTrigger("Lunge");
        animator.SetFloat("SpeedMultiplier", 2f);
    }

    public void ApplyDamage()
    {
        if (hasHitPlayer) return;

        float distance = Vector3.Distance(transform.position, lastKnownLocation);

        if (distance < range)
        {
            var playerHealth = playerTransform.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(lungeDamage);
            }

            hasHitPlayer = true;
        }
    }

    private void FixedUpdate()
    {
        if (!isLunging) return;

        currentLungeDistance = Vector3.Distance(startPosition, transform.position);

        if (currentLungeDistance >= maxDistance)
        {
            StopLunge();
            return;
        }

        Vector3 direction = (lastKnownLocation - transform.position).normalized;
        transform.position += direction * force * Time.fixedDeltaTime;
    }

    public void StopLunge()
    {
        isLunging = false;

        animator.SetFloat("SpeedMultiplier", 1f);

        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;

        rb.constraints =
            RigidbodyConstraints.FreezePositionY |
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;

        var bgAgent = GetComponent<BehaviorGraphAgent>();
        if (bgAgent != null)
        {
            bgAgent.BlackboardReference.SetVariableValue("lungeFinished", true);
        }
    }
}
