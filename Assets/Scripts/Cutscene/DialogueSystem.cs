using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;

[System.Serializable]
public class DialogueLine
{
    [TextArea(2, 4)]
    public string text;
    public float displayDuration = 3f;
    public string speakerName;
    public AudioClip voiceClip;
    public bool autoAdvance = true;
    public bool waitForInput = false;
}

[System.Serializable]
public class DialogueSequence
{
    public DialogueLine[] lines;
    public bool loopSequence = false;
}

public class DialogueSystem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI shadowText;
    [SerializeField] private CanvasGroup dialogueCanvasGroup;
    [SerializeField] private AudioSource audioSource;
    
    [Header("Animation Settings")]
    [SerializeField] private float typewriterSpeed = 0.05f;
    
    [Header("Visual Settings")]
    [SerializeField] private Vector2 shadowOffset = new Vector2(2f, -2f);
    [SerializeField] private Color shadowColor = new Color(0f, 0f, 0f, 0.8f);
    
    [Header("Text Chunking Settings")]
    [SerializeField] private int maxCharactersPerChunk = 120;
    [SerializeField] private bool autoChunkLongText = true;
    [SerializeField] private float chunkPauseDuration = 0.5f;
    
    [Header("Input Settings")]
    [SerializeField] private bool useJumpToAdvance = true;
    
    private PlayerControls playerControls;
    private bool jumpPressedThisFrame = false;
    
    private Coroutine currentDialogueCoroutine;
    private bool isDisplaying = false;
    private bool waitingForInput = false;
    private bool canAdvance = false;
    private List<string> currentTextChunks = new List<string>();
    private int currentChunkIndex = 0;
    private DialogueSequence currentSequence;
    private int currentSequenceIndex = 0;
    
    public static DialogueSystem Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        SetupShadowText();
        dialogueCanvasGroup.alpha = 0f;
        
        // Set up direct PlayerControls input
        if (useJumpToAdvance)
        {
            SetupPlayerInput();
        }
    }
    
    private void SetupPlayerInput()
    {
        playerControls = new PlayerControls();
        
        // Subscribe to jump input events
        playerControls.PlayerActions.Jump.performed += OnJumpPressed;
        
        playerControls.Enable();
    }
    
    private void SetupShadowText()
    {
        if (shadowText != null && dialogueText != null)
        {
            shadowText.color = shadowColor;
            shadowText.rectTransform.anchoredPosition = dialogueText.rectTransform.anchoredPosition + shadowOffset;
        }
    }
    
    private void OnDestroy()
    {
        if (playerControls != null)
        {
            playerControls.PlayerActions.Jump.performed -= OnJumpPressed;
            playerControls.Disable();
            playerControls.Dispose();
        }
    }
    
    private void OnJumpPressed(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (waitingForInput)
        {
            jumpPressedThisFrame = true;
        }
    }
    
    private void Update()
    {
        if (waitingForInput && jumpPressedThisFrame)
        {
            jumpPressedThisFrame = false;
            AdvanceDialogue();
        }
    }
    
    public void ShowDialogue(DialogueLine dialogueLine)
    {
        if (currentDialogueCoroutine != null)
        {
            StopCoroutine(currentDialogueCoroutine);
        }
        
        currentDialogueCoroutine = StartCoroutine(DisplaySingleDialogueCoroutine(dialogueLine));
    }
    
    public void ShowDialogue(string text, float duration = 3f, AudioClip voiceClip = null)
    {
        DialogueLine line = new DialogueLine
        {
            text = text,
            displayDuration = duration,
            voiceClip = voiceClip,
            autoAdvance = true
        };
        ShowDialogue(line);
    }
    
    public void ShowDialogueSequence(DialogueSequence sequence)
    {
        if (currentDialogueCoroutine != null)
        {
            StopCoroutine(currentDialogueCoroutine);
        }
        
        currentSequence = sequence;
        currentSequenceIndex = 0;
        currentDialogueCoroutine = StartCoroutine(DisplaySequenceCoroutine());
    }
    
    public void ShowDialogueSequence(DialogueLine[] lines, bool loop = false)
    {
        DialogueSequence sequence = new DialogueSequence
        {
            lines = lines,
            loopSequence = loop
        };
        ShowDialogueSequence(sequence);
    }
    
    private void AdvanceDialogue()
    {
        if (!canAdvance) return;
        
        waitingForInput = false;
        canAdvance = false;
    }
    
    public void HideDialogue()
    {
        if (currentDialogueCoroutine != null)
        {
            StopCoroutine(currentDialogueCoroutine);
        }
        
        // Instant hide instead of fade
        dialogueCanvasGroup.alpha = 0f;
    }
    
    private IEnumerator DisplaySingleDialogueCoroutine(DialogueLine dialogueLine)
    {
        isDisplaying = true;
        
        if (autoChunkLongText)
        {
            currentTextChunks = ChunkText(dialogueLine.text);
        }
        else
        {
            currentTextChunks = new List<string> { dialogueLine.text };
        }
        
        // Instant show instead of fade
        dialogueCanvasGroup.alpha = 1f;
        
        if (dialogueLine.voiceClip != null && audioSource != null)
        {
            audioSource.clip = dialogueLine.voiceClip;
            audioSource.Play();
        }
        
        for (currentChunkIndex = 0; currentChunkIndex < currentTextChunks.Count; currentChunkIndex++)
        {
            yield return StartCoroutine(TypewriterEffect(currentTextChunks[currentChunkIndex]));
            
            if (currentChunkIndex < currentTextChunks.Count - 1)
            {
                if (dialogueLine.waitForInput)
                {
                    waitingForInput = true;
                    canAdvance = true;
                    yield return new WaitUntil(() => !waitingForInput);
                }
                else
                {
                    yield return new WaitForSeconds(chunkPauseDuration);
                }
            }
        }
        
        if (dialogueLine.waitForInput)
        {
            waitingForInput = true;
            canAdvance = true;
            yield return new WaitUntil(() => !waitingForInput);
        }
        else
        {
            yield return new WaitForSeconds(dialogueLine.displayDuration);
        }
        
        // Instant hide instead of fade
        dialogueCanvasGroup.alpha = 0f;
        
        isDisplaying = false;
    }
    
    private IEnumerator DisplaySequenceCoroutine()
    {
        do
        {
            for (currentSequenceIndex = 0; currentSequenceIndex < currentSequence.lines.Length; currentSequenceIndex++)
            {
                yield return StartCoroutine(DisplaySingleDialogueCoroutine(currentSequence.lines[currentSequenceIndex]));
                
                if (currentSequenceIndex < currentSequence.lines.Length - 1)
                {
                    yield return new WaitForSeconds(0.2f);
                }
            }
        }
        while (currentSequence.loopSequence);
    }
    
    private List<string> ChunkText(string text)
    {
        List<string> chunks = new List<string>();
        
        if (text.Length <= maxCharactersPerChunk)
        {
            chunks.Add(text);
            return chunks;
        }
        
        string[] sentences = text.Split('.', '!', '?');
        string currentChunk = "";
        
        foreach (string sentence in sentences)
        {
            string trimmedSentence = sentence.Trim();
            if (string.IsNullOrEmpty(trimmedSentence)) continue;
            
            string potentialChunk = currentChunk + (currentChunk.Length > 0 ? ". " : "") + trimmedSentence + ".";
            
            if (potentialChunk.Length <= maxCharactersPerChunk)
            {
                currentChunk = potentialChunk;
            }
            else
            {
                if (currentChunk.Length > 0)
                {
                    chunks.Add(currentChunk);
                    currentChunk = trimmedSentence + ".";
                }
                else
                {
                    if (trimmedSentence.Length > maxCharactersPerChunk)
                    {
                        string[] words = trimmedSentence.Split(' ');
                        string wordChunk = "";
                        
                        foreach (string word in words)
                        {
                            if ((wordChunk + " " + word).Length <= maxCharactersPerChunk - 1)
                            {
                                wordChunk += (wordChunk.Length > 0 ? " " : "") + word;
                            }
                            else
                            {
                                if (wordChunk.Length > 0)
                                {
                                    chunks.Add(wordChunk + "...");
                                    wordChunk = word;
                                }
                            }
                        }
                        
                        if (wordChunk.Length > 0)
                        {
                            currentChunk = wordChunk + ".";
                        }
                    }
                    else
                    {
                        currentChunk = trimmedSentence + ".";
                    }
                }
            }
        }
        
        if (currentChunk.Length > 0)
        {
            chunks.Add(currentChunk);
        }
        
        return chunks;
    }
    
    private IEnumerator DisplayDialogueCoroutine(DialogueLine dialogueLine)
    {
        yield return StartCoroutine(DisplaySingleDialogueCoroutine(dialogueLine));
    }
    
    private IEnumerator TypewriterEffect(string text)
    {
        dialogueText.text = "";
        shadowText.text = "";
        
        for (int i = 0; i <= text.Length; i++)
        {
            string currentText = text.Substring(0, i);
            dialogueText.text = currentText;
            shadowText.text = currentText;
            yield return new WaitForSeconds(typewriterSpeed);
        }
    }
    
    public bool IsDisplaying => isDisplaying;
    public bool IsWaitingForInput => waitingForInput;
}