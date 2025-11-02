using UnityEngine;

public class CutsceneExample : MonoBehaviour
{
    [Header("Cutscene Setup")]
    [SerializeField] private CutsceneManager cutsceneManager;
    [SerializeField] private Camera[] cutsceneCameras;
    [SerializeField] private Transform[] cameraTargets;
    [SerializeField] private string[] dialogueTexts;
    [SerializeField] private bool testOnStart = true;

    [Header("Animation Setup")]
    [SerializeField] private string[] animationStateNames;
    [SerializeField] private bool enableAnimations = false;

    [Header("Scene Transition")]
    [SerializeField] private string sceneToLoadAfterCutscene;

    [Header("Test Settings")]
    [SerializeField] private float transitionDuration = 2f;
    [SerializeField] private bool waitForDialogue = true;
    [SerializeField] private bool createCamerasIfMissing = true;

    private void Start()
    {
        if (testOnStart)
        {
            // Increased delay to ensure everything is initialized
            Invoke(nameof(StartExampleCutscene), 1.5f);
        }
    }

    public void StartExampleCutscene()
    {
        if (cutsceneManager == null)
        {
            cutsceneManager = FindFirstObjectByType<CutsceneManager>();
        }

        if (cutsceneManager == null)
        {
            Debug.LogWarning("CutsceneManager not found!");
            return;
        }

        Debug.Log("Starting example cutscene...");
        
        if (!string.IsNullOrEmpty(sceneToLoadAfterCutscene))
        {
            cutsceneManager.SetSceneToLoad(sceneToLoadAfterCutscene);
            Debug.Log($"Scene '{sceneToLoadAfterCutscene}' will load after cutscene completes");
        }
        
        SetupCamerasAndTargets();
        CreateExampleShots();
        cutsceneManager.PlayCutscene();
    }

    public void StopCutscene()
    {
        if (cutsceneManager != null)
        {
            cutsceneManager.StopCutscene();
        }
    }

    private void SetupCamerasAndTargets()
    {
        if (createCamerasIfMissing)
        {
            if (cutsceneCameras == null || cutsceneCameras.Length == 0)
            {
                CreateCutsceneCameras();
            }

            if (cameraTargets == null || cameraTargets.Length == 0)
            {
                CreateCameraTargets();
            }
        }
    }

    private void CreateExampleShots()
    {
        if (cutsceneCameras == null || cutsceneCameras.Length == 0)
        {
            Debug.LogWarning("No cutscene cameras available!");
            return;
        }

        int shotCount = Mathf.Min(cutsceneCameras.Length, cameraTargets?.Length ?? cutsceneCameras.Length);
        CutsceneShot[] shots = new CutsceneShot[shotCount];

        for (int i = 0; i < shotCount; i++)
        {
            shots[i] = new CutsceneShot();
            shots[i].shotName = $"Shot {i + 1}";
            shots[i].shotCamera = cutsceneCameras[i];

            if (cameraTargets != null && i < cameraTargets.Length)
            {
                shots[i].targetPosition = cameraTargets[i];
            }
            else
            {
                shots[i].targetPos = cutsceneCameras[i].transform.position + Vector3.forward * 2f;
                shots[i].targetRotation = cutsceneCameras[i].transform.eulerAngles;
            }

            shots[i].transitionDuration = transitionDuration;
            shots[i].waitForDialogueCompletion = waitForDialogue;

            if (enableAnimations && animationStateNames != null && i < animationStateNames.Length && !string.IsNullOrEmpty(animationStateNames[i]))
            {
                shots[i].playAnimation = true;
                shots[i].animationStateName = animationStateNames[i];
            }

            string dialogueText = "";
            if (dialogueTexts != null && i < dialogueTexts.Length && !string.IsNullOrEmpty(dialogueTexts[i]))
            {
                dialogueText = dialogueTexts[i];
            }
            else
            {
                dialogueText = $"Shot {i + 1}: Camera '{cutsceneCameras[i].name}' is now active and moving to its target position. Press jump to continue to the next camera.";
            }

            shots[i].dialogueLine = new DialogueLine
            {
                text = dialogueText,
                displayDuration = waitForDialogue ? 0f : 4f,
                waitForInput = waitForDialogue,
                autoAdvance = !waitForDialogue
            };
        }

        cutsceneManager.SetShots(shots);
        Debug.Log($"Created cutscene with {shots.Length} shots using {shotCount} cameras");
    }

