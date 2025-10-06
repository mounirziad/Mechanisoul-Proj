using UnityEngine;
using UnityEngine.Playables;

public class CutsceneMixerBehaviour : PlayableBehaviour
{
    private CutsceneManager cutsceneManager;
    
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        cutsceneManager = playerData as CutsceneManager;
        
        if (cutsceneManager == null)
            return;
        
        int inputCount = playable.GetInputCount();
        
        for (int i = 0; i < inputCount; i++)
        {
            float inputWeight = playable.GetInputWeight(i);
            ScriptPlayable<CutsceneBehaviour> inputPlayable = (ScriptPlayable<CutsceneBehaviour>)playable.GetInput(i);
            CutsceneBehaviour input = inputPlayable.GetBehaviour();
            
            if (inputWeight > 0f)
            {
                input.ProcessFrame(inputPlayable, info, playerData);
            }
        }
    }
}