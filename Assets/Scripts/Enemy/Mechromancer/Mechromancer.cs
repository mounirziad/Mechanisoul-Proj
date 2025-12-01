using UnityEngine;
using System;
using Unity.Behavior;

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

    [Header("References")]
    private Resurrection resurrection;
    private LightningController lightningController;
    private MechBehaviorController controller;
    private MechAnimationController animationController;
    private MechLunge lunge;

    protected override void Awake()
    {
        base.Awake();

        var agent = GetComponent<BehaviorGraphAgent>();
        if (agent == null)
        {
            Debug.LogError("Mechromancer: BehaviorGraphAgent not found");
            return;
        }

        var blackboard = agent.BlackboardReference;
        if (blackboard == null)
        {
            Debug.LogError("Mechromancer: BlackboardReference is null on BehaviorGraphAgent");
            return;
        }

        blackboard.SetVariableValue("Self", this.gameObject);

        /*if (Self != null)
        {
            Debug.Log($"Mechromancer: Self assigned correctly in Blackboard: {selfCheck.name}");
        }
        else
        {
            Debug.LogError("Mechromancer: Self variable failed to assign in Blackboard");
        }*/

        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            blackboard.SetVariableValue("Player", player);
            blackboard.SetVariableValue("PlayerTransform", player.transform);

            Debug.Log($"Mechromancer: Player assigned in Blackboard: {player.name}");
        }
        else
        {
            Debug.LogError("Player not found in scene");
        }
    }

    private void Start()
    {
        controller = GetComponent<MechBehaviorController>();
        animationController = GetComponent<MechAnimationController>();

        lightningController = GetComponent<LightningController>();
        if (lightningController != null)
        {
            lightningController.blackboard = GetComponent<BehaviorGraphAgent>().BlackboardReference;
        }

        if (controller != null)
        {
            Debug.Log("Mechromancer: MechBehaviorController found on Mechromancer");
        }
        else
        {
            Debug.LogError("Mechromancer: MechBehaviorController not found on Mechromancer");
        }

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