    [ContextMenu("Create Cutscene Cameras")]
    public void CreateCutsceneCameras()
    {
        // Create parent object for organization
        GameObject cameraParent = GameObject.Find("Cutscene Cameras");
        if (cameraParent == null)
        {
            cameraParent = new GameObject("Cutscene Cameras");
        }

        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogWarning("Main Camera not found!");
            return;
        }

        // Create 3 example cameras
        cutsceneCameras = new Camera[3];

        for (int i = 0; i < 3; i++)
        {
            GameObject camGO = new GameObject($"Cutscene Camera {i + 1}");
            camGO.transform.SetParent(cameraParent.transform);

            Camera cam = camGO.AddComponent<Camera>();

            // Copy settings from main camera
            cam.CopyFrom(mainCam);

            // Disable immediately and explicitly
            cam.enabled = false;
            camGO.SetActive(false);

            // Set unique starting positions
            Vector3 offset = Vector3.zero;
            switch (i)
            {
                case 0: offset = Vector3.back * 3f + Vector3.up * 1f; break;
                case 1: offset = Vector3.right * 4f + Vector3.up * 2f; break;
                case 2: offset = Vector3.left * 3f + Vector3.forward * 2f + Vector3.up * 1.5f; break;
            }

            cam.transform.position = mainCam.transform.position + offset;
            cam.transform.LookAt(mainCam.transform.position + Vector3.forward * 5f);

            cutsceneCameras[i] = cam;
        }

        Debug.Log($"Created {cutsceneCameras.Length} cutscene cameras (all disabled)");
    }

    [ContextMenu("Create Camera Targets")]
    public void CreateCameraTargets()
    {
        // Create parent object for organization
        GameObject targetsParent = GameObject.Find("Camera Targets");
        if (targetsParent == null)
        {
            targetsParent = new GameObject("Camera Targets");
        }

        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        Vector3 basePos = mainCam.transform.position;

        // Create target positions for each camera
        Vector3[] positions = {
            basePos + Vector3.back * 2f + Vector3.up * 0.5f,
            basePos + Vector3.right * 3f + Vector3.up * 1f + Vector3.forward * 1f,
            basePos + Vector3.left * 2f + Vector3.forward * 3f + Vector3.up * 2f
        };

        Vector3[] rotations = {
            mainCam.transform.eulerAngles + Vector3.up * 10f,
            mainCam.transform.eulerAngles + Vector3.up * -25f,
            mainCam.transform.eulerAngles + Vector3.up * 45f + Vector3.right * -10f
        };

        cameraTargets = new Transform[positions.Length];

        for (int i = 0; i < positions.Length; i++)
        {
            GameObject target = new GameObject($"Camera Target {i + 1}");
            target.transform.SetParent(targetsParent.transform);
            target.transform.position = positions[i];
            target.transform.rotation = Quaternion.Euler(rotations[i]);

            cameraTargets[i] = target.transform;
        }

        Debug.Log($"Created {cameraTargets.Length} camera targets");
    }

    [ContextMenu("Debug Setup")]
    public void DebugSetup()
    {
        Debug.Log($"Cutscene Manager: {cutsceneManager != null}");
        Debug.Log($"Cutscene Cameras: {cutsceneCameras?.Length ?? 0}");
        Debug.Log($"Camera Targets: {cameraTargets?.Length ?? 0}");

        if (cutsceneManager != null)
        {
            cutsceneManager.DebugCameraStates();
        }
    }
}