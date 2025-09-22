using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    InputManager inputManager;
    PlayerLocomotion playerLocomotion;
    Animator animator;
    public bool isInteracting;

    [Header("Upgrade Values")]
    [SerializeField] float aoeAmount;
    [SerializeField] float slowAmount;
    [SerializeField] float slowLength;
    [SerializeField] float lifeStealAmount;
    [SerializeField] float stunLength;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        inputManager = GetComponent<InputManager>();
        playerLocomotion = GetComponent<PlayerLocomotion>();
      

    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
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

    //used to update the upgrade values when new upgrades are chosen
    public void UpdateUpgrades(float aoeAmount, float slowAmount, float slowLength, float lifeStealAmount, float stunLength)
    {
        this.aoeAmount = aoeAmount;
        this.slowAmount = slowAmount;
        this.slowLength = slowLength;
        this.lifeStealAmount = lifeStealAmount;
        this.stunLength = stunLength;
    }
}
