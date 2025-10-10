using UnityEngine;
using System;

public class Mechromancer : MonoBehaviour, IDamage
{
    [Header("Stats")]
    [SerializeField] public float maxHealth = 75f;
    [SerializeField] public float currentHealth;
    [SerializeField] private float damageProvider = 8f;

    public float rotationSpeed;
    public GameObject blade;
    private bool attacking;

    private bool isDead = false;

    private void Start()
    {
        currentHealth = maxHealth;
        attacking = false;
    }

    private void Update()
    {
        if (attacking)
        {
            blade.transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            attacking = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            attacking = false;
        }
    }

    public float GetDamage()
    {
        return damageProvider;
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"Mechromancer took {damage} damage. Remaining HP {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        Debug.Log("Mechromancer is dead");
        Destroy(gameObject);
    }
}
