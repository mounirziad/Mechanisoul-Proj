using UnityEngine;

public class BasicEnemyHealth : MonoBehaviour
{
    public float maxHealth;
    public float currentHealth;
   
    public float blinkIntesnity;
    public float blinkDuration;
    float blinkTimer;
    AiAgent agent;
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f;
    public float hitCooldownTime = 0.2f;
    private bool hitCooldown = false;

    
    private bool isSlowed = false;
    private float slowTimer = 0f;
    private float slowAmount = 0f;

    private bool isStunned = false;
    private float stunTimer = 0f;

    private float baseSpeed;
    public UnityEngine.AI.NavMeshAgent navAgent;

    public bool IsStunned() => isStunned;
    public bool IsSlowed() => isSlowed;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        navAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navAgent != null)
        {
            baseSpeed = navAgent.speed; // save original speed
        }

        agent = GetComponent<AiAgent>();
        agent.skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        currentHealth = maxHealth;
        var rigidBodies = GetComponentsInChildren<Rigidbody>();
        foreach(var rigidBody in rigidBodies)
        {
           HitBox hitBox = rigidBody.gameObject.AddComponent<HitBox>();
            hitBox.health = this;
        }
    }

    public void ApplyModifiers(float slowAmount, float slowLength, float stunLength)
    {

        Debug.Log("We Out here applying effects n shit");
        // Apply slow if applicable
        if (slowLength > 0 && slowAmount > 0)
        {
            this.slowAmount = slowAmount;
            this.slowTimer = slowLength;
            isSlowed = true;
        }

        // Apply stun if applicable
        if (stunLength > 0)
        {
            baseSpeed = 0;
            this.stunTimer = stunLength;
            isStunned = true;
        }
    }


    // Update is called once per frame
    void Update()
    {
        blinkTimer -= Time.deltaTime;
        float lerp = Mathf.Clamp01(blinkTimer / blinkDuration);
        float intensity = (lerp * blinkIntesnity) + 1.0f;
        agent.skinnedMeshRenderer.material.color = Color.white * intensity;

        if (navAgent != null)
        {
            if (isStunned)
            {
                Debug.Log("STUNNED: Setting speed to 0. Timer: " + stunTimer); // <-- ADD THIS
                navAgent.speed = 0f;
            }
            else if (isSlowed)
            {
                Debug.Log("SLOWED: Base Speed: " + baseSpeed + " Slow Amount: " + slowAmount); // <-- ADD THIS
                navAgent.speed = baseSpeed * (1f - slowAmount);
            }
            else
            {
                // Debug.Log("NORMAL SPEED: " + baseSpeed); // Optional: if you want to see it reset
                navAgent.speed = baseSpeed;
            }
        }
    }


    public void TakeDamage(float amount, Vector3 direction)
    {
        
        if (hitCooldown || agent.isDead) return; // Prevent damage if dead

        if (hitCooldown) return; // skip repeated hits
        hitCooldown = true;
        Invoke(nameof(ResetHitCooldown), hitCooldownTime);

        currentHealth -= amount;

        if (currentHealth > 0)
        {
            Animator anim = GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetTrigger("Hit");
            }

            BasicEnemyLocomotion locomotion = GetComponent<BasicEnemyLocomotion>();
            if (locomotion != null)
            {
                locomotion.ApplyKnockback(direction, knockbackForce, knockbackDuration);
            }
        }

        if (currentHealth <= 0)
        {
            Die(direction);
        }

        blinkTimer = blinkDuration;
    }

    private void ResetHitCooldown()
    {
        hitCooldown = false;
    }

    private void Die(Vector3 direction)
    {
        AiDeathState deathState = agent.stateMachine.GetState(AiStateId.Death) as AiDeathState;
        deathState.direction = direction;
        agent.stateMachine.ChangeState(AiStateId.Death);
       
    }

     
}
