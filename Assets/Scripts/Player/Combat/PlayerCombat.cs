using System.Collections;
using System.Collections.Generic;
using Unity.AppUI.UI;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public enum ShootingMode
{
    Projectile,
    Hitscan
}

public enum RootMotionMode
{
    LungeOnly,
    RootMotionOnly,
    Hybrid
}

public class PlayerCombat : MonoBehaviour
{
    [Header("Aiming Settings")]
    [SerializeField] private bool useSpawnCorrection = true;
    [SerializeField] private float maxSpawnOffset = 0.5f;
    [Header("Debug")]
    [SerializeField] private bool showAimDebug = false;

    // Camera zoom while aiming
    [SerializeField] private CinemachineCamera cinemachineCam;
    [SerializeField] private float zoomedFOV = 30f;
    [SerializeField] private float zoomSpeed = 5f;
    private float defaultFOV;
    private CombatCameraController combatCameraController;

    // Ranged
    [Header("Ranged Settings")]
    [Tooltip("Choose between projectile-based or instant hitscan shooting.")]
    public ShootingMode shootingMode = ShootingMode.Projectile;

    [Header("Shared Ranged Settings")]
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float weaponDamage = 10f;
    [Tooltip("Baseline shots per second (Joy multiplies this).")]
    public float baseFireRate = 2f;
    public bool rangedEnabled = true;

    [Header("Projectile Settings")]
    [SerializeField] private GameObject projectilePrefab;

    [Header("Hitscan Settings")]
    [SerializeField] private float hitscanRange = 100f;
    [SerializeField] private LayerMask hitscanLayerMask = -1;
    [SerializeField] private GameObject impactEffectPrefab;
    [SerializeField] private float tracerDuration = 0.1f;

    [Header("Ranged VFX References")]
    [SerializeField] private GameObject baseMuzzleFlashPrefab;
    [SerializeField] private GameObject angerMuzzleFlashPrefab;
    [SerializeField] private GameObject joyMuzzleFlashPrefab;
    [SerializeField] private GameObject fearMuzzleFlashPrefab;
    [SerializeField] private GameObject sadnessMuzzleFlashPrefab;
    [SerializeField] private GameObject loveMuzzleFlashPrefab;

    [SerializeField] private LineRenderer baseTracerLinePrefab;
    [SerializeField] private LineRenderer angerTracerLinePrefab;
    [SerializeField] private LineRenderer joyTracerLinePrefab;
    [SerializeField] private LineRenderer fearTracerLinePrefab;
    [SerializeField] private LineRenderer sadnessTracerLinePrefab;
    [SerializeField] private LineRenderer loveTracerLinePrefab;

    [SerializeField] private GameObject currentMuzzleFlashPrefab;
    [SerializeField] private LineRenderer currentTracerLinePrefab;
    private Emotions currentRangedEmotion = Emotions.None;

    [Header("Ammo Settings")]
    [SerializeField] int clipSize = 6;
    [SerializeField] float reloadTime = 2f;
    [SerializeField] float autoReloadDelay = 1f;
    [SerializeField] float baseFireCooldown = 0.20f;

    int ammoInClip;
    bool isReloading;
    float fireCooldown;       // computed from joyFireRateMultiplier
    float lastFireInputTime;  // to detect "gun not in use"
    private float lastShotTime = -999f;
    Coroutine autoReloadCR;
    Coroutine reloadCR; // track active reload


    [Header("VFX Prefabs")]
    public GameObject angerExplosionPrefab;
    public GameObject joyExplosionPrefab;

    private RangedModifiers rangedMods;

    // Melee & input
    public List<AttackSO> combo;
    float lastClickedTime, lastComboEnd;
    int comboCounter;
    public bool isAttacking = false;
    private float minAnimationPlayTime = 0.4f;
    private float attackStartTime;
    private bool attackQueued = false;

    [Header("Input Buffer Settings")]
    public float inputBufferTime = 0.3f;    // How long to buffer attack inputs
    private float lastAttackInputTime = -999f;

    [Header("Animation Cancel Settings")]
    public float earlyComboWindow = 0.6f;   // When in animation you can start next combo (0.6 = 60% through)
    public float dodgeCancelWindow = 0.4f;  // When you can cancel attack with dodge (0.4 = 40% through)

    [Header("Air Attack Settings")]
    [Tooltip("Allow limited air attacks (prevents infinite air combos)")]
    public bool allowAirAttacks = false;
    [Tooltip("Maximum number of attacks allowed in air before landing")]
    public int maxAirAttacks = 1;
    private int currentAirAttackCount = 0;
    private bool wasGroundedLastFrame = true;

    public bool isAiming = false;
    private InputManager inputManager;
    private PlayerControls playerControls;
    private Animator anim;
    [SerializeField] Weapon weapon;
    private AttackAnimationManager attackAnimManager;
    private CameraManagerAdapter cameraAdapter;

    [Header("Root Motion Settings")]
    [Tooltip("How to combine root motion with attack lunge movement")]
    public RootMotionMode rootMotionMode = RootMotionMode.Hybrid;

