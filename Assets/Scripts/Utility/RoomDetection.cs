using UnityEngine;
using System.Collections;

public class RoomDetection : MonoBehaviour
{
    [Header("Dialogue Settings")]
    [SerializeField] private DialogueSequence roomDialogue;
    
    [Header("Custom Dialogue")]
    [SerializeField] private bool useCustomLines = true;
    [SerializeField] private DialogueLine[] customDialogueLines;
    
    [Header("Trigger Settings")]
    [SerializeField] private bool triggerOnce = true;
    [SerializeField] private bool requiresTag = true;
    [SerializeField] private string requiredTag = "Player";
    
    [Header("Door Settings")]
    [SerializeField] private GameObject doorToOpen;
    [SerializeField] private bool openDoorAfterDialogue = false;
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private bool hasTriggered = false;
    private DialogueSystem dialogueSystem;
    
    private void Start()
    {
        dialogueSystem = DialogueSystem.Instance;
        
        if (dialogueSystem == null)
        {
            Debug.LogError("DialogueSystem instance not found! Make sure DialogueSystem exists in the scene.");
        }
        
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            Debug.LogWarning("No BoxCollider found on RoomDetection GameObject. Adding one automatically.");
            boxCollider = gameObject.AddComponent<BoxCollider>();
        }
        
        if (!boxCollider.isTrigger)
        {
            boxCollider.isTrigger = true;
            if (showDebugLogs)
            {
                Debug.Log("BoxCollider on RoomDetection set to trigger.");
            }
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnce && hasTriggered)
        {
            return;
        }
        
        if (requiresTag && !other.CompareTag(requiredTag))
        {
            return;
        }
        
        if (dialogueSystem == null)
        {
            Debug.LogError("Cannot show dialogue - DialogueSystem is null!");
            return;
        }
        
        TriggerRoomDialogue();
        
        if (triggerOnce)
        {
            hasTriggered = true;
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"Player entered room: {gameObject.name}. Dialogue triggered.");
        }
        
        if (openDoorAfterDialogue && doorToOpen != null)
        {
            StartCoroutine(WaitForDialogueAndOpenDoor());
        }
    }
    
    private void TriggerRoomDialogue()
    {
        if (useCustomLines && customDialogueLines != null && customDialogueLines.Length > 0)
        {
            dialogueSystem.ShowDialogueSequence(customDialogueLines);
        }
        else if (roomDialogue != null && roomDialogue.lines != null && roomDialogue.lines.Length > 0)
        {
            dialogueSystem.ShowDialogueSequence(roomDialogue);
        }
        else
        {
            Debug.LogWarning("No dialogue configured for RoomDetection on " + gameObject.name);
        }
    }
    
    public void ResetTrigger()
    {
        hasTriggered = false;
    }
    
    public void ManualTrigger()
    {
        if (dialogueSystem != null)
        {
            TriggerRoomDialogue();
        }
    }
    
    private IEnumerator WaitForDialogueAndOpenDoor()
    {
        yield return new WaitUntil(() => !dialogueSystem.IsDisplaying);
        
        if (doorToOpen != null)
        {
            doorToOpen.SetActive(false);
            
            if (showDebugLogs)
            {
                Debug.Log($"Door opened: {doorToOpen.name}");
            }
        }
    }
}
