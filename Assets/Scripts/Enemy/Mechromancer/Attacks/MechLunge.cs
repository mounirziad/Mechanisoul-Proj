using JetBrains.Annotations;
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

        Vector3 lungeDirection = (lastKnownLocation - transform.position).normalized;

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(lungeDirection * force, ForceMode.Impulse);
        }

        if (animator != null)
        {
            animator.SetFloat("SpeedMultiplier", 2f);
            animator.SetTrigger("Lunge");
        }

        Invoke(nameof(EnableLungeMovement), 0.5f); //delay
    }

    private void EnableLungeMovement()
    {
        if (rb != null)
        {
            rb.isKinematic = false;
            Vector3 lungeDirection = (lastKnownLocation - transform.position).normalized;
            rb.AddForce(lungeDirection * force, ForceMode.Impulse);
        }

        if (animator != null)
        {
            animator.SetFloat("SpeedMultiplier", 2f);
        }
    }

    public void ApplyDamage()
    {
        if (hasHitPlayer) return;

        if (animator != null)
        {
            animator.SetFloat("SpeedMultiplier", 2f);
        }

        float distance = Vector3.Distance(transform.position, lastKnownLocation);

        if (distance < range)
        {
            var player = playerTransform.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(lungeDamage);
            }

            hasHitPlayer = true;
        }
    }

    private void FixedUpdate()
    {
        if (isLunging)
        {
            currentLungeDistance = Vector3.Distance(startPosition, transform.position);

            if (currentLungeDistance < maxDistance)
            {
                transform.position = Vector3.MoveTowards(transform.position, lastKnownLocation, force * Time.fixedDeltaTime);
            }
            else
            {
                StopLunge();
            }
        }

        if (lungeCooldownTimer > 0f)
        {
            lungeCooldownTimer -= Time.deltaTime;
        }
    }

    public void StopLunge()
    {
        if (!isLunging) return;

        isLunging = false;

        if (animator != null)
        {
            animator.SetFloat("SpeedMultiplier", 1f);
            animator.SetBool("isLunging", false);
        }

        animator.ResetTrigger("Lunge");

        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            rb.linearVelocity = Vector3.zero;
        }
    }
}
