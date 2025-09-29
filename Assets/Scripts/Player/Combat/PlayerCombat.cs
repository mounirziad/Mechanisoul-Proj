using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    // Camera zoom while aiming
    [SerializeField] private CinemachineCamera cinemachineCam;
    [SerializeField] private float zoomedFOV = 30f;
    [SerializeField] private float zoomSpeed = 5f;
    private float defaultFOV;

    // Ranged
    [Header("Ranged")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float projectileDamage = 10f;
    [Tooltip("Baseline shots per second (Joy multiplies this).")]
    public float baseFireRate = 2f;
    public bool rangedEnabled = true;
    public GameObject angerExplosionPrefab; // FireDoTZone-like prefab (optional)

    private float lastShotTime = -999f;
    private RangedModifiers rangedMods;    // set by UpgradeHandler

    // Melee & input
    public List<AttackSO> combo;
    float lastClickedTime, lastComboEnd;
    int comboCounter;
    public bool isAttacking = false;
    private float minAnimationPlayTime = 0.4f;
    private float attackStartTime;
    private bool attackQueued = false;

    public bool isAiming = false;
    private InputManager inputManager;
    private PlayerControls playerControls;
    private Animator anim;
    [SerializeField] Weapon weapon;

    [SerializeField] private float aimAssistRange = 20f;
    [SerializeField] private float aimAssistAngle = 30f;
    [SerializeField] private LayerMask enemyLayer;
    [HideInInspector] public Transform currentTarget;

    void Awake()
    {
        anim = GetComponent<Animator>();
        inputManager = GetComponent<InputManager>();
    }

    void Start()
    {
        if (cinemachineCam != null) defaultFOV = cinemachineCam.Lens.FieldOfView;
        if (inputManager != null && inputManager.playerControls != null)
        {
            playerControls = inputManager.playerControls;
            playerControls.PlayerActions.Attack.performed += OnAttackPerformed;
        }
        if (weapon != null) weapon.DisableTriggerBox();
    }

    void OnDisable()
    {
        if (playerControls != null)
            playerControls.PlayerActions.Attack.performed -= OnAttackPerformed;
    }

    void Update()
    {
        CheckAttackCompletion();
        ProcessQueuedAttack();
        HandleAiming();
        HandleCameraZoom();
    }

    void HandleCameraZoom()
    {
        if (cinemachineCam == null) return;
        float targetFOV = isAiming ? zoomedFOV : defaultFOV;
        var lens = cinemachineCam.Lens;
        lens.FieldOfView = Mathf.Lerp(lens.FieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
        cinemachineCam.Lens = lens;
    }

    void HandleAiming()
    {
        if (inputManager.aimInput && !isAttacking && !isAiming) StartAiming();
        else if (isAiming && !inputManager.aimInput) StopAiming();

        if (isAiming && inputManager.shootInput) HandleShoot();
    }

    void StartAiming()
    {
        isAiming = true;
        anim.SetBool("IsAiming", true);
        var loco = GetComponent<PlayerLocomotion>();
        if (loco) { loco.walkingSpeed *= 0.7f; loco.runningSpeed *= 0.7f; }
    }

    void StopAiming()
    {
        isAiming = false;
        anim.SetBool("IsAiming", false);
        var loco = GetComponent<PlayerLocomotion>();
        if (loco) { loco.walkingSpeed /= 0.7f; loco.runningSpeed /= 0.7f; }
    }

    // Ranged API 
    public void SetRangedUpgrades(RangedModifiers mods)
    {
        rangedMods = mods;
        if (rangedMods.joyFireRateMultiplier <= 0f) rangedMods.joyFireRateMultiplier = 1f;
        if (rangedMods.joyDamageMultiplier <= 0f) rangedMods.joyDamageMultiplier = 1f;
    }

    public void ToggleRanged(bool on)
    {
        rangedEnabled = on;
        if (!on) { isAiming = false; anim.SetBool("IsAiming", false); }
    }

    void HandleShoot()
    {
        if (!rangedEnabled) { inputManager.shootInput = false; return; }

        float effectiveRate = baseFireRate * rangedMods.joyFireRateMultiplier;
        float cooldown = 1f / Mathf.Max(0.0001f, effectiveRate);
        if (Time.time - lastShotTime < cooldown) { inputManager.shootInput = false; return; }

        if (projectilePrefab && shootPoint)
        {
            var go = Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);
            var proj = go.GetComponent<PlayerProjectile>();
            if (proj != null)
            {
                float finalDamage = projectileDamage * rangedMods.joyDamageMultiplier;
                proj.Initialize(shootPoint.forward, finalDamage, rangedMods, this);
            }
            lastShotTime = Time.time;
        }
        else Debug.LogWarning("Projectile Prefab or Shoot Point not assigned.");

        inputManager.shootInput = false;
    }

    // Melee (unchanged) 
    private void OnAttackPerformed(InputAction.CallbackContext ctx)
    {
        if (isAiming) { inputManager.shootInput = true; return; }
        if (CanAttack()) Attack();
        else if (isAttacking) attackQueued = true;
    }

    bool CanAttack() => !isAttacking && !isAiming && Time.time - lastComboEnd > 0.2f;

    private void SetAttackTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, aimAssistRange);
        float closest = Mathf.Infinity; Transform nearest = null;
        foreach (var h in hits)
        {
            if (!h.CompareTag("Enemy")) continue;
            Vector3 d = h.transform.position - transform.position;
            float ang = Vector3.Angle(transform.forward, d);
            if (ang < aimAssistAngle)
            {
                float dist = d.sqrMagnitude;
                if (dist < closest) { closest = dist; nearest = h.transform; }
            }
        }
        currentTarget = nearest;
    }

    void Attack()
    {
        if (comboCounter < combo.Count && combo[comboCounter] != null)
        {
            CancelInvoke(nameof(EndCombo));
            anim.runtimeAnimatorController = combo[comboCounter].animatorOV;
            anim.Play("Attack", 0, 0);
            weapon.damage = combo[comboCounter].damage;

            SetAttackTarget();
            isAttacking = true;
            attackStartTime = Time.time;
            comboCounter++;
            lastClickedTime = Time.time;
            attackQueued = false;
            if (comboCounter >= combo.Count) comboCounter = 0;
        }
    }

    void CheckAttackCompletion()
    {
        if (!isAttacking) return;
        float norm = anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
        if (Time.time - attackStartTime >= minAnimationPlayTime && norm > 0.7f)
            if (attackQueued) isAttacking = false;

        if (norm > 0.95f && anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
            CompleteAttack();
    }

    void ProcessQueuedAttack()
    {
        if (attackQueued && !isAttacking && Time.time - attackStartTime >= minAnimationPlayTime) Attack();
    }

    void CompleteAttack()
    {
        isAttacking = false;
        if (!attackQueued) Invoke(nameof(EndCombo), 0.5f);
        currentTarget = null;
    }

    public bool IsAttacking() => isAttacking;

    void EndCombo()
    {
        if (!isAttacking)
        {
            comboCounter = 0;
            lastComboEnd = Time.time;
            attackQueued = false;
        }
    }

    // Animation events
    public void OnAnimationEnableWeapon() { if (weapon != null) weapon.EnableTriggerBox(); }
    public void OnAnimationDisableWeapon() { if (weapon != null) weapon.DisableTriggerBox(); }
    public void CancelAttack() { isAttacking = false; attackQueued = false; anim.Play("Idle"); currentTarget = null; }
}