    [Tooltip("When using Additive mode, multiplier for extra lunge force")]
    [Range(0.1f, 2f)]
    public float additionalLungeMultiplier = 0.5f;

    [SerializeField] private float aimAssistRange = 20f;
    [SerializeField] private float aimAssistAngle = 30f;
    [SerializeField] private LayerMask enemyLayer;
    [HideInInspector] public Transform currentTarget;
    private AttackSO currentAttackData;

    void Awake()
    {
        anim = GetComponent<Animator>();
        inputManager = GetComponent<InputManager>();
        attackAnimManager = GetComponent<AttackAnimationManager>();

        cameraAdapter = GetComponent<CameraManagerAdapter>();
        if (cameraAdapter == null)
        {
            cameraAdapter = gameObject.AddComponent<CameraManagerAdapter>();
        }

        if (attackAnimManager == null)
        {
            attackAnimManager = gameObject.AddComponent<AttackAnimationManager>();
        }
    }

    void Start()
    {
        if (cinemachineCam != null)
        {
            defaultFOV = cinemachineCam.Lens.FieldOfView;
            combatCameraController = cinemachineCam.GetComponent<CombatCameraController>();
        }
        if (inputManager != null && inputManager.playerControls != null)
        {
            playerControls = inputManager.playerControls;
            playerControls.PlayerActions.Attack.performed += OnAttackPerformed;
        }
        if (weapon != null) weapon.DisableTriggerBox();

        // Ensure root motion is disabled initially
        if (anim != null)
        {
            anim.applyRootMotion = false;
        }

        InitRangedGun();

        currentMuzzleFlashPrefab = baseMuzzleFlashPrefab;
        currentTracerLinePrefab = baseTracerLinePrefab;
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
        ClearQueuedAttacksInAir();
        TrackGroundedState();
    }

    void TrackGroundedState()
    {
        var playerLoco = GetComponent<PlayerLocomotion>();
        bool isGrounded = playerLoco != null && playerLoco.isGrounded;

        // Reset air attack counter when landing
        if (isGrounded && !wasGroundedLastFrame)
        {
            currentAirAttackCount = 0;
        }

        wasGroundedLastFrame = isGrounded;
    }

    void ClearQueuedAttacksInAir()
    {
        if (attackQueued)
        {
            var playerLoco = GetComponent<PlayerLocomotion>();
            var playerMgr = GetComponent<PlayerManager>();
            bool isGrounded = playerLoco != null && playerLoco.isGrounded;
            bool isInteracting = playerMgr != null && playerMgr.isInteracting;

            // Clear queued attacks when airborne (if air attacks disabled) or when interacting
            if (isInteracting || (!isGrounded && !allowAirAttacks))
            {
                attackQueued = false;
            }
        }
    }

