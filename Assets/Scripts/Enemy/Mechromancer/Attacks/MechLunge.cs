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
    private MechAnimationController animationController;
    private Vector3 startPosition;
    private Vector3 lastKnownLocation;
    private float currentLungeDistance;
    private bool isLunging;
    private bool hasHitPlayer;
    private bool canDamage;
    private float lungeCooldownTimer;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        animationController = GetComponent<MechAnimationController>();
    }

    public void StartLunge()
    {
        if (isLunging) return;

        if (playerTransform == null)
        {
            var bgAgent = GetComponent<BehaviorGraphAgent>();
            if (bgAgent != null)
            {
                bgAgent.BlackboardReference.GetVariableValue("PlayerTransform", out playerTransform);
            }
        }

        if (playerTransform == null)
        {
            Debug.LogError("No playerTransform assigned for lunge");
            return;
        }

        isLunging = true;
        hasHitPlayer = false;
        canDamage = false;
        lungeCooldownTimer = pauseDuration;

        lastKnownLocation = playerTransform.position;
        startPosition = transform.position;
        currentLungeDistance = 0f;

        rb.constraints = RigidbodyConstraints.FreezeRotation;

        rb.isKinematic = true;
        rb.linearVelocity = Vector3.zero;

        if (animationController != null)
        {
            animationController.TriggerLunge();
        }
        else if (animator != null)
        {
            animator.SetTrigger("Lunge");
        }
        
        animator.SetFloat("SpeedMultiplier", 2f);
        Debug.Log("MechLunge: Started lunge attack");
    }

    public void EnableHitbox(int index)
    {
        canDamage = true;
        Debug.Log("MechLunge: Damage window enabled");
    }

    public void DisableHitbox(int index)
    {
        canDamage = false;
        Debug.Log("MechLunge: Damage window disabled");
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

        if (canDamage && !hasHitPlayer)
        {
            CheckForPlayerHit();
        }
    }

    private void CheckForPlayerHit()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance < range)
        {
            var playerHealth = playerTransform.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(lungeDamage);
                Debug.Log($"MechLunge: Hit player for {lungeDamage} damage");
            }

            hasHitPlayer = true;
        }
    }

    public void StopLunge()
    {
        isLunging = false;
        canDamage = false;

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

        Debug.Log("MechLunge: Stopped lunge attack");
    }
}
