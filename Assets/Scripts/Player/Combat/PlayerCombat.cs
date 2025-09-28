using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder;

public class PlayerCombat : MonoBehaviour
{
     //camera zoom variables
    [SerializeField] private CinemachineCamera cinemachineCam;
    [SerializeField] private float zoomedFOV = 30f;
    [SerializeField] private float zoomSpeed = 5f;

    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint; // Where projectiles spawn (e.g., barrel of gun)
    [SerializeField] private float projectileDamage = 10f;


    private float defaultFOV;


    public List<AttackSO> combo;
    float lastClickedTime;
    float lastComboEnd;
    int comboCounter;

    // Add these new variables
    public bool isAttacking = false;
    private float minAnimationPlayTime = 0.4f;
    private float attackStartTime;
    private bool attackQueued = false;

    // Aiming variables
    public bool isAiming = false;
    private InputManager inputManager;
    private PlayerControls playerControls;
    private Animator anim;
    [SerializeField] Weapon weapon;

    [SerializeField] private float aimAssistRange = 20f;
    [SerializeField] private float aimAssistAngle = 30f; // Degrees cone of assist
    [SerializeField] private LayerMask enemyLayer;

    [HideInInspector] public Transform currentTarget;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        inputManager = GetComponent<InputManager>();
    }

    void Start()
    {
        if (cinemachineCam != null)
        {
            defaultFOV = cinemachineCam.Lens.FieldOfView;
        }

        if (inputManager != null && inputManager.playerControls != null)
        {
            playerControls = inputManager.playerControls;
            playerControls.PlayerActions.Attack.performed += OnAttackPerformed;
        }

        if (weapon != null)
        {
            weapon.DisableTriggerBox();
        }

       

    }

    private void OnDisable()
    {
        if (playerControls != null)
        {
            playerControls.PlayerActions.Attack.performed -= OnAttackPerformed;
        }
    }

    void Update()
    {
        CheckAttackCompletion();
        ProcessQueuedAttack();
        HandleAiming(); // Handle aiming state
        HandleCameraZoom();
    }

    private void HandleCameraZoom()
    {
        if (cinemachineCam == null) return;

        float targetFOV = isAiming ? zoomedFOV : defaultFOV;

        // Read lens, modify, write back
        var lens = cinemachineCam.Lens;
        lens.FieldOfView = Mathf.Lerp(lens.FieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
        cinemachineCam.Lens = lens;
    }

    private void HandleAiming()
    {
        // Start aiming when right click is pressed and not attacking
        if (inputManager.aimInput && !isAttacking && !isAiming)
        {
            StartAiming();
        }
        // Stop aiming when right click is released
        else if (isAiming && !inputManager.aimInput)
        {
            StopAiming();
        }

        // Handle shoot input while aiming
        if (isAiming && inputManager.shootInput)
        {
            HandleShoot();
        }
    }

    private void StartAiming()
    {
        isAiming = true;
        anim.SetBool("IsAiming", true);

        // Optional: Reduce movement speed while aiming
        PlayerLocomotion playerLocomotion = GetComponent<PlayerLocomotion>();
        if (playerLocomotion != null)
        {
            playerLocomotion.walkingSpeed *= 0.7f;
            playerLocomotion.runningSpeed *= 0.7f;
        }

        Debug.Log("Started Aiming");
    }

    private void StopAiming()
    {
        isAiming = false;
        anim.SetBool("IsAiming", false);

        // Restore movement speed
        PlayerLocomotion playerLocomotion = GetComponent<PlayerLocomotion>();
        if (playerLocomotion != null)
        {
            playerLocomotion.walkingSpeed /= 0.7f;
            playerLocomotion.runningSpeed /= 0.7f;
        }

        Debug.Log("Stopped Aiming");
    }

    private void HandleShoot()
    {
        if (projectilePrefab != null && shootPoint != null)
        {
            GameObject proj = Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);
            PlayerProjectile projectileScript = proj.GetComponent<PlayerProjectile>();

            if (projectileScript != null)
            {
                projectileScript.Initialize(shootPoint.forward, projectileDamage);
            }
        }
        else
        {
            Debug.LogWarning("Projectile Prefab or Shoot Point not assigned.");
        }
    }

    // Event-based approach instead of polling
    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        // If aiming, handle as shoot input instead of melee attack
        if (isAiming)
        {
            inputManager.shootInput = true;
            return;
        }

        if (CanAttack())
        {
            Attack();
        }
        else if (isAttacking)
        {
            // Queue the attack for later
            attackQueued = true;
        }
    }

    bool CanAttack()
    {
        // Can't attack while aiming
        return !isAttacking && !isAiming && Time.time - lastComboEnd > 0.2f;
    }

    private void SetAttackTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, aimAssistRange);

        float closestDist = Mathf.Infinity;
        Transform nearestEnemy = null;

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Enemy")) 
            {
                Vector3 dirToEnemy = hit.transform.position - transform.position;
                float angle = Vector3.Angle(transform.forward, dirToEnemy);

                if (angle < aimAssistAngle)
                {
                    float dist = dirToEnemy.sqrMagnitude;
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        nearestEnemy = hit.transform;
                    }
                }
            }
        }

        currentTarget = nearestEnemy;
    }


    void Attack()
    {
        if (comboCounter < combo.Count && combo[comboCounter] != null)
        {
            CancelInvoke("EndCombo");

            // Set up the attack
            anim.runtimeAnimatorController = combo[comboCounter].animatorOV;
            anim.Play("Attack", 0, 0);
            weapon.damage = combo[comboCounter].damage;



            // Update state
            SetAttackTarget();
            isAttacking = true;
            attackStartTime = Time.time;
            comboCounter++;
            lastClickedTime = Time.time;
            attackQueued = false;

            if (comboCounter >= combo.Count)
            {
                comboCounter = 0;
            }
        }
    }

    void CheckAttackCompletion()
    {
        if (isAttacking)
        {
            // Check if the current animation has played enough
            float normalizedTime = anim.GetCurrentAnimatorStateInfo(0).normalizedTime;

            // Allow next attack only after minimum play time has passed
            if (Time.time - attackStartTime >= minAnimationPlayTime && normalizedTime > 0.7f)
            {
                // Ready for next attack if one is queued
                if (attackQueued)
                {
                    isAttacking = false;
                }
            }

            // Check if animation is completely finished
            if (normalizedTime > 0.95f && anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
            {
                CompleteAttack();
            }
        }
    }

    void ProcessQueuedAttack()
    {
        if (attackQueued && !isAttacking && Time.time - attackStartTime >= minAnimationPlayTime)
        {
            Attack();
        }
    }

    void CompleteAttack()
    {
        isAttacking = false;

        // If no attack is queued and we're at the end of combo, start ending the combo
        if (!attackQueued)
        {
            Invoke("EndCombo", 0.5f); // Give a small buffer before combo ends
        }
        currentTarget = null;
    }

    public bool IsAttacking()
    {
        return isAttacking;
    }

    void EndCombo()
    {
        // Only end combo if no new attack has started
        if (!isAttacking)
        {
            comboCounter = 0;
            lastComboEnd = Time.time;
            attackQueued = false;
        }
    }

    public void OnAnimationEnableWeapon()
    {
        if (weapon != null)
        {
            weapon.EnableTriggerBox();
        }
    }

    public void OnAnimationDisableWeapon()
    {
        if (weapon != null)
        {
            weapon.DisableTriggerBox();
        }
    }

    public void CancelAttack()
    {
        isAttacking = false;
        attackQueued = false;
        anim.Play("Idle"); // fallback animation
        currentTarget = null;
    }

}