using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
    public bool useItalics = false;
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
    [SerializeField] private GameObject continueIndicator;

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
    private string inputDevice = "Keyboard";

    private PlayerControls playerControls;
    private bool advancePressedThisFrame = false; // RENAMED: from jumpPressedThisFrame

    private Coroutine currentDialogueCoroutine;
    private bool isDisplaying = false;
    private bool isSequenceActive = false;
    private bool waitingForInput = false;
    private bool canAdvance = false;
    private bool isTextRunning = false;
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
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            if (Instance != this)
            {
                TransferUIReferencesFromDuplicate(this);
            }
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindAndLinkUIReferences();
    }

    private void TransferUIReferencesFromDuplicate(DialogueSystem duplicate)
    {
        if (duplicate.dialogueText != null) dialogueText = duplicate.dialogueText;
        if (duplicate.shadowText != null) shadowText = duplicate.shadowText;
        if (duplicate.dialogueCanvasGroup != null) dialogueCanvasGroup = duplicate.dialogueCanvasGroup;
        if (duplicate.audioSource != null) audioSource = duplicate.audioSource;

        SetupShadowText();

        if (dialogueCanvasGroup != null)
        {
            dialogueCanvasGroup.alpha = 0f;
        }
    }

    private void FindAndLinkUIReferences()
    {
        DialogueSystem[] allDialogueSystems = FindObjectsOfType<DialogueSystem>(true);

        foreach (DialogueSystem ds in allDialogueSystems)
        {
            if (ds != this && ds.dialogueCanvasGroup != null)
            {
                dialogueText = ds.dialogueText;
                shadowText = ds.shadowText;
                dialogueCanvasGroup = ds.dialogueCanvasGroup;
                audioSource = ds.audioSource;

                SetupShadowText();

                if (dialogueCanvasGroup != null)
                {
                    dialogueCanvasGroup.alpha = 0f;
                }

                if (ds.gameObject != this.gameObject)
                {
                    Destroy(ds.gameObject);
                }

                break;
            }
        }
    }

    private string GetCurrentInputDevice()
    {
        if (Gamepad.current != null && Gamepad.current.wasUpdatedThisFrame)
        {
            return "Gamepad";
        }
        else if (Keyboard.current != null && Mouse.current != null)
        {
            return "Keyboard";
        }

        return inputDevice;
    }

    private void Start()
    {
        SetupShadowText();

        if (dialogueCanvasGroup != null)
        {
            dialogueCanvasGroup.alpha = 0f;
        }

        // Set up direct PlayerControls input
        if (useJumpToAdvance)
        {
            SetupPlayerInput();
        }
    }

    private void SetupPlayerInput()
    {
        playerControls = new PlayerControls();

        // Subscribe to ADVANCE DIALOGUE input events (not jump)
        playerControls.PlayerActions.AdvanceDialogue.performed += OnAdvancePressed;

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
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        if (playerControls != null)
        {
            playerControls.PlayerActions.AdvanceDialogue.performed -= OnAdvancePressed;
            playerControls.Disable();
            playerControls.Dispose();
        }
    }

    private void OnAdvancePressed(UnityEngine.InputSystem.InputAction.CallbackContext context) // RENAMED
    {
        advancePressedThisFrame = true;
    }

    private void Update()
    {
        // Check if UI references have been destroyed (e.g., during scene transition)
        if (isDisplaying && dialogueCanvasGroup == null)
        {
            StopAllDialogue();
            return;
        }

        // NEW: Alternative input method through InputManager
        if (!advancePressedThisFrame)
        {
            InputManager inputManager = FindObjectOfType<InputManager>();
            if (inputManager != null && inputManager.GetAdvanceDialogueInput())
            {
                advancePressedThisFrame = true;
            }
        }

        if (waitingForInput && advancePressedThisFrame && !isTextRunning)
        {
            advancePressedThisFrame = false;
            AdvanceDialogue();
        }
    }

    private void StopAllDialogue()
    {
        if (currentDialogueCoroutine != null)
        {
            StopCoroutine(currentDialogueCoroutine);
            currentDialogueCoroutine = null;
        }
        HideContinueIndicator();

        isDisplaying = false;
        isSequenceActive = false;
        waitingForInput = false;
        canAdvance = false;
        advancePressedThisFrame = false;
        isTextRunning = false;
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
        isSequenceActive = true;
        currentDialogueCoroutine = StartCoroutine(DisplaySequenceCoroutine());
    }

    public void ShowDialogueSequence(DialogueLine[] lines, bool loop = false)
    {
        DialogueSequence sequence = new DialogueSequence
        {
            lines = lines,
            loopSequence = loop
        };
        isSequenceActive = true;
        ShowDialogueSequence(sequence);
    }

    private void AdvanceDialogue()
    {
        if (!canAdvance) return;

        HideContinueIndicator();
        waitingForInput = false;
        canAdvance = false;
    }

    public void HideDialogue()
    {
        if (currentDialogueCoroutine != null)
        {
            StopCoroutine(currentDialogueCoroutine);
        }

        HideContinueIndicator();

        if (dialogueCanvasGroup != null)
        {
            // Instant hide instead of fade
            dialogueCanvasGroup.alpha = 0f;
        }

        // Reset input state
        advancePressedThisFrame = false;
        waitingForInput = false;
        canAdvance = false;
    }

    private IEnumerator DisplaySingleDialogueCoroutine(DialogueLine dialogueLine)
    {
        isDisplaying = true;

        dialogueLine.text = SetInputPlaceholders(dialogueLine.text);

        if (dialogueLine.useItalics)
        {
            dialogueLine.text = "<i>" + dialogueLine.text + "</i>";
        }

        if (autoChunkLongText)
        {
            currentTextChunks = ChunkText(dialogueLine.text);
        }
        else
        {
            currentTextChunks = new List<string> { dialogueLine.text };
        }

        if (dialogueCanvasGroup == null)
        {
            isDisplaying = false;
            yield break;
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

        if (dialogueCanvasGroup != null)
        {
            // Instant hide instead of fade
            dialogueCanvasGroup.alpha = 0f;
        }

        isDisplaying = false;
    }

    private IEnumerator DisplaySequenceCoroutine()
    {
        isSequenceActive = true;

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

        isSequenceActive = false;
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
        HideContinueIndicator();
        dialogueText.text = text;
        shadowText.text = text;

        dialogueText.ForceMeshUpdate();
        shadowText.ForceMeshUpdate();

        int totalVisibleCharacters = dialogueText.textInfo.characterCount;

        dialogueText.maxVisibleCharacters = 0;
        shadowText.maxVisibleCharacters = 0;

        isTextRunning = true;

        for (int i = 0; i <= totalVisibleCharacters; i++)
        {
            if (advancePressedThisFrame)
            {
                dialogueText.maxVisibleCharacters = totalVisibleCharacters;
                shadowText.maxVisibleCharacters = totalVisibleCharacters;
                advancePressedThisFrame = false;
                isTextRunning = false;
                ShowContinueIndicator();
                yield break;
            }

            dialogueText.maxVisibleCharacters = i;
            shadowText.maxVisibleCharacters = i;

            yield return new WaitForSeconds(typewriterSpeed);
        }

        dialogueText.maxVisibleCharacters = totalVisibleCharacters;
        shadowText.maxVisibleCharacters = totalVisibleCharacters;

        isTextRunning = false;

        ShowContinueIndicator();

    }

    private string SetInputPlaceholders(string text)
    {
        inputDevice = GetCurrentInputDevice();

        if (inputDevice == "Gamepad")
        {
            text = text.Replace("{MOVE}", "LEFT STICK");
            text = text.Replace("{DASH}", "B");
            text = text.Replace("{JUMP}", "A");
            text = text.Replace("{ATTACK}", "X");
            text = text.Replace("{AIM}", "LEFT TRIGGER");
            text = text.Replace("{RANGE}", "RIGHT TRIGGER");
            text = text.Replace("{LOCK_ON}", "RIGHT CLICK");
            text = text.Replace("{SKILLTREE}", "SELECT");
            text = text.Replace("{INTERACT}", "Y");
        }
        else
        {
            text = text.Replace("{MOVE}", "WASD");
            text = text.Replace("{DASH}", "X");
            text = text.Replace("{JUMP}", "SPACE");
            text = text.Replace("{ATTACK}", "LEFT MOUSE BUTTON");
            text = text.Replace("{AIM}", "RIGHT MOUSE BUTTON");
            text = text.Replace("{RANGE}", "LEFT MOUSE BUTTON");
            text = text.Replace("{LOCK_ON}", "R");
            text = text.Replace("{SKILLTREE}", "TAB");
            text = text.Replace("{INTERACT}", "E");
        }

        return text;
    }

    private void ShowContinueIndicator()
    {
        if (continueIndicator != null)
        {
            continueIndicator.SetActive(true);
        }
    }

    private void HideContinueIndicator()
    {
        if (continueIndicator != null)
        {
            continueIndicator.SetActive(false);
        }
    }

    public bool IsDisplaying => isDisplaying || isSequenceActive;
    public bool IsWaitingForInput => waitingForInput;
}