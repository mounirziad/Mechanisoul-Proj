using UnityEngine;
using System;

public class Mechromancer : Enemy, IDamage
{
    [Header("Stats")]
    [SerializeField] private float damageProvider = 8f;
    
    Transform player;
    private bool attacking;

    public bool isDead = false;

    private MechBehaviorController controller;
    private bool phase2Triggered = false;
    private bool rageTriggered = false;

    protected override void Awake()
    {
        base.Awake();
        player = GameObject.FindWithTag("Player").transform;
    }

    private void Start()
    {
        controller = GetComponent<MechBehaviorController>();
        attacking = false;
    }

    /*private void Update()
    {
        if (!phase2Triggered && currentHealth <= 50)
        {
            phase2Triggered = true;
            controller.TriggerPhase("Phase2");
        }

        if (!rageTriggered && currentHealth <= 35)
        {
            rageTriggered = true;
            controller.TriggerPhase("Rage");
        }

        if (currentHealth <= 0)
        {
            controller.TriggerPhase("Death");
        }
    }*/

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
