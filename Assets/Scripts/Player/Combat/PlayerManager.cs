using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    InputManager inputManager;
    PlayerLocomotion playerLocomotion;
    Animator animator;
    PlayerHealth playerHealth;
    public bool isInteracting;

    [Header("Base Player Values")]
    [SerializeField] float baseDamage;

    [Header("Dash / Abilities")]
    [SerializeField] private DashAbility dash; // forwards upgrade values to dash effects (Anger/Sadness)

    [Header("Melee Upgrade Values")]
    [SerializeField] int buffStackCap; //max dmg buff stacks
    [SerializeField] float buffPercent; //% buff added after each attack
    [SerializeField] int buffStacks; //current # of dmg stacks
    [SerializeField] float buffStackMaxTime; //max amount of time between attacks to keep buff
    [SerializeField] float buffStackTimer; //current time remaining till lose dmg stacks
    [SerializeField] bool hasBuffStacks; //if player has dmg buff stacks






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

    public void UpdateMeleeUpgrades(int buffStackCap, float buffPercent)
    {
        this.buffStackCap = buffStackCap;
        this.buffPercent = buffPercent;
    }

    public void AddBuffStack()
    {
        if (buffStacks < buffStackCap) buffStacks++;

        hasBuffStacks = true;
        buffStackTimer = buffStackMaxTime;

        Debug.Log("added buff stack");
        Debug.Log($"current buff stacks: {buffStacks}");
    }

    public float GetDamageBuffIncrease()
    {
        return (float)buffStacks * buffPercent;
    }

    /*
     * for use if the basedamage gets moved to this script
    public float GetDamageDealt()
    {
        return baseDamage * GetDamageBuffIncrease();
    }
    */







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
