using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ComicCutsceneController : MonoBehaviour
{
    [System.Serializable]
    public class CutsceneShot
    {
        public string shotName;
        public Camera camera;
        public Transform targetPosition;
        public float panDuration = 4f;
        public string dialogueText;
        public bool waitForInput = false;
        public float displayDuration = 3f;
        public float dialogueFadeDuration = 0.5f;
        public bool fadeInAtStart = false;
        public bool fadeOutAtEnd = false;
        public float screenFadeDuration = 1.5f;
    }

    [Header("Cutscene Shots")]
    public CutsceneShot[] shots;

    [Header("UI References")]
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI shadowText;
    public CanvasGroup dialogueCanvasGroup;
    public CanvasGroup fadeCanvasGroup;
    public GameObject continueIndicator;

    [Header("Settings")]
    public float typewriterSpeed = 0.05f;
    public string nextSceneName = "TutorialCutscene";
    public float delayBeforeNextScene = 2f;

    private bool waitingForInput = false;
    private bool inputPressed = false;

    void Start()
    {
        dialogueCanvasGroup.alpha = 0f;
        ClearDialogueText();
        
        StartCoroutine(PlayCutscene());
    }

    IEnumerator PlayCutscene()
    {
        for (int i = 0; i < shots.Length; i++)
        {
            bool isLastShot = (i == shots.Length - 1);
            bool nextShotExists = (i + 1 < shots.Length);
            
            yield return StartCoroutine(PlayShot(shots[i], nextShotExists, isLastShot));
        }

        yield return new WaitForSeconds(delayBeforeNextScene);
        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator PlayShot(CutsceneShot shot, bool hasNextShot, bool isLastShot)
    {
        // Enable the camera
        DisableAllCameras();
        if (shot.camera != null)
        {
            shot.camera.enabled = true;
        }

        // Fade in from black at the start if specified
        if (shot.fadeInAtStart && fadeCanvasGroup.alpha > 0.5f)
        {
            yield return StartCoroutine(FadeScreen(1f, 0f, shot.screenFadeDuration));
        }

        // Start camera pan and dialogue simultaneously
        Coroutine panCoroutine = null;
        Coroutine dialogueCoroutine = null;

        if (shot.targetPosition != null)
        {
            panCoroutine = StartCoroutine(PanCamera(shot.camera, shot.targetPosition.position, shot.panDuration));
        }

        if (!string.IsNullOrEmpty(shot.dialogueText))
        {
            dialogueCoroutine = StartCoroutine(ShowDialogue(shot.dialogueText, shot.waitForInput, shot.displayDuration, shot.dialogueFadeDuration));
        }

        // Wait for both to complete
        if (panCoroutine != null) yield return panCoroutine;
        if (dialogueCoroutine != null) yield return dialogueCoroutine;

        // Fade out to black at the end if specified
        if (shot.fadeOutAtEnd)
        {
            yield return StartCoroutine(FadeScreen(0f, 1f, shot.screenFadeDuration));
        }
    }

    IEnumerator PanCamera(Camera cam, Vector3 targetPos, float duration)
    {
        Vector3 startPos = cam.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            float smoothT = t * t * (3f - 2f * t);
            
            cam.transform.position = Vector3.Lerp(startPos, targetPos, smoothT);
            yield return null;
        }

        cam.transform.position = targetPos;
    }

    IEnumerator ShowDialogue(string text, bool waitForInput, float displayDuration, float fadeDuration)
    {
        yield return StartCoroutine(FadeDialogue(0f, 1f, fadeDuration));
        
        yield return StartCoroutine(TypewriterEffect(text));

        if (waitForInput)
        {
            ShowContinueIndicator();
            waitingForInput = true;
            inputPressed = false;
            
            yield return new WaitUntil(() => inputPressed);
            
            waitingForInput = false;
            HideContinueIndicator();
        }
        else
        {
            yield return new WaitForSeconds(displayDuration);
        }

        yield return StartCoroutine(FadeDialogue(1f, 0f, fadeDuration));
        
        ClearDialogueText();
    }

    IEnumerator TypewriterEffect(string text)
    {
        dialogueText.text = text;
        shadowText.text = text;
        dialogueText.maxVisibleCharacters = 0;
        shadowText.maxVisibleCharacters = 0;

        int totalChars = text.Length;

        for (int i = 0; i <= totalChars; i++)
        {
            dialogueText.maxVisibleCharacters = i;
            shadowText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(typewriterSpeed);
        }
    }

    IEnumerator FadeDialogue(float fromAlpha, float toAlpha, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            float smoothT = t * t * (3f - 2f * t);
            
            dialogueCanvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, smoothT);
            yield return null;
        }

        dialogueCanvasGroup.alpha = toAlpha;
    }

    IEnumerator FadeScreen(float fromAlpha, float toAlpha, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            fadeCanvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);
            yield return null;
        }

        fadeCanvasGroup.alpha = toAlpha;
    }

    void DisableAllCameras()
    {
        foreach (var shot in shots)
        {
            if (shot.camera != null)
            {
                shot.camera.enabled = false;
            }
        }
    }

    void ClearDialogueText()
    {
        dialogueText.text = "";
        shadowText.text = "";
        dialogueText.maxVisibleCharacters = 0;
        shadowText.maxVisibleCharacters = 0;
    }

    void ShowContinueIndicator()
    {
        if (continueIndicator != null)
            continueIndicator.SetActive(true);
    }

    void HideContinueIndicator()
    {
        if (continueIndicator != null)
            continueIndicator.SetActive(false);
    }
}