    void HandleCameraZoom()
    {
        if (cinemachineCam == null) return;

        if (combatCameraController != null && combatCameraController.IsInCombatZone)
        {
            return;
        }

        float targetFOV = isAiming ? zoomedFOV : defaultFOV;
        var lens = cinemachineCam.Lens;
        lens.FieldOfView = Mathf.Lerp(lens.FieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
        cinemachineCam.Lens = lens;
    }

    void HandleAiming()
    {
        bool isDialogueActive = DialogueSystem.Instance != null && DialogueSystem.Instance.IsDisplaying;
        bool isLoadingActive = WarehouseLoadingScreen.Instance != null && WarehouseLoadingScreen.Instance.IsDisplaying;

        if (isDialogueActive || isLoadingActive)
        {
            if (isAiming) StopAiming();
            return;
        }

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
        RecalcFireCooldown(mods.joyFireRateMultiplier);
        LogGun($"SetRangedUpgrades applied: ammo={ammoInClip}/{clipSize}");
    }

    public void SetRangedEmotion(Emotions emotion)
    {
        switch (emotion)
        {
            case Emotions.Joy:
                currentMuzzleFlashPrefab = joyMuzzleFlashPrefab ?? baseMuzzleFlashPrefab;
                currentTracerLinePrefab = joyTracerLinePrefab ?? baseTracerLinePrefab;
                break;
            case Emotions.Anger:
                currentMuzzleFlashPrefab = angerMuzzleFlashPrefab ?? baseMuzzleFlashPrefab;
                currentTracerLinePrefab = angerTracerLinePrefab ?? baseTracerLinePrefab;
                break;
            case Emotions.Sadness:
                currentMuzzleFlashPrefab = sadnessMuzzleFlashPrefab ?? baseMuzzleFlashPrefab;
                currentTracerLinePrefab = sadnessTracerLinePrefab ?? baseTracerLinePrefab;
                break;
            case Emotions.Love:
                currentMuzzleFlashPrefab = loveMuzzleFlashPrefab ?? baseMuzzleFlashPrefab;
                currentTracerLinePrefab = loveTracerLinePrefab ?? baseTracerLinePrefab;
                break;
            case Emotions.Fear:
                currentMuzzleFlashPrefab = fearMuzzleFlashPrefab ?? baseMuzzleFlashPrefab;
                currentTracerLinePrefab = fearTracerLinePrefab ?? baseTracerLinePrefab;
                break;
            case Emotions.None:
            default:
                currentMuzzleFlashPrefab = baseMuzzleFlashPrefab;
                currentTracerLinePrefab = baseTracerLinePrefab;
                break;
        }
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

        if (shootingMode == ShootingMode.Projectile)
        {
            TryFireRanged();
        }
        else
        {
            // Route hitscan through ammo/reload as well
            TryFireRanged();
        }

        lastShotTime = Time.time;
        inputManager.shootInput = false;
    }

    void ShootProjectile()
    {
        if (projectilePrefab == null || shootPoint == null)
        {
            Debug.LogWarning("Projectile Prefab or Shoot Point not assigned.");
            return;
        }

        Vector3 spawnPosition = GetCorrectedSpawnPosition();
        var go = Instantiate(projectilePrefab, spawnPosition, shootPoint.rotation);
        var proj = go.GetComponent<PlayerProjectile>();

        if (proj != null)
        {
            float finalDamage = weaponDamage * rangedMods.joyDamageMultiplier;
            Vector3 correctedDirection = GetCorrectedAimDirection();

            float targetDistance = 0f;
            if (cameraAdapter != null)
            {
                Vector3 aimTarget = cameraAdapter.GetAimTarget();
                if (aimTarget != Vector3.zero)
                {
                    targetDistance = Vector3.Distance(spawnPosition, aimTarget);
                }
            }

            proj.Initialize(correctedDirection, finalDamage, rangedMods, this, targetDistance);
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayLaserSound();
        }
    }

    void ShootHitscan()
    {
        if (shootPoint == null)
        {
            Debug.LogWarning("Shoot Point not assigned for hitscan.");
            return;
        }

        Vector3 aimTarget = GetCameraCenterAimPoint();
        Vector3 shootOrigin = shootPoint.position;
        Vector3 shootDirection = (aimTarget - shootOrigin).normalized;

        float distanceToTarget = Vector3.Distance(shootOrigin, aimTarget);
        float effectiveRange = Mathf.Max(hitscanRange, distanceToTarget + 10f);

        if (showAimDebug)
        {
            Debug.Log($"Shoot Origin: {shootOrigin}, Aim Target: {aimTarget}, Direction: {shootDirection}, Range: {effectiveRange}");
            Debug.DrawLine(shootOrigin, aimTarget, Color.yellow, 2f);
            Debug.DrawRay(shootOrigin, shootDirection * effectiveRange, Color.red, 2f);
        }

        float finalDamage = weaponDamage * rangedMods.joyDamageMultiplier;

        if (currentMuzzleFlashPrefab != null)
        {
            var muzzle = Instantiate(currentMuzzleFlashPrefab, shootOrigin, Quaternion.LookRotation(shootDirection));
            Destroy(muzzle, 0.1f);
        }

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayLaserSound();
        }

        RaycastHit hit;
        Vector3 hitPosition;
        bool hitSomething = Physics.Raycast(shootOrigin, shootDirection, out hit, effectiveRange, hitscanLayerMask, QueryTriggerInteraction.Ignore);

        if (hitSomething)
        {
            hitPosition = hit.point;
            float hitDistance = Vector3.Distance(shootOrigin, hitPosition);

            Debug.Log($"[Hitscan] Hit '{hit.collider.gameObject.name}' on layer '{LayerMask.LayerToName(hit.collider.gameObject.layer)}' at distance {hitDistance:F2}m (Target was {distanceToTarget:F2}m away)");

            if (hit.collider.CompareTag("Player"))
            {
                Debug.LogWarning($"[Hitscan] Hit player's own collider! Skipping this hit.");
                hitPosition = shootOrigin + shootDirection * effectiveRange;
            }
            else
            {
                var enemyHealth = hit.collider.GetComponentInParent<BasicEnemyHealth>();
                if (enemyHealth != null)
                {
                    RangedHitscanHelper.Apply(
                        weaponDamage,
                        rangedMods,
                        enemyHealth,
                        hitPosition,
                        shootDirection,
                        hit.collider.transform,
                        enemyLayer
                    );

                    SpawnHitscanVFX(hitPosition, hit.normal);
                }

                else if (hit.collider.CompareTag("Enemy") || (hit.transform.root != null && hit.transform.root.CompareTag("Enemy")))
                {
                    SpawnHitscanVFX(hitPosition, hit.normal);
                }
                else
                {
                    SpawnImpactEffect(hitPosition, hit.normal);
                }
            }
        }
        else
        {
            hitPosition = shootOrigin + shootDirection * effectiveRange;
            Debug.Log($"[Hitscan] No hit detected within range {effectiveRange:F2}m");
        }

        if (currentTracerLinePrefab != null)
        {
            DrawTracerLine(shootOrigin, hitPosition);
        }
    }

