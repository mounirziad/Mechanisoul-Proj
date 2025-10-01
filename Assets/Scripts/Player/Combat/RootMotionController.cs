using UnityEngine;

/// <summary>
/// Advanced root motion controller for precise control over when root motion is applied.
/// Attach this script if you need more granular control beyond the basic PlayerCombat implementation.
/// </summary>
public class RootMotionController : MonoBehaviour
{
    [Header("Root Motion Control")]
    [Tooltip("Enable automatic root motion control based on animation states")]
    public bool autoControlRootMotion = true;
    
    [Tooltip("Animation state names that should use root motion")]
    public string[] rootMotionStates = { "Attack", "Combo1", "Combo2", "Combo3" };
    
    private Animator animator;
    private bool wasUsingRootMotion = false;
    
    void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("RootMotionController requires an Animator component!");
            enabled = false;
        }
    }
    
    void Start()
    {
        // Ensure root motion starts disabled
        if (animator != null)
        {
            animator.applyRootMotion = false;
        }
    }
    
    void Update()
    {
        if (!autoControlRootMotion || animator == null) return;
        
        bool shouldUseRootMotion = ShouldUseRootMotion();
        
        if (shouldUseRootMotion != wasUsingRootMotion)
        {
            animator.applyRootMotion = shouldUseRootMotion;
            wasUsingRootMotion = shouldUseRootMotion;
            
            Debug.Log($"Root motion {(shouldUseRootMotion ? "enabled" : "disabled")} for state: {animator.GetCurrentAnimatorStateInfo(0).shortNameHash}");
        }
    }
    
    /// <summary>
    /// Checks if the current animation state should use root motion
    /// </summary>
    bool ShouldUseRootMotion()
    {
        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);
        
        foreach (string stateName in rootMotionStates)
        {
            if (currentState.IsName(stateName))
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Manually enable root motion
    /// </summary>
    public void EnableRootMotion()
    {
        if (animator != null)
        {
            animator.applyRootMotion = true;
            wasUsingRootMotion = true;
        }
    }
    
    /// <summary>
    /// Manually disable root motion
    /// </summary>
    public void DisableRootMotion()
    {
        if (animator != null)
        {
            animator.applyRootMotion = false;
            wasUsingRootMotion = false;
        }
    }
    
    /// <summary>
    /// Toggle root motion on/off
    /// </summary>
    public void ToggleRootMotion()
    {
        if (animator != null)
        {
            bool newState = !animator.applyRootMotion;
            animator.applyRootMotion = newState;
            wasUsingRootMotion = newState;
        }
    }
}