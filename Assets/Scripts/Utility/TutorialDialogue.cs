using UnityEngine;

public class TutorialDialogue : MonoBehaviour
{
    [Header("Dialogue Testing")]
    public DialogueSystem dialogueSystem;
    
    [Header("Test Options")]
    [SerializeField] private bool testOnStart = true;
    [SerializeField] private TestType testType = TestType.LongTextChunking;
    
    [Header("Custom Test Text")]
    [TextArea(3, 8)]
    [SerializeField] private string customText = "This is a very long piece of dialogue that should automatically be broken into smaller, readable chunks. Each chunk will display one after another, making it much easier to read and follow. This mimics how BioShock Infinite handles longer dialogue sequences by breaking them into digestible pieces rather than overwhelming the player with a wall of text.";
    
    private enum TestType
    {
        SimpleText,
        LongTextChunking,
        MultipleLines,
        WaitForInput
    }
    
    void Start()
    {
        if (testOnStart && dialogueSystem != null)
        {
            Invoke(nameof(RunTest), 1f);
        }
    }
    
    void Update()
    {
       
    }
    
    public void RunTest()
    {
        if (dialogueSystem == null)
        {
            dialogueSystem = DialogueSystem.Instance;
        }
        
        if (dialogueSystem == null)
        {
            Debug.LogWarning("DialogueSystem not found!");
            return;
        }
        
        switch (testType)
        {
            case TestType.SimpleText:
                TestSimpleText();
                break;
            case TestType.LongTextChunking:
                TestLongTextChunking();
                break;
            case TestType.MultipleLines:
                TestMultipleLines();
                break;
            case TestType.WaitForInput:
                TestWaitForInput();
                break;
        }
    }
    
    private void TestSimpleText()
    {
        dialogueSystem.ShowDialogue("This is a simple dialogue line that fits in one chunk.", 3f);
    }
    
    private void TestLongTextChunking()
    {
        dialogueSystem.ShowDialogue(customText, 2f);
    }
    
    private void TestMultipleLines()
    {
        DialogueLine[] lines = new DialogueLine[]
        {
            new DialogueLine { text = "Welcome to the facility, operative.", displayDuration = 3f, autoAdvance = true },
            new DialogueLine { text = "Your mission parameters have been updated. Please proceed to the extraction point.", displayDuration = 4f, autoAdvance = true },
            new DialogueLine { text = "Be advised: hostile entities detected in the area. Exercise extreme caution.", displayDuration = 4f, autoAdvance = true },
            new DialogueLine { text = "Good luck, agent. The fate of the mission rests in your hands.", displayDuration = 3f, autoAdvance = true }
        };
        
        dialogueSystem.ShowDialogueSequence(lines);
    }
    
    private void TestWaitForInput()
    {
        DialogueLine[] lines = new DialogueLine[]
        {
            new DialogueLine { text = "This dialogue waits for your JUMP input. Press your jump button to continue.", waitForInput = true },
            new DialogueLine { text = "Perfect! Now you can use jump to advance dialogue without interfering with gameplay.", waitForInput = true },
            new DialogueLine { text = "When dialogue isn't active, jump works normally for movement.", displayDuration = 4f, autoAdvance = true }
        };
        
        dialogueSystem.ShowDialogueSequence(lines);
    }
}
