using UnityEngine;

[System.Serializable]
public class CutsceneShot
{
    [Header("Camera Setup")]
    public Camera shotCamera;
    public Transform targetPosition;
    public Vector3 targetPos;
    public Vector3 targetRotation;
    public float transitionDuration = 2f;
    
    [Header("Dialogue")]
    public DialogueLine dialogueLine;
    
    [Header("Animation")]
    public string animationStateName;
    public bool playAnimation = false;
    
    [Header("Player Teleportation")]
    public bool teleportPlayer = false;
    public Transform playerTargetPosition;
    
    [Header("Screen Fade")]
    public bool fadeOutBeforeShot = false;
    public bool fadeInAfterShot = false;
    public float fadeOutDuration = 1f;
    public float fadeInDuration = 1f;
    
    [Header("Shot Settings")]
    public string shotName;
    public bool waitForDialogueCompletion = true;
    public float additionalWaitTime = 0f;
    
    [Header("Advanced Settings")]
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public bool useFieldOfViewTransition = false;
    public float targetFieldOfView = 60f;
    public float fovTransitionDuration = 1f;
    
    [Header("GameObject Activation")]
    public GameObject[] objectsToEnable;
    public GameObject[] objectsToDisable;
    
    public CutsceneShot()
    {
        dialogueLine = new DialogueLine();
        shotName = "New Shot";
    }
    
    public CutsceneShot(string name, Camera camera, Transform target, DialogueLine dialogue)
    {
        shotName = name;
        shotCamera = camera;
        targetPosition = target;
        dialogueLine = dialogue;
        
        if (target != null)
        {
            targetPos = target.position;
            targetRotation = target.eulerAngles;
        }
    }
    
    public Vector3 GetTargetPosition()
    {
        return targetPosition != null ? targetPosition.position : targetPos;
    }
    
    public Quaternion GetTargetRotation()
    {
        return targetPosition != null ? targetPosition.rotation : Quaternion.Euler(targetRotation);
    }
}