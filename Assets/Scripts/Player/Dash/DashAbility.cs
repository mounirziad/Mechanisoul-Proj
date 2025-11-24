using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[DisallowMultipleComponent]
public class DashAbility : MonoBehaviour
{
    public event Action<Vector3> OnDashStep;                 // fires each frame with current position
    public event Action<Vector3, Vector3> OnDashFinished;    // (startPos, endPos)

    [Header("Hook-ins")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform directionSource; // optional, for debugging later if needed
    [SerializeField] private PlayerLocomotion playerLocomotion;

    [Header("Input System")]
    public InputActionReference dashAction;
    
    [Header("Grounded Requirement")]
    public bool requireGrounded = true;

    [Header("Dash Tuning")]
    public float dashDistanceInBodyLengths = 3.5f;
    public float dashDuration = 0.28f;
    public float dashCooldown = 0.25f;
    public AnimationCurve dashEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float fallbackBodyLength = 2.0f;

    [Header("Collision Detection")]
    public LayerMask obstacleLayerMask = -1;
    public float collisionCheckRadius = 0.5f;

    [Header("Debug")]
    public bool debugLogs = true;

    bool isDashing;
    bool dashOnCooldown;
    float curveArea = 1f;

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody>();
        if (!playerLocomotion) playerLocomotion = GetComponent<PlayerLocomotion>();
        curveArea = ApproxCurveArea(dashEase, 200);
        if (curveArea < 1e-3f) curveArea = 1f;
        if (!directionSource) directionSource = transform;
        if (debugLogs) Debug.Log("[DashAbility] Awake. rb=" + rb + " dirSrc=" + directionSource + " curveArea=" + curveArea, this);
    }

    void OnEnable()
    {
        if (dashAction != null)
        {
            dashAction.action.performed += OnDashPerformed;
            if (!dashAction.action.enabled) dashAction.action.Enable();
        }
        if (debugLogs) Debug.Log("[DashAbility] OnEnable. Subscribed input; OnDashFinished subscribers=" + (OnDashFinished?.GetInvocationList()?.Length ?? 0), this);
    }

    void OnDisable()
    {
        if (dashAction != null)
        {
            dashAction.action.performed -= OnDashPerformed;
            if (dashAction.action.enabled) dashAction.action.Disable();
        }
        if (debugLogs) Debug.Log("[DashAbility] OnDisable. Unsubscribed input.", this);
    }

    void OnDashPerformed(InputAction.CallbackContext _) => TryDash();

    public void TryDash()
    {
        if (isDashing || dashOnCooldown)
        {
            if (debugLogs) Debug.Log($"[DashAbility] TryDash ignored. isDashing={isDashing} cooldown={dashOnCooldown}", this);
            return;
        }
        
        if (DialogueSystem.Instance != null && DialogueSystem.Instance.IsDisplaying)
        {
            if (debugLogs) Debug.Log("[DashAbility] TryDash ignored. Dialogue is active.", this);
            return;
        }
        
        if (requireGrounded && playerLocomotion != null && !playerLocomotion.isGrounded)
        {
            if (debugLogs) Debug.Log("[DashAbility] TryDash ignored. Player is not grounded.", this);
            return;
        }
        
        Vector3 dir = GetDashDirection();
        if (dir.sqrMagnitude < 0.0001f) dir = directionSource.forward;
        if (debugLogs) Debug.Log($"[DashAbility] TryDash start. dir={dir}", this);
        StartCoroutine(DashRoutine(dir.normalized));
    }

    Vector3 GetDashDirection()
    {
        Vector3 v = rb ? rb.linearVelocity : Vector3.zero;
        if (v.sqrMagnitude > 0.01f) return new Vector3(v.x, 0f, v.z).normalized;
        return directionSource ? directionSource.forward : transform.forward;
    }

