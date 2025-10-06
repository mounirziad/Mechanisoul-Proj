using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class DialogueClip : PlayableAsset, ITimelineClipAsset
{
    public DialogueBehaviour template = new DialogueBehaviour();
    
    public ClipCaps clipCaps
    {
        get { return ClipCaps.Blending; }
    }
    
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<DialogueBehaviour>.Create(graph, template);
        return playable;
    }
}

[Serializable]
public class DialogueBehaviour : PlayableBehaviour
{
    [TextArea(3, 5)]
    public string dialogueText;
    public string speakerName;
    public AudioClip voiceClip;
    public bool useTypewriterEffect = true;
    public float typewriterSpeed = 0.05f;
    
    private DialogueSystem dialogueSystem;
    private bool hasTriggered = false;
    
    public override void OnPlayableCreate(Playable playable)
    {
        dialogueSystem = DialogueSystem.Instance;
    }
    
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        dialogueSystem = playerData as DialogueSystem;
        
        if (dialogueSystem == null)
            return;
        
        float time = (float)playable.GetTime();
        float duration = (float)playable.GetDuration();
        
        if (time > 0 && time < duration && !hasTriggered)
        {
            hasTriggered = true;
            
            DialogueLine line = new DialogueLine
            {
                text = dialogueText,
                displayDuration = duration,
                speakerName = speakerName,
                voiceClip = voiceClip
            };
            
            dialogueSystem.ShowDialogue(line);
        }
        
        if (time >= duration && hasTriggered)
        {
            hasTriggered = false;
        }
    }
    
    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (dialogueSystem != null && hasTriggered)
        {
            dialogueSystem.HideDialogue();
            hasTriggered = false;
        }
    }
}