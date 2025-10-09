using System.ComponentModel;
using Unity.Cinemachine;
using UnityEngine;

public class TestEnemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float maxHealth = 100f;
    public float currentHealth;

    //stats for testing
    float baseSpeed = 10f;
    [SerializeField] float speed;
    [SerializeField] bool stunned;
    float stunTimer;
    float slowTimer;
    [SerializeField] bool slowed;

  

    private bool isDead = false;

    private void Awake()
    {
        speed = baseSpeed;
    }

    void Start()
    {
        currentHealth = maxHealth;
    }

    private void Update()
    {
        if (slowed)
        {
            slowTimer -= Time.deltaTime;

            if (slowTimer <= 0)
                slowed = false;
        }

        if (stunned)
        {
            stunTimer -= Time.deltaTime;

            if (stunTimer <= 0)
                stunned = false;
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"Enemy took {damage} damage. Health: {currentHealth}");

        // Trigger hit effects
        OnHit();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void ApplyModifiers(float slowAmt, float slowLen, float stunLen)
    {
        if (!slowed)
        {
            speed -= baseSpeed * slowAmt;
            slowTimer = slowLen;

            Debug.Log("Applied Slow");
        }

        if (!stunned)
        {
            stunned = true;
            stunTimer = stunLen;

            Debug.Log("Applied Stun");
        }
    }

    private void OnHit()
    {
        
    }

   

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("Enemy died!");

        // Disable all colliders when enemy dies
        DisableHitBoxes();

        // Death camera effects
        OnDeath();

        // Destroy or disable enemy
        Destroy(gameObject, 1f); // Optional delay for death animation
    }

    private void DisableHitBoxes()
    {
        // Disable all colliders to prevent any interaction
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.enabled = false;
        }
    }

    private void OnDeath()
    {
      

        // Optional: Screen shake on death
        StartCoroutine(DeathScreenShake());
    }

    private System.Collections.IEnumerator DeathScreenShake()
    {
        // Add additional death effects here
        yield return new WaitForSeconds(0.1f);

        // You can add more camera effects for death
    }
}