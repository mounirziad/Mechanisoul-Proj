using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    InputManager inputManager;
    PlayerLocomotion playerLocomotion;
    Animator animator;
    PlayerHealth playerHealth;
    public bool isInteracting;

    [Header("Dash / Abilities")]
    [SerializeField] private DashAbility dash; // forwards upgrade values to dash effects (Anger/Sadness)

    [Header("Melee Anger Upgrade Values")]
    [SerializeField] int buffStackCap; //max dmg buff stacks
    [SerializeField] float buffPercent; //% buff added after each attack
    [SerializeField] int buffStacks; //current # of dmg stacks
    [SerializeField] float buffStackMaxTime; //max amount of time between attacks to keep buff
    [SerializeField] float buffStackTimer; //current time remaining till lose dmg stacks
    [SerializeField] bool hasBuffStacks; //if player has dmg buff stacks

    [Header("Melee Joy Upgrade Values")]
    [SerializeField] float attackSpeedBuff;
    [SerializeField] float critChance;
    [SerializeField] float critMult;

    [Header("Melee Sadness Upgrade Values")]
    float dotTickDmg; //dmg per tick
    float dotMaxTicks; //how many ticks per hit
    float dotMaxStacks; //max number of dmg stacks
    float dotTickTimer; //time till next tick
    float dotTickMaxTime; //time between ticks






    //should be obsolete soon inshallah
    [Header("OLD Upgrade Values")]
    [SerializeField] float aoeAmount;
    [SerializeField] float slowAmount;
    [SerializeField] float slowLength;
    [SerializeField] float lifeStealAmount;
    [SerializeField] float stunLength;



    private void Awake()
    {
        playerHealth = gameObject.GetComponent<PlayerHealth>();
        animator = GetComponent<Animator>();
        inputManager = GetComponent<InputManager>();
        playerLocomotion = GetComponent<PlayerLocomotion>();
        if (!dash) dash = GetComponent<DashAbility>();
    }

    void Update()
    {
        inputManager.HandleAllInputs();
        HandleTimers();
    }

    private void FixedUpdate()
    {
        playerLocomotion.HandleAllMovement();
    }

    private void LateUpdate()
    {
        isInteracting = animator.GetBool("isInteracting");
        playerLocomotion.isJumping = animator.GetBool("isJumping");
        animator.SetBool("isGrounded", playerLocomotion.isGrounded);
    }


    void HandleTimers()
    {
        if (hasBuffStacks)
        {
            if (buffStackTimer <= 0)
            {
                hasBuffStacks = false;
                buffStacks = 0;
                Debug.Log("removed all buff stacks");
            }

            buffStackTimer -= Time.deltaTime;
        }
    }

    public void UpdateMeleeUpgrades(int buffStackCap, float buffPercent, float attackSpeedBuff, float critChance, float critMult, float dotTickDmg, float dotMaxTicks)
    {
        this.buffStackCap = buffStackCap;
        this.buffPercent = buffPercent;
        this.attackSpeedBuff = attackSpeedBuff;
        this.critChance = critChance;
        this.critMult = critMult;
        this.dotTickDmg = dotTickDmg;
        this.dotMaxTicks = dotMaxTicks;
    }

    public void AddBuffStack()
    {
        if (buffStacks < buffStackCap) buffStacks++;

        
        if (buffStacks > 0) hasBuffStacks = true;
        buffStackTimer = buffStackMaxTime;

        Debug.Log("added buff stack");
        Debug.Log($"current buff stacks: {buffStacks}");
    }

    public float GetDamageBuffIncrease()
    {
        return buffStacks * buffPercent;
    }

    public float GetCrit()
    {
        if (Random.Range(0f, 1f) <= critChance) return critMult;
        return 0;
    }


    public float GetDamageMultiplier() //get this for total damage multipler from modifier
    {
        return 1 + GetDamageBuffIncrease() + GetCrit();
    }

    public float GetAttackSpeedBuff() 
    { 
        return attackSpeedBuff; 
    }

    public void temp()
    {

    }






    //old melee upgrades, will delete when fully removed from use
    public void UpdateUpgrades(float aoeAmount, float slowAmount, float slowLength, float lifeStealAmount, float stunLength)
    {
        this.aoeAmount = aoeAmount;
        this.slowAmount = slowAmount;
        this.slowLength = slowLength;
        this.lifeStealAmount = lifeStealAmount;
        this.stunLength = stunLength;
    }


    public float GetAOEAmount()
    {
        return aoeAmount;
    }

    public float GetSlowAmount()
    {
        return slowAmount;
    }

    public float GetSlowLength()
    {
        return slowLength;
    }

    public float GetLifeStealAmount()
    {
        return lifeStealAmount;
    }

    public float GetStunLength()
    {
        return stunLength;
    }

    public void ApplyLifesteal(float damage)
    {
        playerHealth.Heal(damage * lifeStealAmount);
        Debug.Log($"healed {damage * lifeStealAmount} hp");
    }

}
