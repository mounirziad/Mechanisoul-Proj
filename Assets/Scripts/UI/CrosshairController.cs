using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private GameObject crosshairObject;
    [SerializeField] private CameraManagerAdapter cameraAdapter;
    
    [Header("Settings")]
    [SerializeField] private bool fadeInOut = true;
    [SerializeField] private float fadeSpeed = 5f;
    
    private CanvasGroup canvasGroup;
    private bool isVisible = false;
    private Vector3 lastAimTarget = Vector3.zero;
    
    void Start()
    {
        FindReferencesIfNeeded();
        
        if (crosshairObject != null)
        {
            canvasGroup = crosshairObject.GetComponent<CanvasGroup>();
            if (canvasGroup == null && fadeInOut)
            {
                canvasGroup = crosshairObject.AddComponent<CanvasGroup>();
            }
            
            SetCrosshairVisibility(false, true);
        }
    }
    
    void Update()
    {
        if (playerCombat == null)
        {
            FindReferencesIfNeeded();
            return;
        }
        
        bool shouldBeVisible = playerCombat.isAiming;
        
        if (shouldBeVisible != isVisible)
        {
            SetCrosshairVisibility(shouldBeVisible);
        }
        
        if (shouldBeVisible && cameraAdapter != null)
        {
            Vector3 currentAimTarget = cameraAdapter.GetAimTarget();
            if (currentAimTarget != Vector3.zero)
            {
                lastAimTarget = currentAimTarget;
            }
        }
        
        if (fadeInOut && canvasGroup != null)
        {
            float targetAlpha = isVisible ? 1f : 0f;
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
        }
    }
    
    private void FindReferencesIfNeeded()
    {
        if (playerCombat == null)
        {
            playerCombat = FindAnyObjectByType<PlayerCombat>();
        }
        
        if (cameraAdapter == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                cameraAdapter = player.GetComponent<CameraManagerAdapter>();
                if (cameraAdapter == null)
                {
                    cameraAdapter = player.AddComponent<CameraManagerAdapter>();
                }
            }
        }
        
        if (crosshairObject == null && transform.childCount > 0)
        {
            crosshairObject = transform.GetChild(0).gameObject;
        }
    }
    
    private void SetCrosshairVisibility(bool visible, bool immediate = false)
    {
        if (crosshairObject == null) return;
        
        isVisible = visible;
        
        if (immediate)
        {
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
            crosshairObject.SetActive(visible);
        }
    }
    
    public void ForceSetVisibility(bool visible)
    {
        SetCrosshairVisibility(visible, true);
    }
    
    public bool IsVisible()
    {
        return isVisible;
    }
    
    public Vector3 GetLastAimTarget()
    {
        return lastAimTarget;
    }
}