    void SpawnHitscanVFX(Vector3 position, Vector3 normal)
    {
        if (rangedMods.angerExplosionOnHit && angerExplosionPrefab != null)
        {
            var aoe = Instantiate(angerExplosionPrefab, position, Quaternion.LookRotation(normal));
            var cameraLookAt = aoe.GetComponent<VFXCameraLookAt>();
            if (cameraLookAt != null) cameraLookAt.enabled = false;

            var dot = aoe.GetComponent<AngerBurstZone>();
            if (dot != null)
                dot.Configure(Mathf.Max(0f, rangedMods.angerAOEPercent));
            else
                aoe.SendMessage("Configure", rangedMods.angerAOEPercent, SendMessageOptions.DontRequireReceiver);
        }

        if (rangedMods.joyExplosionOnHit && joyExplosionPrefab != null)
        {
            var vfx = Instantiate(joyExplosionPrefab, position, Quaternion.LookRotation(normal));
            var cameraLookAt = vfx.GetComponent<VFXCameraLookAt>();
            if (cameraLookAt != null) cameraLookAt.enabled = false;

            AutoDestroyVFX(vfx);
        }
    }

    void SpawnImpactEffect(Vector3 position, Vector3 normal)
    {
        if (impactEffectPrefab != null)
        {
            var impact = Instantiate(impactEffectPrefab, position, Quaternion.LookRotation(normal));
            Destroy(impact, 2f);
        }
    }

    void DrawTracerLine(Vector3 start, Vector3 end)
    {
        var tracerObj = Instantiate(currentTracerLinePrefab, start, Quaternion.identity);
        tracerObj.SetPosition(0, start);
        tracerObj.SetPosition(1, end);
        Destroy(tracerObj.gameObject, tracerDuration);
    }

    void AutoDestroyVFX(GameObject go)
    {
        float fallback = 2f;
        float maxTime = 0f;

        var psList = go.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in psList)
        {
            var m = ps.main;
            float dur = m.duration + m.startLifetime.constantMax;
            if (dur > maxTime) maxTime = dur;
        }

