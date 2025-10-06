using UnityEngine;
using System.Collections;

public class DialogueExample : MonoBehaviour
{
    [Header("Example Dialogue Lines")]
    [SerializeField] private DialogueLine[] exampleDialogues;
    
    [Header("Controls")]
    [SerializeField] private KeyCode triggerKey = KeyCode.Space;
    
    private int currentDialogueIndex = 0;
    
    private void Start()
    {
        SetupExampleDialogues();
    }
    
    private void SetupExampleDialogues()
    {
        if (exampleDialogues == null || exampleDialogues.Length == 0)
        {
            exampleDialogues = new DialogueLine[]
            {
                new DialogueLine 
                { 
                    text = "Welcome to the dialogue system demonstration.", 
                    displayDuration = 3f, 
                    speakerName = "System" 
                },
                new DialogueLine 
                { 
                    text = "This text appears at the bottom of the screen with a shadow for visibility.", 
                    displayDuration = 4f, 
                    speakerName = "Narrator" 
                },
                new DialogueLine 
                { 
                    text = "Perfect for cutscenes and in-game conversations.", 
                    displayDuration = 3f, 
                    speakerName = "Guide" 
                },
                new DialogueLine 
                { 
                    text = "Press Space to cycle through these examples.", 
                    displayDuration = 3f, 
                    speakerName = "System" 
                }
            };
        }
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(triggerKey))
        {
            TriggerNextDialogue();
        }
    }
    
    public void TriggerNextDialogue()
    {
        if (DialogueSystem.Instance == null) return;
        
        if (currentDialogueIndex < exampleDialogues.Length)
        {
            DialogueSystem.Instance.ShowDialogue(exampleDialogues[currentDialogueIndex]);
            currentDialogueIndex++;
        }
        else
        {
            currentDialogueIndex = 0;
            DialogueSystem.Instance.ShowDialogue(exampleDialogues[currentDialogueIndex]);
            currentDialogueIndex++;
        }
    }
    
    public void ShowCustomDialogue(string text, float duration = 3f)
    {
        if (DialogueSystem.Instance != null)
        {
            DialogueSystem.Instance.ShowDialogue(text, duration);
        }
    }
    
    public void StartCutsceneDialogueSequence()
    {
        StartCoroutine(CutsceneDialogueCoroutine());
    }
    
    private IEnumerator CutsceneDialogueCoroutine()
    {
        string[] cutsceneLines = {
            "Shot 1: Character approaches the mysterious door.",
            "Shot 2: The door creaks open revealing darkness within.",
            "Shot 3: A voice echoes from the shadows...",
            "Shot 4: 'Welcome, traveler. We've been expecting you.'"
        };
        
        for (int i = 0; i < cutsceneLines.Length; i++)
        {
            DialogueSystem.Instance.ShowDialogue(cutsceneLines[i], 3f);
            yield return new WaitForSeconds(4f);
        }
    }
}