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

    [Header("Input System")]
    public InputActionReference dashAction;

    [Header("Dash Tuning")]
    public float dashDistanceInBodyLengths = 3.5f;
    public float dashDuration = 0.28f;
    public float dashCooldown = 0.25f;
    public AnimationCurve dashEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float fallbackBodyLength = 2.0f;

    bool isDashing;
    bool dashOnCooldown;
    float curveArea = 1f;

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

    void OnDashPerformed(InputAction.CallbackContext _) => TryDash();

    public void TryDash()
    {
        if (isDashing || dashOnCooldown) return;
        Vector3 dir = GetDashDirection();
        if (dir.sqrMagnitude < 0.0001f) dir = directionSource.forward;
        StartCoroutine(DashRoutine(dir.normalized));
    }

    Vector3 GetDashDirection()
    {
        Vector3 v = rb ? rb.linearVelocity : Vector3.zero;
        if (v.sqrMagnitude > 0.01f) return new Vector3(v.x, 0f, v.z).normalized;
        return directionSource ? directionSource.forward : transform.forward;
    }

    // smoothly dash in the given direction over the dash duration
    IEnumerator DashRoutine(Vector3 direction)
    {
        isDashing = true;
        Vector3 startPos = rb.position;

        float bodyLen = fallbackBodyLength;
        var capsule = GetComponent<CapsuleCollider>();
        if (capsule) bodyLen = Mathf.Max(0.25f, capsule.height);

        float totalDistance = bodyLen * dashDistanceInBodyLengths;

        float t = 0f;
        while (t < dashDuration)
        {
            float dt = Mathf.Max(Time.deltaTime, 1e-4f);
            float u0 = Mathf.Clamp01(t / dashDuration);
            float u1 = Mathf.Clamp01((t + dt) / dashDuration);
            float uMid = 0.5f * (u0 + u1);

            float frac = dashEase.Evaluate(uMid) * (u1 - u0);
            float stepLen = (totalDistance / curveArea) * frac;
            Vector3 step = direction * stepLen;

            rb.MovePosition(rb.position + step);
            OnDashStep?.Invoke(rb.position);

            t += dt;
            yield return null;
        }

        Vector3 endPos = rb.position;
        OnDashFinished?.Invoke(startPos, endPos);

        isDashing = false;
        dashOnCooldown = true;
        yield return new WaitForSeconds(dashCooldown);
        dashOnCooldown = false;
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
