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


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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

    // Update is called once per frame
    void Update()
    {
        blinkTimer -= Time.deltaTime;
        float lerp = Mathf.Clamp01(blinkTimer / blinkDuration);
        float intensity = (lerp * blinkIntesnity) + 1.0f;
        agent.skinnedMeshRenderer.material.color = Color.white * intensity;
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
