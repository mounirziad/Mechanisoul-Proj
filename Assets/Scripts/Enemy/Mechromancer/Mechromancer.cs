using UnityEngine;
using System;
using Unity.Behavior;

public class Mechromancer : Enemy, IDamage
{
    [Header("Damage")]
    [SerializeField] private float combo1Damage = 8f;
    [SerializeField] private float combo2Damage = 9f;

    [Header("Hitboxes")]
    [SerializeField] private Collider[] comboHitboxes;
    //0 = combo1, 1 = combo2, 2 = combo3

    public bool isDead = false;

    [Header("References")]
    private LightningController lightningController;
    private MechBehaviorController controller;
    private MechAnimationController animationController;
    private MechLunge lunge;

    private BehaviorGraphAgent bgAgent;

    protected override void Awake()
    {
        base.Awake();

        bgAgent = GetComponent<BehaviorGraphAgent>();
        if (bgAgent == null)
        {
            Debug.LogError("Mechromancer: BehaviorGraphAgent not found");
            return;
        }

        var blackboard = bgAgent.BlackboardReference;
        blackboard.SetVariableValue("Self", gameObject);

        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            blackboard.SetVariableValue("Player", playerObj);
            blackboard.SetVariableValue("PlayerTransform", playerObj.transform);
        }
        else
        {
            Debug.LogError("Player not found");
        }
    }

    private void Start()
    {
        controller = GetComponent<MechBehaviorController>();
        animationController = GetComponent<MechAnimationController>();
        lightningController = GetComponent<LightningController>();

        if (lightningController != null)
        {
            lightningController.blackboard = bgAgent.BlackboardReference;
        }

        foreach (var hitbox in comboHitboxes)
        {
            if (hitbox != null)
            {
                hitbox.enabled = false;
            }
        }
    }

    //Behavior graph to animator
    public void TriggerAttack(string triggerName)
    {
        animationController?.SetTrigger(triggerName);
    }

    //Animations to behavior graph
    public void OnComboHit()
    {
        bgAgent.BlackboardReference.SetVariableValue("comboLanded", true);
    }

    public void OnAttackAnimationFinished()
    {
        bgAgent.BlackboardReference.SetVariableValue("attackFinished", true);
        Debug.Log("Mechromancer: attackFinished = true");
    }

    public void EnableHitbox(int index)
    {
        if (IsHitboxValid(index))
        {
            comboHitboxes[index].enabled = true;
        }
    }

    public void DisableHitbox(int index)
    {
        if (IsHitboxValid(index))
        {
            comboHitboxes[index].enabled = false;
        }
    }

    private bool IsHitboxValid(int index)
    {
        return index >= 0 && index < comboHitboxes.Length && comboHitboxes[index] != null;
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