        Destroy(go, maxTime > 0.05f ? maxTime : fallback);
    }

    Vector3 GetAimTargetPosition()
    {
        var aimCameraManager = GetComponent<ThirdPersonAimCameraManager>();
        if (aimCameraManager != null && aimCameraManager.IsAimCameraActive())
        {
            Vector3 aimTarget = aimCameraManager.GetAimTarget();

            // Validate that the aim target is reasonable (not too close or at origin)
            if (aimTarget != Vector3.zero)
            {
                float distanceToTarget = Vector3.Distance(shootPoint.position, aimTarget);
                if (distanceToTarget > 1f) // Must be at least 1 meter away
                {
                    return aimTarget;
                }
            }
        }

        return Vector3.zero; // No valid target found
    }

    Vector3 GetAccurateAimDirection()
    {
        if (cameraAdapter != null)
        {
            Vector3 aimTarget = cameraAdapter.GetAimTarget();

            if (aimTarget != Vector3.zero)
            {
                Vector3 cameraPos = GetCameraPosition();
                Vector3 direction = (aimTarget - cameraPos).normalized;
                return direction;
            }
        }

        if (cameraAdapter != null && cameraAdapter.IsAimCameraActive())
        {
            Vector3 cameraDirection = cameraAdapter.GetAimDirection();
            if (cameraDirection != Vector3.zero)
            {
                return cameraDirection;
            }
        }

        return shootPoint.forward;
    }

    Vector3 GetCorrectedAimDirection()
    {
        if (cameraAdapter != null && cameraAdapter.IsAimCameraActive())
        {
            Vector3 aimTarget = cameraAdapter.GetAimTarget();

            if (aimTarget != Vector3.zero)
            {
                Vector3 cameraPos = GetCameraPosition();
                Vector3 cameraToTargetDirection = (aimTarget - cameraPos).normalized;

                return cameraToTargetDirection;
            }
        }

        return GetAccurateAimDirection();
    }

    Vector3 CalculateTrajectoryCorrection(Vector3 shootOrigin, Vector3 cameraOrigin, Vector3 target, Vector3 idealDirection)
    {
        // Distance to target from camera
        float distanceToTarget = Vector3.Distance(cameraOrigin, target);

        // Calculate a convergence point along the ideal trajectory
        // The closer the target, the more correction we need
        float convergenceDistance = Mathf.Min(distanceToTarget * 0.3f, 10f); // Converge within 30% of distance or 10m max
        Vector3 convergencePoint = cameraOrigin + idealDirection * convergenceDistance;

        // Calculate direction from actual shoot point to convergence point
        Vector3 correctedDirection = (convergencePoint - shootOrigin).normalized;

        // Blend between corrected direction (for close targets) and direct direction (for far targets)
        float blendFactor = Mathf.Clamp01(20f / distanceToTarget); // More correction for closer targets
        Vector3 directDirection = (target - shootOrigin).normalized;

        return Vector3.Slerp(directDirection, correctedDirection, blendFactor).normalized;
    }

    Vector3 GetCameraPosition()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            return mainCam.transform.position;
        }

        return transform.position + Vector3.up * 1.6f;
    }

    Vector3 GetCorrectedSpawnPosition()
    {
        if (!useSpawnCorrection) return shootPoint.position;

        if (cameraAdapter == null || !cameraAdapter.IsAimCameraActive()) return shootPoint.position;

        Vector3 aimTarget = cameraAdapter.GetAimTarget();
        if (aimTarget == Vector3.zero) return shootPoint.position;

        Vector3 cameraPos = GetCameraPosition();

        Vector3 idealDirection = (aimTarget - cameraPos).normalized;

        Vector3 cameraToShoot = shootPoint.position - cameraPos;
        float projectionDistance = Vector3.Dot(cameraToShoot, idealDirection);
        Vector3 projectedPoint = cameraPos + idealDirection * projectionDistance;

        Vector3 offset = projectedPoint - shootPoint.position;

        if (offset.magnitude > maxSpawnOffset)
        {
            offset = offset.normalized * maxSpawnOffset;
        }

        return shootPoint.position + offset;
    }


    // ====== Ranged Ammo System with Debug Logs ======
    void LogGun(string msg) { Debug.Log($"[RangedGun] {msg}"); }

    void InitRangedGun()
    {
        ammoInClip = clipSize;
        RecalcFireCooldown(1f);
        LogGun($"Init clip={clipSize}, ammo={ammoInClip}/{clipSize}, fireCD={fireCooldown:0.00}s, reload={reloadTime:0.00}s");
    }

    void RecalcFireCooldown(float joyFireRateMultiplier)
    {
        float prev = fireCooldown;
        fireCooldown = Mathf.Max(0.05f, baseFireCooldown / Mathf.Max(0.01f, joyFireRateMultiplier));
        LogGun($"RecalcFireCooldown: joyFRx={joyFireRateMultiplier:0.00} -> cooldown {prev:0.00}s => {fireCooldown:0.00}s");
    }

    public void TryFireRanged()
    {
        lastFireInputTime = Time.time;

        // Allow canceling reload if there are still bullets left
        if (isReloading)
        {
            if (ammoInClip > 0)
            {
                LogGun($"TryFireRanged: canceling reload to fire (ammo={ammoInClip}/{clipSize})");
                CancelReload();
            }
            else
            {
                LogGun($"TryFireRanged blocked: reloading from empty (ammo={ammoInClip}/{clipSize})");
                return;
            }
        }

        float sinceLast = Time.time - lastShotTime;
        if (sinceLast < fireCooldown)
        {
            LogGun($"TryFireRanged blocked: cooldown {fireCooldown - sinceLast:0.00}s remaining");
            return;
        }

        if (ammoInClip <= 0)
        {
            LogGun("Empty clip -> starting reload");
            StartReload();
            return;
        }

        // Fire using the selected shooting mode
        if (shootingMode == ShootingMode.Projectile)
        {
            ShootProjectile();
        }
        else
        {
            ShootHitscan();
        }

        ammoInClip--;
        lastShotTime = Time.time;
        LogGun($"Fired. Ammo now {ammoInClip}/{clipSize}");

        RestartIdleAutoReload();
    }

    void StartReload()
    {
        if (reloadCR != null)
        {
            LogGun("StartReload ignored: already reloading");
            return;
        }

        reloadCR = StartCoroutine(ReloadCR());
    }

    void CancelReload()
    {
        if (!isReloading) return;

        if (reloadCR != null)
        {
            StopCoroutine(reloadCR);
            reloadCR = null;
        }

        isReloading = false;
        LogGun($"Reload canceled (ammo={ammoInClip}/{clipSize})");
    }

    void RestartIdleAutoReload()
    {
        if (autoReloadCR != null) { StopCoroutine(autoReloadCR); autoReloadCR = null; }
        LogGun($"Idle auto-reload arming (delay={autoReloadDelay:0.00}s)");
        autoReloadCR = StartCoroutine(IdleAutoReloadCR());
    }

    IEnumerator IdleAutoReloadCR()
    {
        float armedAt = Time.time;
        yield return new WaitUntil(() => Time.time - lastFireInputTime >= autoReloadDelay);
        float waited = Time.time - armedAt;

        if (!isReloading && ammoInClip < clipSize)
        {
            LogGun($"Idle auto-reload triggered after {waited:0.00}s idle (ammo={ammoInClip}/{clipSize})");
            StartReload(); // instead of yield return ReloadCR();
        }
        else
        {
            LogGun($"Idle auto-reload canceled (reloading={isReloading}, ammo={ammoInClip}/{clipSize})");
        }
        autoReloadCR = null;

    }

    public void ManualReload()
    {
        if (isReloading) { LogGun("ManualReload ignored: already reloading"); return; }
        if (ammoInClip >= clipSize) { LogGun("ManualReload ignored: clip full"); return; }
        if (autoReloadCR != null) { StopCoroutine(autoReloadCR); autoReloadCR = null; }
        LogGun($"ManualReload started (ammo={ammoInClip}/{clipSize})");
        StartReload(); // instead of StartCoroutine(ReloadCR());
    }


    IEnumerator ReloadCR()
    {
        isReloading = true;
        LogGun($"Reload start (duration={reloadTime:0.00}s) ammo={ammoInClip}/{clipSize}");
        yield return new WaitForSeconds(reloadTime);
        ammoInClip = clipSize;
        isReloading = false;
        reloadCR = null;
        LogGun($"Reload complete -> ammo={ammoInClip}/{clipSize}");
    }


    public int GetAmmoInClip()
    {
        return ammoInClip;
    }

    public int GetClipSize()
    {
        return clipSize;
    }

    public bool IsReloading()
    {
        return isReloading;
    }

    // Melee (improved)
    private void OnAttackPerformed(InputAction.CallbackContext ctx)
    {
        bool isDialogueActive = DialogueSystem.Instance != null && DialogueSystem.Instance.IsDisplaying;
        bool isLoadingActive = WarehouseLoadingScreen.Instance != null && WarehouseLoadingScreen.Instance.IsDisplaying;

        if (isDialogueActive || isLoadingActive) return;

        if (isAiming) { inputManager.shootInput = true; return; }

        // Block attack input during landing (similar to jumping)
        var playerLoco = GetComponent<PlayerLocomotion>();
        if (playerLoco != null && playerLoco.isLanding)
        {
            return;
        }

        lastAttackInputTime = Time.time;

        if (CanAttack())
        {
            Attack();
        }
        else if (isAttacking)
        {
            // Buffer the attack input
            attackQueued = true;
        }
    }

    bool CanAttack()
    {
        var playerLoco = GetComponent<PlayerLocomotion>();
        var playerMgr = GetComponent<PlayerManager>();
        bool isGrounded = playerLoco != null && playerLoco.isGrounded;
        bool isInteracting = playerMgr != null && playerMgr.isInteracting;
        bool isLanding = playerLoco != null && playerLoco.isLanding;

        // Block attacks during landing (similar to jumping)
        if (isLanding)
            return false;

        // Basic checks first (including interaction/landing check)
        if (isAttacking || isAiming || isInteracting || Time.time - lastComboEnd < 0.2f)
            return false;

        // If grounded and not interacting, allow attacks
        if (isGrounded)
        {
            // Just landed - block attacks during landing transition
            if (!wasGroundedLastFrame)
            {
                currentAirAttackCount = 0;
                return false;
            }
            return true;
        }

        // If in air, check if air attacks are allowed and within limit
        if (allowAirAttacks && currentAirAttackCount < maxAirAttacks)
        {
            return true;
        }

        // Default: no attack allowed
        return false;
    }

    // Check for buffered attacks during animation
    void ProcessQueuedAttack()
    {
        bool isDialogueActive = DialogueSystem.Instance != null && DialogueSystem.Instance.IsDisplaying;
        bool isLoadingActive = WarehouseLoadingScreen.Instance != null && WarehouseLoadingScreen.Instance.IsDisplaying;

        if (isDialogueActive || isLoadingActive)
        {
            attackQueued = false;
            return;
        }

        if (!attackQueued || isAttacking) return;

        // Ensure player can still attack (check grounded, air limits, and interaction state)
        var playerLoco = GetComponent<PlayerLocomotion>();
        var playerMgr = GetComponent<PlayerManager>();
        bool isGrounded = playerLoco != null && playerLoco.isGrounded;
        bool isInteracting = playerMgr != null && playerMgr.isInteracting;
        bool isLanding = playerLoco != null && playerLoco.isLanding;

        // Cancel queued attack if player is landing
        if (isLanding)
        {
            attackQueued = false;
            return;
        }

        // Cancel queued attack if player is interacting (landing, dodging, etc.)
        if (isInteracting)
        {
            attackQueued = false;
            return;
        }

        // Cancel queued attack if player just landed
        if (isGrounded && !wasGroundedLastFrame)
        {
            attackQueued = false;
            return;
        }

        if (!isGrounded)
        {
            if (!allowAirAttacks || currentAirAttackCount >= maxAirAttacks)
            {
                attackQueued = false;
                return;
            }
        }

        // Check if we're in the combo window
        if (Time.time - attackStartTime >= minAnimationPlayTime)
        {
            float norm = anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
            if (norm >= earlyComboWindow || norm > 0.95f)
            {
                Attack();
            }
        }

        // Clear old buffered inputs
        if (Time.time - lastAttackInputTime > inputBufferTime)
        {
            attackQueued = false;
        }
    }

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
        // Double-check we can actually attack (prevents attack during landing)
        if (!CanAttack())
        {
            Debug.LogWarning("<color=yellow>Attack() called but CanAttack() is false - blocking attack.</color>");
            attackQueued = false;
            return;
        }

        if (comboCounter < combo.Count && combo[comboCounter] != null)
        {
            var playerMgr = GetComponent<PlayerManager>();
            CancelInvoke(nameof(EndCombo));

            // Get the current attack data before setting the animator
            currentAttackData = combo[comboCounter];

            anim.runtimeAnimatorController = currentAttackData.animatorOV;
            anim.Play("Attack", 0, 0);

            // Apply attack speed modifier using the dedicated manager with attack data
            if (attackAnimManager != null)
            {
                attackAnimManager.ApplyAttackSpeedToAnimation(currentAttackData);
            }

            weapon.damage = currentAttackData.damage * playerMgr.GetDamageMultiplier();

            // Reset weapon hit sound cooldown for new attack
            if (weapon != null)
            {
                weapon.ResetHitSoundCooldown();
            }

            // Play attack sound effect
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayAttackSound();
            }

            SetAttackTarget();

            // Track air attacks
            var playerLoco = GetComponent<PlayerLocomotion>();
            bool isGrounded = playerLoco != null && playerLoco.isGrounded;
            if (!isGrounded)
            {
                currentAirAttackCount++;
            }

            // Handle different root motion modes
            switch (rootMotionMode)
            {
                case RootMotionMode.LungeOnly:
                    anim.applyRootMotion = false;
                    break;

                case RootMotionMode.RootMotionOnly:
                    anim.applyRootMotion = true;
                    break;

                case RootMotionMode.Hybrid:
                    anim.applyRootMotion = true;
                    break;
            }

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

        // Allow early combo transitions for fluid combat
        if (Time.time - attackStartTime >= minAnimationPlayTime && norm >= earlyComboWindow)
        {
            if (attackQueued)
            {
                isAttacking = false;
                return;
            }
        }

        // Complete attack when animation is nearly finished
        // NOTE: Don't clear currentAttackData here - let animation events handle it
        if (norm > 0.95f && anim.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
            CompleteAttack();
    }

    void CompleteAttack()
    {
        isAttacking = false;

        // Reset animator speed to normal when attack completes using the manager
        if (attackAnimManager != null)
        {
            attackAnimManager.ResetAnimationSpeed();
        }

        // Disable root motion when attack completes (except for LungeOnly mode where it's already off)
        if (rootMotionMode != RootMotionMode.LungeOnly)
        {
            anim.applyRootMotion = false;
        }

        // End combo if no attack is queued
        if (!attackQueued)
        {
            Invoke(nameof(EndCombo), 0.5f);
        }

        currentTarget = null;
    }

    // Allow dodge canceling during attacks for anime-style mobility
    public bool CanDodgeCancel()
    {
        if (!isAttacking) return true;

        float norm = anim.GetCurrentAnimatorStateInfo(0).normalizedTime;
        return norm >= dodgeCancelWindow;
    }

    public bool IsAttacking() => isAttacking;

    void EndCombo()
    {
        if (!isAttacking)
        {
            // Reset animator speed to normal when combo ends using the manager
            if (attackAnimManager != null)
            {
                attackAnimManager.ResetAnimationSpeed();
            }

            comboCounter = 0;
            lastComboEnd = Time.time;
            attackQueued = false;
            currentAttackData = null;
        }
    }

    // Animation events
    public void OnAnimationEnableWeapon() { if (weapon != null) weapon.EnableTriggerBox(); }
    public void OnAnimationDisableWeapon() { if (weapon != null) weapon.DisableTriggerBox(); }

    public void StartAttackLunge()
    {
        if (!isAttacking && currentAttackData == null)
        {
            Debug.LogWarning($"<color=orange>[AnimEvent] StartAttackLunge called after attack ended. This is a timing issue - animation event fires too late.</color>");
            return;
        }

        var playerLoco = GetComponent<PlayerLocomotion>();

        // Block lunge during landing
        if (playerLoco != null && playerLoco.isLanding)
        {
            Debug.LogWarning($"<color=orange>[AnimEvent] StartAttackLunge blocked - player is landing.</color>");
            return;
        }

        // Block lunge during landing transition
        if (playerLoco != null && playerLoco.isGrounded && !wasGroundedLastFrame)
        {
            Debug.LogWarning($"<color=orange>[AnimEvent] StartAttackLunge blocked - player just landed.</color>");
            return;
        }

        if (playerLoco != null && currentAttackData != null)
        {
            Vector3 attackDirection = transform.forward;

            if (currentTarget != null)
            {
                Vector3 directionToTarget = (currentTarget.position - transform.position).normalized;
                directionToTarget.y = 0;
                attackDirection = directionToTarget;
                transform.rotation = Quaternion.LookRotation(attackDirection);
            }

            AttackSO lungeData = currentAttackData;
            if (rootMotionMode == RootMotionMode.Hybrid)
            {
                AttackSO hybridData = ScriptableObject.CreateInstance<AttackSO>();
                hybridData.moveDistance = currentAttackData.moveDistance * additionalLungeMultiplier;
                hybridData.moveSpeed = currentAttackData.moveSpeed * additionalLungeMultiplier;
                hybridData.moveCurve = currentAttackData.moveCurve;
                hybridData.rotateTowardsTarget = currentAttackData.rotateTowardsTarget;
                lungeData = hybridData;
            }

            playerLoco.StartAttackLungeEvent(attackDirection, lungeData);
        }
        else
        {
            Debug.LogWarning($"<color=orange>[AnimEvent] StartAttackLunge failed - PlayerLoco: {playerLoco != null}, CurrentAttackData: {currentAttackData != null}, IsAttacking: {isAttacking}</color>");
        }
    }

    public void StopAttackLunge()
    {
        var playerLoco = GetComponent<PlayerLocomotion>();
        if (playerLoco != null)
        {
            playerLoco.StopAttackLungeEvent();
        }

        if (!attackQueued)
        {
            currentAttackData = null;
        }
    }

    public void CancelAttack()
    {
        isAttacking = false;
        attackQueued = false;

        // Reset animator speed to normal when canceling attack using the manager
        if (attackAnimManager != null)
        {
            attackAnimManager.ResetAnimationSpeed();
        }

        anim.Play("Idle");
        currentTarget = null;
        currentAttackData = null;

        // Disable root motion when canceling attack (except for LungeOnly mode where it's already off)
        if (rootMotionMode != RootMotionMode.LungeOnly)
        {
            anim.applyRootMotion = false;
        }

        // Stop any attack movement
        var playerLoco = GetComponent<PlayerLocomotion>();
        if (playerLoco != null)
        {
            playerLoco.ForceStopAttackLunge();
        }
    }

    // Public method to reset air attack counter (useful for abilities, special moves, etc.)
    public void ResetAirAttackCounter()
    {
        currentAirAttackCount = 0;
    }

    // Public method to check current air attack status (useful for UI or other systems)
    public bool CanPerformAirAttack()
    {
        var playerLoco = GetComponent<PlayerLocomotion>();
        var playerMgr = GetComponent<PlayerManager>();
        bool isGrounded = playerLoco != null && playerLoco.isGrounded;
        bool isInteracting = playerMgr != null && playerMgr.isInteracting;

        // Can't attack while interacting (landing, dodging, etc.)
        if (isInteracting) return false;

        if (isGrounded) return true; // Always can attack when grounded and not interacting

        return allowAirAttacks && currentAirAttackCount < maxAirAttacks;
    }

    // Debug method to get air attack info
    public string GetAirAttackDebugInfo()
    {
        var playerLoco = GetComponent<PlayerLocomotion>();
        var playerMgr = GetComponent<PlayerManager>();
        bool isGrounded = playerLoco != null && playerLoco.isGrounded;
        bool isInteracting = playerMgr != null && playerMgr.isInteracting;

        return $"Grounded: {isGrounded}, Interacting: {isInteracting}, Air Attacks: {currentAirAttackCount}/{maxAirAttacks}, Allow Air: {allowAirAttacks}";
    }

    void OnDrawGizmos()
    {
        if (!showAimDebug || !isAiming || shootPoint == null) return;

        if (cameraAdapter == null) return;

        Vector3 aimTarget = cameraAdapter.GetAimTarget();
        if (aimTarget == Vector3.zero) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(aimTarget, 0.2f);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(shootPoint.position, 0.1f);

        Vector3 correctedSpawn = GetCorrectedSpawnPosition();
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(correctedSpawn, 0.08f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(shootPoint.position, correctedSpawn);

        Gizmos.color = Color.green;
        Vector3 correctedDirection = GetCorrectedAimDirection();
        float distanceToTarget = Vector3.Distance(correctedSpawn, aimTarget);
        Gizmos.DrawLine(correctedSpawn, correctedSpawn + correctedDirection * distanceToTarget);

        Vector3 cameraPos = GetCameraPosition();
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(cameraPos, Vector3.one * 0.1f);

        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(cameraPos, aimTarget);
    }

    private Vector3 GetCameraCenterAimPoint()
    {
        if (cameraAdapter != null)
        {
            Vector3 aimTarget = cameraAdapter.GetAimTarget();
            if (aimTarget != Vector3.zero)
            {
                if (showAimDebug)
                {
                    Debug.Log($"Using CameraAdapter aim target: {aimTarget}");
                }
                return aimTarget;
            }
        }

        Camera cam = Camera.main;
        if (cam == null)
        {
            if (showAimDebug)
            {
                Debug.LogWarning("Camera.main is null! Using shootPoint forward fallback.");
            }
            return shootPoint.position + shootPoint.forward * 10f;
        }

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, hitscanLayerMask))
        {
            if (showAimDebug)
            {
                Debug.Log($"Using Camera raycast aim target: {hit.point}");
            }
            return hit.point;
        }
        else
        {
            Vector3 fallbackTarget = ray.origin + ray.direction * 100f;
            if (showAimDebug)
            {
                Debug.Log($"Using Camera forward fallback: {fallbackTarget}");
            }
            return fallbackTarget;
        }
    }
}
