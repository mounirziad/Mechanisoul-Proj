using UnityEngine;
using System;

public class Mechromancer : Enemy, IDamage
{
    [Header("Damage")]
    [SerializeField] private float combo1Damage = 8f;
    [SerializeField] private float combo2Damage = 9f;
    [SerializeField] private float combo3Damage = 13f;

    [Header("Hitboxes")]
    [SerializeField] private Collider[] comboHitboxes;
    //0 = combo1, 1 = combo2, 2 = combo3

    public bool isDead = false;

    private MechBehaviorController controller;
    private MechAnimationController animationController;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        controller = GetComponent<MechBehaviorController>();
        animationController = GetComponent<MechAnimationController>();

        //Hitboxes start disabled
        foreach (var hitbox in comboHitboxes)
        {
            hitbox.enabled = false;
        }
    }

    //Behavior graph to animator
    public void TriggerAttack(string triggerName)
    {
        animationController.SetTrigger(triggerName);
    }

    //Animations to behavior graph
    public void OnComboHit()
    {
        controller.SetBlackboardBool("comboLanded", true);
    }

    public void OnAttackAnimationFinished()
    {
        controller.SetBlackboardBool("attackFinished", true);
    }

    public void EnableHitbox(int index)
    {
        if (index >= 0 && index < comboHitboxes.Length)
        {
            comboHitboxes[index].enabled = true;
        }
    }

    public void DisableHitbox(int index)
    {
        if (index >= 0 && index < comboHitboxes.Length)
        {
            comboHitboxes[index].enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        int hitboxIndex = GetHitboxIndex(other);
        if (hitboxIndex == -1) return; //not a combo hitbox

        float damage = GetDamageForCombo(hitboxIndex);
        other.GetComponent<PlayerHealth>()?.TakeDamage(damage);

        OnComboHit();
    }

    private int GetHitboxIndex(Collider collider)
    {
        for (int i = 0; i < comboHitboxes.Length; i++)
        {
            if (comboHitboxes[i] == collider) return i;
        }
        return -1;
    }

    private float GetDamageForCombo(int index)
    {
        return index switch
        {
            0 => combo1Damage,
            1 => combo2Damage,
            2 => combo3Damage,
            _ => 0f
        };
    }

    //Damage
    public float GetDamage() => combo1Damage;

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
