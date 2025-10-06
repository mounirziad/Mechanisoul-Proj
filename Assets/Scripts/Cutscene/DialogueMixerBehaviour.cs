using UnityEngine;
using UnityEngine.Playables;

public class DialogueMixerBehaviour : PlayableBehaviour
{
    private DialogueSystem dialogueSystem;
    
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        dialogueSystem = playerData as DialogueSystem;
        
        if (dialogueSystem == null)
            return;
        
        int inputCount = playable.GetInputCount();
        
        for (int i = 0; i < inputCount; i++)
        {
            float inputWeight = playable.GetInputWeight(i);
            ScriptPlayable<DialogueBehaviour> inputPlayable = (ScriptPlayable<DialogueBehaviour>)playable.GetInput(i);
            DialogueBehaviour input = inputPlayable.GetBehaviour();
            
            if (inputWeight > 0f)
            {
                input.ProcessFrame(inputPlayable, info, playerData);
            }
        }
    }
}