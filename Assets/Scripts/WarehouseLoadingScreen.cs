using System.Collections;
using UnityEngine;

public class WarehouseLoadingScreen : MonoBehaviour
{
    public static WarehouseLoadingScreen Instance { get; private set; }
    
    [Header("Loading Settings")]
    [SerializeField] private float displayDuration = 5f;
    [SerializeField] private float fadeDuration = 1f;
    
    [Header("References")]
    [SerializeField] private CanvasGroup canvasGroup;
    
    private const float INITIAL_ALPHA = 1f;
    private const float FINAL_ALPHA = 0f;
    
    private bool isDisplaying = false;
    public bool IsDisplaying => isDisplaying;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
    }
    
    private void Start()
    {
        StartCoroutine(LoadingSequence());
    }
    
    private IEnumerator LoadingSequence()
    {
        isDisplaying = true;
        
        canvasGroup.alpha = INITIAL_ALPHA;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        
        yield return new WaitForSeconds(displayDuration);
        
        isDisplaying = false;
        
        yield return StartCoroutine(FadeOut());
        
        gameObject.SetActive(false);
    }
    
    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(INITIAL_ALPHA, FINAL_ALPHA, normalizedTime);
            yield return null;
        }
        
        canvasGroup.alpha = FINAL_ALPHA;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
    
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
