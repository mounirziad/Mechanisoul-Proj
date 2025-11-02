using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    [Header("Cutscene Setup")]
    [SerializeField] private CutsceneShot[] shots;
    [SerializeField] private bool playOnStart = false;
    [SerializeField] private bool loopCutscene = false;

    [Header("Character Setup")]
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Transform playerTransform;

    [Header("Scene Transition")]
    [SerializeField] private string sceneToLoadOnComplete;
    [SerializeField] private bool transitionOnLastDialogueInput = true;
    [SerializeField] private float delayBeforeSceneTransition = 0.5f;

    [Header("Camera Management")]
    [SerializeField] private Camera[] cutsceneCameras;
    [SerializeField] private Camera originalMainCamera;
    [SerializeField] private bool smoothTransitions = true;
    [SerializeField] private float defaultTransitionSpeed = 2f;

    [Header("Screen Fade")]
    [SerializeField] private FadingScript fadingScript;

    [Header("Events")]
    public UnityEvent OnCutsceneStart;
    public UnityEvent OnCutsceneComplete;
    public UnityEvent<int> OnShotStart;
    public UnityEvent<int> OnShotComplete;
    public UnityEvent<Camera> OnCameraSwitch;

    private int currentShotIndex = 0;
    private bool isPlayingCutscene = false;
    private bool isWaitingForDialogue = false;
    private bool isWaitingForLastDialogue = false;
    private Coroutine cutsceneCoroutine;
    private Coroutine cameraTransitionCoroutine;

    private Dictionary<Camera, Vector3> originalCameraPositions = new Dictionary<Camera, Vector3>();
    private Dictionary<Camera, Quaternion> originalCameraRotations = new Dictionary<Camera, Quaternion>();
    private Dictionary<Camera, float> originalCameraFOVs = new Dictionary<Camera, float>();
    private Dictionary<Camera, float> originalCameraDepths = new Dictionary<Camera, float>();
    private Dictionary<Camera, bool> originalCameraStates = new Dictionary<Camera, bool>();

    public static CutsceneManager Instance { get; private set; }

    public bool IsPlaying => isPlayingCutscene;
    public int CurrentShotIndex => currentShotIndex;
    public CutsceneShot CurrentShot => currentShotIndex < shots.Length ? shots[currentShotIndex] : null;
    public Camera CurrentCamera => CurrentShot?.shotCamera;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (originalMainCamera == null)
        {
            originalMainCamera = Camera.main;
        }
    }

    private void Start()
    {
        InitializeCameras();

        if (playOnStart)
        {
            Invoke(nameof(PlayCutscene), 0.1f);
        }
    }

    private void Update()
    {
        if (isWaitingForDialogue && DialogueSystem.Instance != null)
        {
            if (!DialogueSystem.Instance.IsDisplaying && !DialogueSystem.Instance.IsWaitingForInput)
            {
                isWaitingForDialogue = false;
            }
        }

        if (isWaitingForLastDialogue && DialogueSystem.Instance != null)
        {
            if (!DialogueSystem.Instance.IsDisplaying && !DialogueSystem.Instance.IsWaitingForInput)
            {
                isWaitingForLastDialogue = false;
                
                if (transitionOnLastDialogueInput && !string.IsNullOrEmpty(sceneToLoadOnComplete))
                {
                    LoadSceneImmediately();
                }
            }
        }
    }

    private void InitializeCameras()
    {
        // Store original states but DON'T change any camera states
        if (cutsceneCameras != null)
        {
            foreach (Camera cam in cutsceneCameras)
            {
                if (cam != null)
                {
                    originalCameraPositions[cam] = cam.transform.position;
                    originalCameraRotations[cam] = cam.transform.rotation;
                    originalCameraFOVs[cam] = cam.fieldOfView;
                    originalCameraDepths[cam] = cam.depth;
                    originalCameraStates[cam] = cam.enabled;
                    Debug.Log($"Stored state for camera: {cam.name} (enabled: {cam.enabled}, depth: {cam.depth})");
                }
            }
        }

        if (originalMainCamera != null)
        {
            originalCameraPositions[originalMainCamera] = originalMainCamera.transform.position;
            originalCameraRotations[originalMainCamera] = originalMainCamera.transform.rotation;
            originalCameraFOVs[originalMainCamera] = originalMainCamera.fieldOfView;
            originalCameraDepths[originalMainCamera] = originalMainCamera.depth;
            originalCameraStates[originalMainCamera] = originalMainCamera.enabled;
            Debug.Log($"Stored state for main camera: {originalMainCamera.name} (enabled: {originalMainCamera.enabled}, depth: {originalMainCamera.depth})");
        }
    }

    public void PlayCutscene()
    {
        if (isPlayingCutscene)
        {
            Debug.LogWarning("Cutscene is already playing!");
            return;
        }

        if (shots == null || shots.Length == 0)
        {
            Debug.LogWarning("CutsceneManager: No shots configured!");
            return;
        }

        cutsceneCoroutine = StartCoroutine(PlayCutsceneCoroutine());
    }

    public void StopCutscene()
    {
        if (cutsceneCoroutine != null)
        {
            StopCoroutine(cutsceneCoroutine);
            cutsceneCoroutine = null;
        }

        if (cameraTransitionCoroutine != null)
        {
            StopCoroutine(cameraTransitionCoroutine);
            cameraTransitionCoroutine = null;
        }

        isPlayingCutscene = false;
        isWaitingForDialogue = false;
        isWaitingForLastDialogue = false;

        RestoreAllCameras();

        if (DialogueSystem.Instance != null)
        {
            DialogueSystem.Instance.HideDialogue();
        }

        OnCutsceneComplete?.Invoke();
        Debug.Log("Cutscene stopped");

        if (!transitionOnLastDialogueInput && !string.IsNullOrEmpty(sceneToLoadOnComplete))
        {
            StartCoroutine(LoadSceneAfterDelay());
        }
    }

    private IEnumerator LoadSceneAfterDelay()
    {
        Debug.Log($"Transitioning to scene '{sceneToLoadOnComplete}' in {delayBeforeSceneTransition} seconds...");
        
        yield return new WaitForSeconds(delayBeforeSceneTransition);

        Debug.Log($"Loading scene: {sceneToLoadOnComplete}");
        SceneManager.LoadScene(sceneToLoadOnComplete);
    }

    private void LoadSceneImmediately()
    {
        Debug.Log($"Loading scene immediately: {sceneToLoadOnComplete}");
        
        isPlayingCutscene = false;
        isWaitingForLastDialogue = false;
        
        if (cutsceneCoroutine != null)
        {
            StopCoroutine(cutsceneCoroutine);
            cutsceneCoroutine = null;
        }
        
        if (delayBeforeSceneTransition > 0f)
        {
            StartCoroutine(LoadSceneWithMinimalDelay());
        }
        else
        {
            SceneManager.LoadScene(sceneToLoadOnComplete);
        }
    }

    private IEnumerator LoadSceneWithMinimalDelay()
    {
        yield return new WaitForSeconds(delayBeforeSceneTransition);
        SceneManager.LoadScene(sceneToLoadOnComplete);
    }

    public void SetSceneToLoad(string sceneName)
    {
        sceneToLoadOnComplete = sceneName;
    }

    public void NextShot()
    {
        if (!isPlayingCutscene) return;

        currentShotIndex++;

        if (currentShotIndex >= shots.Length)
        {
            if (loopCutscene)
            {
                currentShotIndex = 0;
            }
            else
            {
                StopCutscene();
                return;
            }
        }
    }

    public void SetShots(CutsceneShot[] newShots)
    {
        shots = newShots;
    }

    public void AddShot(CutsceneShot shot)
    {
        if (shots == null)
        {
            shots = new CutsceneShot[] { shot };
        }
        else
        {
            CutsceneShot[] newShots = new CutsceneShot[shots.Length + 1];
            shots.CopyTo(newShots, 0);
            newShots[shots.Length] = shot;
            shots = newShots;
        }
    }

    private IEnumerator PlayCutsceneCoroutine()
    {
        Debug.Log("Cutscene starting - using camera depth system");

        isPlayingCutscene = true;
        currentShotIndex = 0;

        // Set up all cameras with proper depth but keep them enabled
        PrepareAllCamerasForCutscene();

        OnCutsceneStart?.Invoke();

        for (currentShotIndex = 0; currentShotIndex < shots.Length; currentShotIndex++)
        {
            if (!isPlayingCutscene) break;

            Debug.Log($"Starting shot {currentShotIndex} with camera: {shots[currentShotIndex].shotCamera?.name}");
            yield return StartCoroutine(PlayShotCoroutine(shots[currentShotIndex]));
        }

        if (isPlayingCutscene)
        {
            if (loopCutscene)
            {
                Debug.Log("Looping cutscene");
                yield return StartCoroutine(PlayCutsceneCoroutine());
            }
            else
            {
                StopCutscene();
            }
        }
    }

    private void PrepareAllCamerasForCutscene()
    {
        // Disable all cameras initially to prevent flicker
        if (originalMainCamera != null)
        {
            originalMainCamera.enabled = false;
        }

        if (cutsceneCameras != null)
        {
            foreach (Camera cam in cutsceneCameras)
            {
                if (cam != null)
                {
                    cam.enabled = false;
                    cam.gameObject.SetActive(true); // Ensure GameObject is active for later use
                }
            }
        }
    }

    private IEnumerator PlayShotCoroutine(CutsceneShot shot)
    {
        if (shot.shotCamera == null)
        {
            Debug.LogWarning($"Shot {currentShotIndex} has no camera assigned!");
            yield break;
        }

        OnShotStart?.Invoke(currentShotIndex);

        if (shot.fadeOutBeforeShot && fadingScript != null)
        {
            yield return StartCoroutine(FadeOut(shot.fadeOutDuration));
        }

        SwitchToCameraUsingDepth(shot.shotCamera);

        bool isLastShot = currentShotIndex >= shots.Length - 1;

        if (shot.teleportPlayer && shot.playerTargetPosition != null)
        {
            TeleportPlayer(shot.playerTargetPosition);
        }
        
        ActivateGameObjectsForShot(shot);

        if (shot.playAnimation && !string.IsNullOrEmpty(shot.animationStateName))
        {
            if (playerAnimator != null)
            {
                playerAnimator.Play(shot.animationStateName);
                Debug.Log($"Playing animation: {shot.animationStateName}");
            }
            else
            {
                Debug.LogWarning("Player Animator is not assigned in CutsceneManager!");
            }
        }

        if (shot.fadeInAfterShot && fadingScript != null)
        {
            yield return StartCoroutine(FadeIn(shot.fadeInDuration));
        }

        if (shot.dialogueLine != null && !string.IsNullOrEmpty(shot.dialogueLine.text))
        {
            if (DialogueSystem.Instance != null)
            {
                DialogueSystem.Instance.ShowDialogue(shot.dialogueLine);
            }
        }

        if (cameraTransitionCoroutine != null)
        {
            StopCoroutine(cameraTransitionCoroutine);
        }

        cameraTransitionCoroutine = StartCoroutine(TransitionCameraToTarget(shot));

        yield return cameraTransitionCoroutine;

        if (shot.dialogueLine != null && !string.IsNullOrEmpty(shot.dialogueLine.text))
        {
            if (DialogueSystem.Instance != null)
            {
                if (shot.waitForDialogueCompletion)
                {
                    if (isLastShot && transitionOnLastDialogueInput && !string.IsNullOrEmpty(sceneToLoadOnComplete))
                    {
                        isWaitingForLastDialogue = true;
                        yield return new WaitUntil(() => !isWaitingForLastDialogue);
                        yield break;
                    }
                    else
                    {
                        isWaitingForDialogue = true;
                        yield return new WaitUntil(() => !isWaitingForDialogue);
                    }
                }
            }
        }

        if (shot.additionalWaitTime > 0f)
        {
            yield return new WaitForSeconds(shot.additionalWaitTime);
        }

        OnShotComplete?.Invoke(currentShotIndex);
    }

    private void SwitchToCameraUsingDepth(Camera targetCamera)
    {
        if (targetCamera == null)
        {
            Debug.LogWarning("Attempted to switch to null camera!");
            return;
        }

        // First disable all other cameras to prevent any overlap
        if (originalMainCamera != null && originalMainCamera != targetCamera)
        {
            originalMainCamera.enabled = false;
        }

        if (cutsceneCameras != null)
        {
            foreach (Camera cam in cutsceneCameras)
            {
                if (cam != null && cam != targetCamera)
                {
                    cam.enabled = false;
                }
            }
        }

        // Now enable and configure the target camera
        targetCamera.gameObject.SetActive(true);
        targetCamera.enabled = true;
        targetCamera.depth = 10f; // Set to high depth for priority

        OnCameraSwitch?.Invoke(targetCamera);

        Debug.Log($"Switched to camera using depth: {targetCamera.name} (depth: {targetCamera.depth}, enabled: {targetCamera.enabled})");
    }

    private IEnumerator TransitionCameraToTarget(CutsceneShot shot)
    {
        Camera cam = shot.shotCamera;
        if (cam == null) yield break;

        Vector3 startPos = cam.transform.position;
        Quaternion startRot = cam.transform.rotation;
        float startFOV = cam.fieldOfView;

        Vector3 targetPos = shot.GetTargetPosition();
        Quaternion targetRot = shot.GetTargetRotation();
        float targetFOV = shot.useFieldOfViewTransition ? shot.targetFieldOfView : startFOV;

        float duration = shot.transitionDuration;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (!isPlayingCutscene) yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float curvedT = shot.transitionCurve.Evaluate(t);

            if (smoothTransitions)
            {
                cam.transform.position = Vector3.Lerp(startPos, targetPos, curvedT);
                cam.transform.rotation = Quaternion.Lerp(startRot, targetRot, curvedT);

                if (shot.useFieldOfViewTransition)
                {
                    float fovT = Mathf.Clamp01(elapsed / shot.fovTransitionDuration);
                    cam.fieldOfView = Mathf.Lerp(startFOV, targetFOV, fovT);
                }
            }
            else
            {
                if (t >= 1f)
                {
                    cam.transform.position = targetPos;
                    cam.transform.rotation = targetRot;
                    if (shot.useFieldOfViewTransition)
                    {
                        cam.fieldOfView = targetFOV;
                    }
                }
            }

            yield return null;
        }

        // Ensure final position
        cam.transform.position = targetPos;
        cam.transform.rotation = targetRot;
        if (shot.useFieldOfViewTransition)
        {
            cam.fieldOfView = targetFOV;
        }
    }

    private void RestoreAllCameras()
    {
        // First disable all cutscene cameras
        if (cutsceneCameras != null)
        {
            foreach (Camera cam in cutsceneCameras)
            {
                if (cam != null)
                {
                    cam.enabled = false; // Disable first to prevent conflicts
                    
                    if (originalCameraPositions.ContainsKey(cam))
                    {
                        cam.transform.position = originalCameraPositions[cam];
                        cam.transform.rotation = originalCameraRotations[cam];
                        cam.fieldOfView = originalCameraFOVs[cam];
                        cam.depth = originalCameraDepths[cam];
                        cam.enabled = originalCameraStates[cam];
                    }
                    else
                    {
                        // If no stored state, disable the camera
                        cam.gameObject.SetActive(false);
                    }
                }
            }
        }

        // Then restore and enable main camera
        if (originalMainCamera != null)
        {
            if (originalCameraPositions.ContainsKey(originalMainCamera))
            {
                originalMainCamera.transform.position = originalCameraPositions[originalMainCamera];
                originalMainCamera.transform.rotation = originalCameraRotations[originalMainCamera];
                originalMainCamera.fieldOfView = originalCameraFOVs[originalMainCamera];
                originalMainCamera.depth = originalCameraDepths[originalMainCamera];
                originalMainCamera.enabled = originalCameraStates[originalMainCamera];
            }
            else
            {
                originalMainCamera.gameObject.SetActive(true);
                originalMainCamera.enabled = true;
                originalMainCamera.depth = 0f; // Default main camera depth
            }
        }
    }

    private IEnumerator FadeOut(float duration)
    {
        if (fadingScript == null)
        {
            Debug.LogWarning("FadingScript is not assigned in CutsceneManager!");
            yield break;
        }

        float elapsed = 0f;
        CanvasGroup canvasGroup = fadingScript.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            Debug.LogWarning("CanvasGroup not found on FadingScript GameObject!");
            yield break;
        }

        float startAlpha = canvasGroup.alpha;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeIn(float duration)
    {
        if (fadingScript == null)
        {
            Debug.LogWarning("FadingScript is not assigned in CutsceneManager!");
            yield break;
        }

        float elapsed = 0f;
        CanvasGroup canvasGroup = fadingScript.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            Debug.LogWarning("CanvasGroup not found on FadingScript GameObject!");
            yield break;
        }

        float startAlpha = canvasGroup.alpha;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }

    private void TeleportPlayer(Transform targetTransform)
    {
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogWarning("Player Transform is not assigned and could not find GameObject with 'Player' tag!");
                return;
            }
        }

        playerTransform.position = targetTransform.position;
        playerTransform.rotation = targetTransform.rotation;

        Debug.Log($"Teleported player to position: {targetTransform.position}, rotation: {targetTransform.rotation.eulerAngles}");
    }
    
    private void ActivateGameObjectsForShot(CutsceneShot shot)
    {
        if (shot.objectsToEnable != null)
        {
            foreach (GameObject obj in shot.objectsToEnable)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                    Debug.Log($"Enabled GameObject: {obj.name}");
                }
            }
        }
        
        if (shot.objectsToDisable != null)
        {
            foreach (GameObject obj in shot.objectsToDisable)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                    Debug.Log($"Disabled GameObject: {obj.name}");
                }
            }
        }
    }

    [ContextMenu("Debug Camera States")]
    public void DebugCameraStates()
    {
        Debug.Log("=== CAMERA STATES ===");
        Debug.Log($"Main Camera: {Camera.main?.name} - enabled: {Camera.main?.enabled} - depth: {Camera.main?.depth}");

        if (cutsceneCameras != null)
        {
            foreach (Camera cam in cutsceneCameras)
            {
                if (cam != null)
                    Debug.Log($"Cutscene Camera: {cam.name} - enabled: {cam.enabled} - active: {cam.gameObject.activeInHierarchy} - depth: {cam.depth}");
            }
        }

        Debug.Log($"Is Playing Cutscene: {isPlayingCutscene}");
        Debug.Log($"Current Shot Index: {currentShotIndex}");
    }
}