using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    InputManager inputManager;
    PlayerLocomotion playerLocomotion;
    Animator animator;
    PlayerHealth playerHealth;
    public bool isInteracting;
    ComboUpgrades comboUpgrades;

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
    [SerializeField] float dotTickDmg; //dmg per tick
    [SerializeField] float dotMaxTicks; //how many ticks per hit
    [SerializeField] float dotMaxStacks; //max number of dmg stacks
    [SerializeField] float dotTickTimer; //time till next tick
    [SerializeField] float dotTickMaxTime; //time between ticks

    [Header("Melee Love Upgrade Values")]
    [SerializeField] float lsAmt;
    [SerializeField] bool lsDoubleActive;

    [Header("Melee Fear Upgrade Values")]
    [SerializeField] float moveSpeedBuff;
    [SerializeField] float moveSpeedDuration;
    [SerializeField] float moveSpeedTimer;
    [SerializeField] bool hasMoveSpeedBuff;
    [SerializeField] float speedMult;

    [Header("Melee Impact VFX References")]
    [SerializeField] private GameObject angerImpact;
    [SerializeField] private GameObject joyImpact;
    [SerializeField] private GameObject fearImpact;
    [SerializeField] private GameObject sadnessImpact;
    [SerializeField] private GameObject loveImpact;
    Weapon weapon;
    Emotions selectedMeleeEmotion;


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
        weapon = GetComponentInChildren<Weapon>();
        if (!dash) dash = GetComponent<DashAbility>();
        comboUpgrade = GetComponent<ComboUpgrade>();
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
                //Debug.Log("removed all buff stacks");
            }

            buffStackTimer -= Time.deltaTime;
        }

        if (hasMoveSpeedBuff)
        {
            if (moveSpeedTimer <= 0)
            {
                hasMoveSpeedBuff = false;
                speedMult = 1;
                UpdatePlayerSpeed();
                //Debug.Log("reset player speed");
            }

            moveSpeedTimer -= Time.deltaTime;
        }
    }

    public void UpdateMeleeUpgrades(int buffStackCap, float buffPercent, float attackSpeedBuff, float critChance, float critMult, float dotTickDmg, float dotMaxTicks, float lsAmt, bool lsDoubleActive, float moveSpeedBuff, float moveSpeedDuration)
    {
        this.buffStackCap = buffStackCap;
        this.buffPercent = buffPercent;
        this.attackSpeedBuff = attackSpeedBuff;
        this.critChance = critChance;
        this.critMult = critMult;
        this.dotTickDmg = dotTickDmg;
        this.dotMaxTicks = dotMaxTicks;
        this.lsAmt = lsAmt;
        this.lsDoubleActive = lsDoubleActive;
        this.moveSpeedBuff = moveSpeedBuff;
        this.moveSpeedDuration = moveSpeedDuration;
    }

    public void AddBuffStack()
    {
        if (buffStacks < buffStackCap) buffStacks++;


        if (buffStacks > 0) hasBuffStacks = true;
        buffStackTimer = buffStackMaxTime;

        //Debug.Log("added buff stack");
        //Debug.Log($"current buff stacks: {buffStacks}");
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

    public void PerformLifesteal(float damage)
    {
        float lifeSteal = damage * lsAmt;

        if (lsDoubleActive)
        {
            if (playerHealth.GetHealthPercent() < 0.3f)
                lifeSteal *= 2;
        }

        playerHealth.Heal(lifeSteal);
    }

    public void ApplyMoveSpeedBuff()
    {
        hasMoveSpeedBuff = true;
        speedMult = 1 + moveSpeedBuff;
        UpdatePlayerSpeed();
        moveSpeedTimer = moveSpeedDuration;
    }

    void UpdatePlayerSpeed()
    {
        playerLocomotion.ChangeSpeed(speedMult);
    }

    void UpdateCombos()
    {
        //not a today problem, im on the swing
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
