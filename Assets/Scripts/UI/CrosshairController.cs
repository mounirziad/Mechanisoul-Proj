using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private GameObject crosshairObject;
    [SerializeField] private ThirdPersonAimCameraManager aimCameraManager;
    
    [Header("Settings")]
    [SerializeField] private bool fadeInOut = true;
    [SerializeField] private float fadeSpeed = 5f;
    
    private CanvasGroup canvasGroup;
    private bool isVisible = false;
    private Vector3 lastAimTarget = Vector3.zero;
    
    void Start()
    {
        // Auto-find PlayerCombat if not assigned
        if (playerCombat == null)
        {
            playerCombat = FindObjectOfType<PlayerCombat>();
        }
        
        // Auto-find ThirdPersonAimCameraManager if not assigned
        if (aimCameraManager == null)
        {
            aimCameraManager = FindObjectOfType<ThirdPersonAimCameraManager>();
        }
        
        // Auto-find crosshair object if not assigned (look for child)
        if (crosshairObject == null)
        {
            crosshairObject = transform.GetChild(0).gameObject;
        }
        
        // Get or add CanvasGroup for smooth fading
        canvasGroup = crosshairObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null && fadeInOut)
        {
            canvasGroup = crosshairObject.AddComponent<CanvasGroup>();
        }
        
        // Start with crosshair hidden
        SetCrosshairVisibility(false, true);
    }
    
    void Update()
    {
        if (playerCombat == null) return;
        
        // Check if player is aiming
        bool shouldBeVisible = playerCombat.isAiming;
        
        // Update visibility if state changed
        if (shouldBeVisible != isVisible)
        {
            SetCrosshairVisibility(shouldBeVisible);
        }
        
        // Store current aim target when aiming for accuracy
        if (shouldBeVisible && aimCameraManager != null)
        {
            Vector3 currentAimTarget = aimCameraManager.GetAimTarget();
            if (currentAimTarget != Vector3.zero)
            {
                lastAimTarget = currentAimTarget;
            }
        }
        
        // Handle smooth fade animation
        if (fadeInOut && canvasGroup != null)
        {
            float targetAlpha = isVisible ? 1f : 0f;
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
        }
    }
    
    private void SetCrosshairVisibility(bool visible, bool immediate = false)
    {
        isVisible = visible;
        
        if (immediate)
        {
            // Immediately set visibility
            if (fadeInOut && canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
            }
            else
            {
                crosshairObject.SetActive(visible);
            }
        }
        else if (!fadeInOut)
        {
            // Simple on/off without fading
            crosshairObject.SetActive(visible);
        }
        // For fade mode, the Update method handles the smooth transition
    }
    
    // Public method to force set visibility (useful for other systems)
    public void ForceSetVisibility(bool visible)
    {
        SetCrosshairVisibility(visible, true);
    }
    
    // Public method to check current visibility state
    public bool IsVisible()
    {
        return isVisible;
    }
    
    // Get the last stored aim target position (useful for projectile accuracy)
    public Vector3 GetLastAimTarget()
    {
        return lastAimTarget;
    }
}