    bool CheckForObstacle(Vector3 currentPos, Vector3 step, float radius)
    {
        float distance = step.magnitude;
        if (distance < 0.001f) return false;

        Vector3 direction = step.normalized;
        
        if (Physics.SphereCast(currentPos, radius, direction, out _, distance, obstacleLayerMask, QueryTriggerInteraction.Ignore))
        {
            return true;
        }
        
        if (Physics.Raycast(currentPos, direction, distance, obstacleLayerMask, QueryTriggerInteraction.Ignore))
        {
            return true;
        }
        
        return false;
    }

    IEnumerator DashRoutine(Vector3 direction)
    {
        isDashing = true;
        Vector3 startPos = rb.position;

        float bodyLen = fallbackBodyLength;
        var capsule = GetComponent<CapsuleCollider>();
        if (capsule) bodyLen = Mathf.Max(0.25f, capsule.height);

        float totalDistance = bodyLen * dashDistanceInBodyLengths;
        float checkRadius = collisionCheckRadius > 0 ? collisionCheckRadius : (capsule ? capsule.radius : 0.5f);

        float t = 0f;
        if (debugLogs) Debug.Log($"[DashAbility] DashRoutine begin. totalDistance={totalDistance} duration={dashDuration}", this);

        while (t < dashDuration)
        {
            float dt = Mathf.Max(Time.deltaTime, 1e-4f);
            float u0 = Mathf.Clamp01(t / dashDuration);
            float u1 = Mathf.Clamp01((t + dt) / dashDuration);
            float uMid = 0.5f * (u0 + u1);

            float frac = dashEase.Evaluate(uMid) * (u1 - u0);
            float stepLen = (totalDistance / curveArea) * frac;
            Vector3 step = direction * stepLen;

            if (CheckForObstacle(rb.position, step, checkRadius))
            {
                if (debugLogs) Debug.Log($"[DashAbility] Obstacle detected! Stopping dash early at {rb.position}", this);
                break;
            }

            rb.MovePosition(rb.position + step);
            SafeInvokeDashStep(rb.position);

            t += dt;
            yield return null;
        }

        Vector3 endPos = rb.position;
        if (debugLogs) Debug.Log($"[DashAbility] DashRoutine end. start={startPos} end={endPos}", this);

        SafeInvokeDashFinished(startPos, endPos);

        isDashing = false;
        dashOnCooldown = true;
        yield return new WaitForSeconds(dashCooldown);
        dashOnCooldown = false;
        if (debugLogs) Debug.Log("[DashAbility] Cooldown complete.", this);
    }

    // --- Safe fan-out with per-subscriber try/catch so one throw won't block others

    void SafeInvokeDashStep(Vector3 pos)
    {
        var handlers = OnDashStep?.GetInvocationList();
        if (handlers == null) return;

        for (int i = 0; i < handlers.Length; i++)
        {
            try
            {
                ((Action<Vector3>)handlers[i]).Invoke(pos);
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
            }
        }
    }

    void SafeInvokeDashFinished(Vector3 startPos, Vector3 endPos)
    {
        var handlers = OnDashFinished?.GetInvocationList();
        if (debugLogs) Debug.Log("[DashAbility] OnDashFinished fan-out to " + (handlers?.Length ?? 0) + " subscribers.", this);
        if (handlers == null) return;

        for (int i = 0; i < handlers.Length; i++)
        {
            var h = handlers[i];
            try
            {
                if (debugLogs) Debug.Log($"[DashAbility] -> invoking {h.Target?.GetType().Name}.{h.Method.Name}", this);
                ((Action<Vector3, Vector3>)h).Invoke(startPos, endPos);
            }
            catch (Exception e)
            {
                Debug.LogException(e, this);
            }
        }

        if (debugLogs) Debug.Log("[DashAbility] OnDashFinished dispatch complete.", this);
    }

    // curve area approximation for movement scaling (broken?)
    float ApproxCurveArea(AnimationCurve curve, int samples)
    {
        float area = 0f; float prevU = 0f, prevV = curve.Evaluate(0f);
        for (int i = 1; i <= samples; i++)
        {
            float u = (float)i / samples;
            float v = curve.Evaluate(u);
            area += 0.5f * (v + prevV) * (u - prevU);
            prevU = u; prevV = v;
        }
        return Mathf.Max(area, 1e-4f);
    }
}
