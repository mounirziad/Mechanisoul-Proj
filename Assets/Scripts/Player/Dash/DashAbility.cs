using UnityEngine;

[DisallowMultipleComponent]
public class DashAbility : MonoBehaviour
{
    [Header("Dash Core")]
    [Min(0.01f)] public float dashDistance = 8f;     // total distance traveled during a dash
    [Min(0.01f)] public float dashDuration = 0.15f;  // total time of the dash

    [Header("VFX/Gameplay Zones")]
    public GameObject fireDoTPrefab;    // Anger: Fire/DoT zone prefab (expects FireDoTZone with Configure(percent))
    public GameObject slowZonePrefab;   // Sadness: Slow zone prefab (expects SlowZone with Configure(percent, seconds))

    [Header("Dash Upgrades (live values)")]
    [Tooltip("Percent of base damage per tick/zone (0.15 = 15%).")]
    [Range(0f, 1f)] public float angerAOEPercent = 0f;
    public bool angerDoTOnDash = false;

    [Range(0f, 1f)] public float sadnessSlowPercent = 0f;
    [Min(0f)] public float sadnessSlowSeconds = 0f;

    // Internal state
    public bool IsDashing { get; private set; }
    Vector3 dashDir;
    float remainingDistance;
    float dashSpeed; // computed from distance/duration

    // Movement backends
    CharacterController cc;
    Rigidbody rb;

    PlayerLocomotion locomotion;
    float cachedWalk, cachedRun;
    bool locomotionScaled;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        locomotion = GetComponent<PlayerLocomotion>();
        if (locomotion != null)
        {
            cachedWalk = locomotion.walkingSpeed;
            cachedRun = locomotion.runningSpeed;
        }
    }

    void Update()
    {
        if (!IsDashing) return;

        dashSpeed = Mathf.Max(0.0001f, dashDistance / Mathf.Max(0.0001f, dashDuration));
        float step = dashSpeed * Time.deltaTime;
        if (step > remainingDistance) step = remainingDistance;

        Vector3 delta = dashDir * step;

        if (cc != null && cc.enabled)
        {
            cc.Move(delta);
        }
        else if (rb != null && rb.isKinematic)
        {
            rb.MovePosition(rb.position + delta);
        }
        else if (rb != null && !rb.isKinematic)
        {
            rb.linearVelocity = dashDir * dashSpeed; // zeroed in EndDash()
        }
        else
        {
            transform.position += delta;
        }

        remainingDistance -= step;
        if (remainingDistance <= 0.0001f)
            EndDash();
    }

    /// Set live dash upgrade values (anger/sadness) in one call
    public void SetDashParams(float angerAOEPercent, float sadnessSlowPercent, float sadnessSlowSeconds)
    {
        this.angerAOEPercent = Mathf.Clamp01(angerAOEPercent);
        this.angerDoTOnDash = angerAOEPercent > 0f;
        this.sadnessSlowPercent = Mathf.Clamp01(sadnessSlowPercent);
        this.sadnessSlowSeconds = Mathf.Max(0f, sadnessSlowSeconds);
        // Debug.Log($"[DashAbility] Params: angerAOE={this.angerAOEPercent:P0}, slow={this.sadnessSlowPercent:P0} for {this.sadnessSlowSeconds:0.##}s");
    }

    /// Starts a dash in the given world direction.
    public void PerformDash(Vector3 worldDirection)
    {
        if (IsDashing) return;

        dashDir = worldDirection.sqrMagnitude > 0.000001f ? worldDirection.normalized : transform.forward;
        remainingDistance = Mathf.Max(0f, dashDistance);
        dashSpeed = Mathf.Max(0.0001f, dashDistance / Mathf.Max(0.0001f, dashDuration));
        IsDashing = true;

        if (locomotion != null && !locomotionScaled)
        {
            locomotion.walkingSpeed *= 0.01f;
            locomotion.runningSpeed *= 0.01f;
            locomotionScaled = true;
        }

        // Spawn zones at dash START (duplicate in EndDash for trail/exit)
        if (angerDoTOnDash && fireDoTPrefab != null)
        {
            var go = Instantiate(fireDoTPrefab, transform.position, Quaternion.identity);
            var dot = go.GetComponent<FireDoTZone>();
            if (dot != null) dot.Configure(Mathf.Max(0f, angerAOEPercent));
        }

        if (sadnessSlowPercent > 0f && sadnessSlowSeconds > 0f && slowZonePrefab != null)
        {
            var slow = Instantiate(slowZonePrefab, transform.position, Quaternion.identity);
            var zone = slow.GetComponent<SlowZone>();
            if (zone != null) zone.Configure(Mathf.Clamp01(sadnessSlowPercent), Mathf.Max(0f, sadnessSlowSeconds));
        }
    }

    void EndDash()
    {
        if (rb != null && !rb.isKinematic) rb.linearVelocity = Vector3.zero;

        if (locomotion != null && locomotionScaled)
        {
            locomotion.walkingSpeed = cachedWalk;
            locomotion.runningSpeed = cachedRun;
            locomotionScaled = false;
        }

        IsDashing = false;
        remainingDistance = 0f;
    }

    public bool TryDash()
    {
        if (IsDashing) return false;
        PerformDash(transform.forward);
        return true;
    }

  
    public bool TryDash(Vector3 worldDirection)
    {
        if (IsDashing) return false;
        PerformDash(worldDirection);
        return true;
    }

    public void SetUpgrades(float angerAOEPercent, float sadnessSlowPercent, float sadnessSlowSeconds, float _lifeStealUnused, float _stunUnused)
    {
        SetDashParams(angerAOEPercent, sadnessSlowPercent, sadnessSlowSeconds);
    }
}
