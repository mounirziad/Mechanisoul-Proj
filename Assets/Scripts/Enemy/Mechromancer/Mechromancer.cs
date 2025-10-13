using UnityEngine;
using System;

public class Mechromancer : MonoBehaviour, IDamage
{
    [Header("Stats")]
    [SerializeField] public float maxHealth = 75f;
    [SerializeField] public float currentHealth;
    [SerializeField] private float damageProvider = 8f;
    [SerializeField] private float lightningDamage = 15f;

    public GameObject cannon;
    public GameObject rangeAttackPrefab;
    public float launchForce;
    public float timeBetweenShots;
    private float timeSinceLastShot;

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
        timeSinceLastShot += Time.deltaTime;

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

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            Vector3 targetPosition = new Vector3(other.transform.position.x, cannon.transform.position.y, other.transform.position.z);
            Quaternion targetRotation = Quaternion.LookRotation(targetPosition - cannon.transform.position);
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
