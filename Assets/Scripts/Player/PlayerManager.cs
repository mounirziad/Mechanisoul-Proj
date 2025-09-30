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

    [Header("Upgrade Values")]
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
