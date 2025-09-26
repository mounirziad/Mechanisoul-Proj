using Unity.Cinemachine;
using UnityEngine;

public class TestEnemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float maxHealth = 100f;
    public float currentHealth;

  

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
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

    private void OnHit()
    {
        
    }

   

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("Enemy died!");

        // Death camera effects
        OnDeath();

        // Destroy or disable enemy
        Destroy(gameObject, 1f); // Optional delay for death animation
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