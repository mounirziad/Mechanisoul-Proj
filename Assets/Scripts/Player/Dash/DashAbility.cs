using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem; // NEW Input System

[RequireComponent(typeof(Rigidbody))]
[DisallowMultipleComponent]
public class DashAbility : MonoBehaviour
{
    [Header("Hook-ins")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform directionSource; // optional: where "forward" should read from (e.g., player/camera root)

    [Header("Input System")]
    [Tooltip("Reference to your Dash action (performed triggers the dash). Leave null if you call TryDash() from code.")]
    public InputActionReference dashAction;

    [Header("Dash Tuning")]
    [Tooltip("Distance = bodyLength * this value")]
    public float dashDistanceInBodyLengths = 3.5f;
    [Tooltip("Seconds the dash motion takes")]
    public float dashDuration = 0.28f;
    [Tooltip("Seconds before next dash allowed")]
    public float dashCooldown = 0.25f;
    [Tooltip("Ease curve for position vs time (integrated & normalized to distance)")]
    public AnimationCurve dashEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Tooltip("Fallback body length when no collider height is available.")]
    public float fallbackBodyLength = 2.0f;

    [Header("Effect Prefabs")]
    [Tooltip("Persistent AoE circle that applies fire DoT. Optional.")]
    public GameObject fireDoTPrefab;
    [Tooltip("Trigger/area that slows enemies. Optional.")]
    public GameObject slowZonePrefab;

    [Header("Effect Placement")]
    [Tooltip("Spacing (meters) for laying slow trail nodes while dashing.")]
    public float trailStepMeters = 0.65f;
    [Tooltip("Number of Anger AOE nodes to drop along the path (1 = end only).")]
    public int angerAOENodes = 1;

    // Upgrade-fed values
    float angerAOEPercent;   // e.g., 0.15 = 15% of player's damage per tick
    float sadnessSlowPercent; // 0..1
    float sadnessSlowSeconds;

    // State
    bool isDashing;
    bool dashOnCooldown;
    float curveArea = 1f;

    Vector3 dashStartPos;
    Vector3 dashDir;

