using UnityEngine;              
using UnityEngine.Events;
public class PlayerHealth : MonoBehaviour, IDamage
{
    [Header("Base Stats")]
    [SerializeField] private float maxHealth = 100f;   // Maximum health the character can have
    [SerializeField] private float currentHealth;      // Current health value

    [Header("Options")]
    public bool isInvulnerable = false;   // If true, ignores all incoming damage (e.g. dodge frames)
    public bool autoRegen = false;        // Toggles passive health regeneration
    public float regenRate = 2f;          // How much HP regenerates per second
    public float regenDelay = 3f;         // Delay (in seconds) after taking damage before regen starts
    private float lastDamageTime;         // Tracks when the last damage was taken (used to time regen)

    [Header("Events")]
    public UnityEvent<float, float> onHealthChanged; // Event triggered whenever health updates (current, max)
    public UnityEvent onDeath;                      // Event triggered once health reaches 0
    public UnityEvent<float> onDamageTaken;         // Event triggered when damage is applied (passes amount)
    public UnityEvent<float> onHealed;              // Event triggered when healing happens (passes amount)
    
    public bool isDead { get; private set; }
    
    public static bool IsPlayerDead { get; private set; } = false;
    
    private AnimatorManager animatorManager;
    private DamageFlashEffect damageFlashEffect;
    private PlayerRagdoll playerRagdoll;

    private void Awake()
    {
        currentHealth = maxHealth;
        isDead = false;
        IsPlayerDead = false;
        onHealthChanged?.Invoke(currentHealth, maxHealth);
        animatorManager = GetComponent<AnimatorManager>();
        damageFlashEffect = GetComponent<DamageFlashEffect>();
        playerRagdoll = GetComponent<PlayerRagdoll>();
    }

    private void Update()
    {
        // If regen is enabled, enough time has passed since last damage, 
        // and health isn’t already full → regenerate
        if (autoRegen && Time.time > lastDamageTime + regenDelay && currentHealth < maxHealth)
        {
            Heal(regenRate * Time.deltaTime); // Regenerate health gradually over time
        }
    }

    public void TakeDamage(float amount)
    {
        if (isInvulnerable || amount <= 0) return;   // Ignore if invulnerable or damage is 0/negative

        currentHealth -= amount;                     // Subtract the damage amount
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Keep health between 0 and max

        lastDamageTime = Time.time;

        if (animatorManager != null)
        {
            animatorManager.PlayTakeDamageAnimation();
        }

        if (damageFlashEffect != null)
        {
            damageFlashEffect.Flash();
        }

        onDamageTaken?.Invoke(amount);
        onHealthChanged?.Invoke(currentHealth, maxHealth); // Update UI or other systems

        if (currentHealth <= 0) Die();               // If no health left → trigger death
    }

    public void Heal(float amount)
    {
        if (amount <= 0) return;                     // Ignore healing of 0 or negative values

        currentHealth += amount;                     // Add health
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Keep health within valid range

        onHealed?.Invoke(amount);                    // Notify listeners that healing happened
        onHealthChanged?.Invoke(currentHealth, maxHealth); // Update UI or other systems

        //Debug.Log($"Healed for {amount} hp");
    }

    public void Die()
    {
        isDead = true;
        IsPlayerDead = true;
        
        if (playerRagdoll != null)
        {
            playerRagdoll.ActivateRagdoll();
        }

        onDeath?.Invoke();
    }

    public void SetMaxHealth(float newMax, bool fullHeal = true)
    {
        maxHealth = newMax;                          // Update max health
        if (fullHeal) currentHealth = maxHealth;     // If fullHeal is true, restore to full health
        onHealthChanged?.Invoke(currentHealth, maxHealth); // Notify systems of new health values
    }

    public float GetHealthPercent() => currentHealth / maxHealth; // Quick helper: returns health ratio (0–1)

    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public void Revive()
    {
        isDead = false;
        IsPlayerDead = false;
    }
}
