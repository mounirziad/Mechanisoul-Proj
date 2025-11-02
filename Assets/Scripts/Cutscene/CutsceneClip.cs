using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class CutsceneClip : PlayableAsset, ITimelineClipAsset
{
    public CutsceneBehaviour template = new CutsceneBehaviour();
    
    public ClipCaps clipCaps
    {
        get { return ClipCaps.None; }
    }
    
    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var playable = ScriptPlayable<CutsceneBehaviour>.Create(graph, template);
        return playable;
    }
}

[Serializable]
public class CutsceneBehaviour : PlayableBehaviour
{
    [Header("Camera Target")]
    public Transform cameraTarget;
    public Vector3 cameraPosition;
    public Vector3 cameraRotation;
    public float transitionDuration = 2f;
    
    [Header("Dialogue")]
    [TextArea(3, 5)]
    public string dialogueText;
    public string speakerName;
    public AudioClip voiceClip;
    public bool waitForDialogueCompletion = true;
    
    [Header("Camera Settings")]
    public bool useFieldOfViewTransition = false;
    public float targetFieldOfView = 60f;
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    
    [Header("GameObject Activation")]
    public GameObject[] objectsToEnable;
    public GameObject[] objectsToDisable;
    
    private CutsceneManager cutsceneManager;
    private bool hasTriggered = false;
    
    public override void OnPlayableCreate(Playable playable)
    {
        cutsceneManager = CutsceneManager.Instance;
    }
    
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        cutsceneManager = playerData as CutsceneManager;
        
        if (cutsceneManager == null)
            return;
        
        float time = (float)playable.GetTime();
        float duration = (float)playable.GetDuration();
        
        if (time > 0 && time < duration && !hasTriggered)
        {
            hasTriggered = true;
            TriggerCutsceneShot();
        }
        
        if (time >= duration && hasTriggered)
        {
            hasTriggered = false;
        }
    }
    
    private void TriggerCutsceneShot()
    {
        ActivateGameObjects();
        
        CutsceneShot shot = new CutsceneShot();
        shot.shotCamera = cutsceneManager.CurrentCamera;
        shot.targetPosition = cameraTarget;
        shot.targetPos = cameraPosition;
        shot.targetRotation = cameraRotation;
        shot.transitionDuration = transitionDuration;
        shot.useFieldOfViewTransition = useFieldOfViewTransition;
        shot.targetFieldOfView = targetFieldOfView;
        shot.transitionCurve = transitionCurve;
        shot.waitForDialogueCompletion = waitForDialogueCompletion;
        
        shot.dialogueLine = new DialogueLine
        {
            text = dialogueText,
            speakerName = speakerName,
            voiceClip = voiceClip,
            waitForInput = waitForDialogueCompletion,
            displayDuration = waitForDialogueCompletion ? 0f : 3f
        };
        
        cutsceneManager.AddShot(shot);
        
        if (!cutsceneManager.IsPlaying)
        {
            cutsceneManager.PlayCutscene();
        }
    }
    
    private void ActivateGameObjects()
    {
        if (objectsToEnable != null)
        {
            foreach (GameObject obj in objectsToEnable)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                }
            }
        }
        
        if (objectsToDisable != null)
        {
            foreach (GameObject obj in objectsToDisable)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                }
            }
        }
    }
    
    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (hasTriggered)
        {
            hasTriggered = false;
        }
    }
}