    void Reset()
    {
        dashEase = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 2f),
            new Keyframe(0.15f, 0.25f),
            new Keyframe(0.85f, 0.75f),
            new Keyframe(1f, 1f, 2f, 0f)
        );
    }

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody>();
        curveArea = ApproxCurveArea(dashEase, 200);
        if (curveArea < 1e-3f) curveArea = 1f;
        if (!directionSource) directionSource = transform;
    }

    void OnEnable()
    {
        if (dashAction != null)
        {
            dashAction.action.performed += OnDashPerformed;
            if (!dashAction.action.enabled) dashAction.action.Enable();
        }
    }

    void OnDisable()
    {
        if (dashAction != null)
        {
            dashAction.action.performed -= OnDashPerformed;
            if (dashAction.action.enabled) dashAction.action.Disable();
        }
    }

    void OnDashPerformed(InputAction.CallbackContext ctx)
    {
        TryDash();
    }

    /// Upgrades forwarded from UpgradeHandler/PlayerManager.
    public void SetUpgrades(float aoePercent, float slowPercent, float slowSeconds, float _lifeSteal = 0f, float _stunSec = 0f)
    {
        angerAOEPercent = Mathf.Max(0f, aoePercent);
        sadnessSlowPercent = Mathf.Clamp01(slowPercent);
        sadnessSlowSeconds = Mathf.Max(0f, slowSeconds);
    }

    /// Attempt to start a dash using the last known movement/velocity or forward.
    public void TryDash()
    {
        if (isDashing || dashOnCooldown) return;

        Vector3 dir = GetDashDirection();
        if (dir.sqrMagnitude < 0.0001f) dir = directionSource.forward;
        StartCoroutine(DashRoutine(dir.normalized));
    }

    Vector3 GetDashDirection()
    {
        // Prefer current linear velocity if moving
        Vector3 v = rb ? rb.linearVelocity : Vector3.zero;
        if (v.sqrMagnitude > 0.01f) return new Vector3(v.x, 0f, v.z).normalized;

        // Fallback to forward if no movement
        return directionSource ? directionSource.forward : transform.forward;
    }

    IEnumerator DashRoutine(Vector3 direction)
    {
        isDashing = true;
        dashDir = direction;
        dashStartPos = rb.position;

        // Compute body length (prefer capsule/collider height)
        float bodyLen = fallbackBodyLength;
        var capsule = GetComponent<CapsuleCollider>();
        if (capsule) bodyLen = Mathf.Max(0.25f, capsule.height);

        float totalDistance = bodyLen * dashDistanceInBodyLengths;

        float t = 0f;
        float dropAccum = 0f;

        while (t < dashDuration)
        {
            float dt = Mathf.Max(Time.deltaTime, 1e-4f);
            float u0 = Mathf.Clamp01(t / dashDuration);
            float u1 = Mathf.Clamp01((t + dt) / dashDuration);
            float uMid = 0.5f * (u0 + u1);

            float frac = dashEase.Evaluate(uMid) * (u1 - u0);
            float stepLen = (totalDistance / curveArea) * frac;
            Vector3 step = dashDir * stepLen;

            rb.MovePosition(rb.position + step);

            // ---- Sadness: lay slow trail ----
            if (sadnessSlowPercent > 0f && slowZonePrefab)
            {
                dropAccum += step.magnitude;
                if (dropAccum >= trailStepMeters)
                {
                    dropAccum = 0f;
                    SpawnSlowZone(rb.position, sadnessSlowPercent, sadnessSlowSeconds);
                }
            }

            t += dt;
            yield return null;
        }

        // ---- Anger: place AoE(s) ----
        if (angerAOEPercent > 0f && fireDoTPrefab)
        {
            if (angerAOENodes <= 1)
            {
                SpawnFireZone(rb.position, angerAOEPercent);
            }
            else
            {
                for (int i = 1; i <= angerAOENodes; i++)
                {
                    float a = (float)i / (angerAOENodes + 1);
                    Vector3 p = Vector3.Lerp(dashStartPos, rb.position, a);
                    SpawnFireZone(p, angerAOEPercent);
                }
            }
        }

        isDashing = false;
        dashOnCooldown = true;
        yield return new WaitForSeconds(dashCooldown);
        dashOnCooldown = false;
    }

    void SpawnFireZone(Vector3 pos, float aoePct)
    {
        var go = Instantiate(fireDoTPrefab, pos, Quaternion.identity);
        var dot = go.GetComponent<FireDoTZone>();
        if (dot) dot.Configure(aoePct);
    }

    void SpawnSlowZone(Vector3 pos, float slowPct, float seconds)
    {
        var go = Instantiate(slowZonePrefab, pos, Quaternion.identity);
        var slow = go.GetComponent<SlowZone>();
        if (slow) slow.Configure(slowPct, seconds);
    }

    float ApproxCurveArea(AnimationCurve curve, int samples)
    {
        float area = 0f;
        float prevU = 0f, prevV = curve.Evaluate(0f);
        for (int i = 1; i <= samples; i++)
        {
            float u = (float)i / samples;
            float v = curve.Evaluate(u);
            area += 0.5f * (v + prevV) * (u - prevU);
            prevU = u; prevV = v;
        }
        return Mathf.Max(area, 1e-4f);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        float bodyLen = fallbackBodyLength;
        var capsule = GetComponent<CapsuleCollider>();
        if (capsule) bodyLen = Mathf.Max(0.25f, capsule.height);
        float d = bodyLen * dashDistanceInBodyLengths;
        Gizmos.color = Color.cyan;
        Vector3 start = transform.position + Vector3.up * 0.1f;
        Vector3 end = start + (directionSource ? directionSource.forward : transform.forward) * d;
        Gizmos.DrawLine(start, end);
    }
#endif
}
