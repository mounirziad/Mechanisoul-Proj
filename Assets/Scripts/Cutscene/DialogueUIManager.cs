using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueUIManager : MonoBehaviour
{
    [Header("Legacy UI Setup - Use DialogueSystemSetup instead")]
    [SerializeField] private Canvas dialogueCanvas;
    [SerializeField] private GameObject dialogueUIParent;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI shadowText;
    [SerializeField] private CanvasGroup canvasGroup;
    
    [Header("Positioning")]
    [SerializeField] private float bottomOffset = 100f;
    [SerializeField] private float sideMargin = 50f;
    
    [Header("Shadow Settings")]
    [SerializeField] private Vector2 shadowOffset = new Vector2(3f, -3f);
    [SerializeField] private Color shadowColor = new Color(0f, 0f, 0f, 0.9f);
    
    private void Start()
    {
       
    }
    
    private void CreateDialogueUI()
    {
        if (dialogueCanvas == null)
        {
            GameObject canvasGO = new GameObject("DialogueCanvas");
            dialogueCanvas = canvasGO.AddComponent<Canvas>();
            dialogueCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            dialogueCanvas.sortingOrder = 100;
            
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            
            canvasGO.AddComponent<GraphicRaycaster>();
        }
        
        if (dialogueUIParent == null)
        {
            dialogueUIParent = new GameObject("DialogueUI");
            dialogueUIParent.transform.SetParent(dialogueCanvas.transform, false);
            
            RectTransform parentRect = dialogueUIParent.AddComponent<RectTransform>();
            parentRect.anchorMin = new Vector2(0f, 0f);
            parentRect.anchorMax = new Vector2(1f, 0f);
            parentRect.anchoredPosition = new Vector2(0f, bottomOffset);
            parentRect.sizeDelta = new Vector2(-sideMargin * 2, 200f);
            
            canvasGroup = dialogueUIParent.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
        }
        
        CreateShadowText();
        CreateMainText();
        
        SetupDialogueSystem();
    }
    
    private void CreateShadowText()
    {
        GameObject shadowGO = new GameObject("ShadowText");
        shadowGO.transform.SetParent(dialogueUIParent.transform, false);
        
        shadowText = shadowGO.AddComponent<TextMeshProUGUI>();
        shadowText.text = "";
        shadowText.fontSize = 24;
        shadowText.color = shadowColor;
        shadowText.alignment = TextAlignmentOptions.BottomLeft;
        shadowText.fontStyle = FontStyles.Bold;
        
        RectTransform shadowRect = shadowText.rectTransform;
        shadowRect.anchorMin = Vector2.zero;
        shadowRect.anchorMax = Vector2.one;
        shadowRect.offsetMin = Vector2.zero;
        shadowRect.offsetMax = Vector2.zero;
        shadowRect.anchoredPosition = shadowOffset;
    }
    
    private void CreateMainText()
    {
        GameObject textGO = new GameObject("DialogueText");
        textGO.transform.SetParent(dialogueUIParent.transform, false);
        
        dialogueText = textGO.AddComponent<TextMeshProUGUI>();
        dialogueText.text = "";
        dialogueText.fontSize = 24;
        dialogueText.color = Color.white;
        dialogueText.alignment = TextAlignmentOptions.BottomLeft;
        dialogueText.fontStyle = FontStyles.Bold;
        
        RectTransform textRect = dialogueText.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
    }
    
    private void SetupDialogueSystem()
    {
        DialogueSystem dialogueSystem = FindFirstObjectByType<DialogueSystem>();
        if (dialogueSystem == null)
        {
            GameObject dialogueSystemGO = new GameObject("DialogueSystem");
            dialogueSystem = dialogueSystemGO.AddComponent<DialogueSystem>();
            dialogueSystemGO.AddComponent<AudioSource>();
        }
        
        var dialogueSystemType = typeof(DialogueSystem);
        var dialogueTextField = dialogueSystemType.GetField("dialogueText", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var shadowTextField = dialogueSystemType.GetField("shadowText", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var canvasGroupField = dialogueSystemType.GetField("dialogueCanvasGroup", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        dialogueTextField?.SetValue(dialogueSystem, dialogueText);
        shadowTextField?.SetValue(dialogueSystem, shadowText);
        canvasGroupField?.SetValue(dialogueSystem, canvasGroup);
    }
    
    public void TestDialogue()
    {
        if (DialogueSystem.Instance != null)
        {
            DialogueSystem.Instance.ShowDialogue("This is a test dialogue line that appears at the bottom of the screen with a shadow, just like in BioShock Infinite.", 4f);
        }
    }
}