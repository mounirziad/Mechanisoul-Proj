using UnityEngine;
using UnityEngine.UI;

public class TargetReticleUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ZTargetingSystem targetingSystem;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Canvas uiCanvas;
    
    [Header("Reticle Settings")]
    [SerializeField] private Image reticleImage;
    [SerializeField] private Sprite reticleSprite;
    [SerializeField] private float reticleSize = 64f;
    [SerializeField] private Color reticleColor = Color.white;
    
    [Header("Position Settings")]
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1f, 0f);
    [SerializeField] private float smoothSpeed = 10f;
    
    [Header("Animation")]
    [SerializeField] private bool enablePulse = true;
    [SerializeField] private float pulseSpeed = 2f;
    [SerializeField] private float pulseMinScale = 0.9f;
    [SerializeField] private float pulseMaxScale = 1.1f;
    
    [Header("Visibility")]
    [SerializeField] private float fadeInSpeed = 5f;
    [SerializeField] private float fadeOutSpeed = 8f;
    
    private RectTransform reticleRectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 currentScreenPosition;
    private float currentAlpha = 0f;
    
    private void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
        
        if (targetingSystem == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                targetingSystem = player.GetComponent<ZTargetingSystem>();
        }
        
        SetupReticleUI();
    }
    
    private void SetupReticleUI()
    {
        if (uiCanvas == null)
        {
            GameObject targetingCanvasObj = GameObject.Find("ZTargetingTarget");
            if (targetingCanvasObj != null)
            {
                uiCanvas = targetingCanvasObj.GetComponent<Canvas>();
            }
            
            if (uiCanvas == null)
            {
                Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
                foreach (Canvas canvas in canvases)
                {
                    if (canvas.renderMode == RenderMode.ScreenSpaceOverlay && canvas.name == "ZTargetingTarget")
                    {
                        uiCanvas = canvas;
                        break;
                    }
                }
            }
            
            if (uiCanvas == null)
            {
                GameObject canvasObj = new GameObject("ZTargetingTarget");
                uiCanvas = canvasObj.AddComponent<Canvas>();
                uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                uiCanvas.sortingOrder = 100;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }
        }
        
        if (reticleImage == null)
        {
            GameObject existingReticle = GameObject.Find("TargetReticle");
            if (existingReticle != null && existingReticle.transform.parent == uiCanvas.transform)
            {
                reticleImage = existingReticle.GetComponent<Image>();
                reticleRectTransform = existingReticle.GetComponent<RectTransform>();
                canvasGroup = existingReticle.GetComponent<CanvasGroup>();
            }
            
            if (reticleImage == null)
            {
                GameObject reticleObj = new GameObject("TargetReticle");
                reticleObj.transform.SetParent(uiCanvas.transform, false);
                
                reticleImage = reticleObj.AddComponent<Image>();
                reticleImage.raycastTarget = false;
                
                reticleRectTransform = reticleObj.GetComponent<RectTransform>();
                reticleRectTransform.sizeDelta = new Vector2(reticleSize, reticleSize);
                reticleRectTransform.anchorMin = Vector2.zero;
                reticleRectTransform.anchorMax = Vector2.zero;
                reticleRectTransform.pivot = new Vector2(0.5f, 0.5f);
                
                canvasGroup = reticleObj.AddComponent<CanvasGroup>();
                canvasGroup.alpha = 0f;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
        }
        else
        {
            if (reticleRectTransform == null)
                reticleRectTransform = reticleImage.GetComponent<RectTransform>();
            if (canvasGroup == null)
                canvasGroup = reticleImage.GetComponent<CanvasGroup>();
        }
        
        if (reticleSprite != null && reticleImage != null)
        {
            reticleImage.sprite = reticleSprite;
        }
        
        if (reticleImage != null)
        {
            reticleImage.color = reticleColor;
        }
    }
    
    private void OnEnable()
    {
        if (targetingSystem != null)
        {
            targetingSystem.OnLockStateChanged += OnLockStateChanged;
        }
    }
    
    private void OnDisable()
    {
        if (targetingSystem != null)
        {
            targetingSystem.OnLockStateChanged -= OnLockStateChanged;
        }
    }
    
    private void ValidateReferences()
    {
        if (mainCamera == null)
        {
            GameObject lockOnCameraObj = GameObject.Find("Lock-On-CameraCollision");
            if (lockOnCameraObj != null)
            {
                mainCamera = lockOnCameraObj.GetComponent<Camera>();
            }
            
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }
        }
        
        if (uiCanvas == null || reticleImage == null || reticleRectTransform == null || canvasGroup == null)
        {
            SetupReticleUI();
        }
    }
    
    private void Update()
    {
        ValidateReferences();
        
        if (targetingSystem == null || mainCamera == null || reticleImage == null)
            return;
        
        if (targetingSystem.IsLocked && targetingSystem.CurrentTarget != null)
        {
            UpdateReticlePosition();
            UpdateReticleVisibility(true);
            
            if (enablePulse)
            {
                UpdateReticlePulse();
            }
        }
        else
        {
            UpdateReticleVisibility(false);
        }
    }
    
    private void UpdateReticlePosition()
    {
        Vector3 worldPosition = targetingSystem.CurrentTarget.position + targetOffset;
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
        
        if (screenPosition.z > 0)
        {
            currentScreenPosition = Vector2.Lerp(
                currentScreenPosition,
                new Vector2(screenPosition.x, screenPosition.y),
                smoothSpeed * Time.deltaTime
            );
            
            reticleRectTransform.position = currentScreenPosition;
        }
    }
    
    private void UpdateReticleVisibility(bool visible)
    {
        float targetAlpha = visible ? 1f : 0f;
        float fadeSpeed = visible ? fadeInSpeed : fadeOutSpeed;
        
        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = currentAlpha;
        }
        
        if (!visible && currentAlpha < 0.01f)
        {
            reticleImage.enabled = false;
        }
        else if (visible)
        {
            reticleImage.enabled = true;
        }
    }
    
    private void UpdateReticlePulse()
    {
        float scale = Mathf.Lerp(
            pulseMinScale,
            pulseMaxScale,
            (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f
        );
        
        reticleRectTransform.localScale = Vector3.one * scale;
    }
    
    private void OnLockStateChanged(bool isLocked)
    {
        if (!isLocked)
        {
            currentScreenPosition = reticleRectTransform.position;
        }
    }
    
    public void SetReticleSprite(Sprite sprite)
    {
        reticleSprite = sprite;
        if (reticleImage != null)
        {
            reticleImage.sprite = sprite;
        }
    }
    
    public void SetReticleColor(Color color)
    {
        reticleColor = color;
        if (reticleImage != null)
        {
            reticleImage.color = color;
        }
    }
    
    public void SetReticleSize(float size)
    {
        reticleSize = size;
        if (reticleRectTransform != null)
        {
            reticleRectTransform.sizeDelta = new Vector2(size, size);
        }
    }
}
