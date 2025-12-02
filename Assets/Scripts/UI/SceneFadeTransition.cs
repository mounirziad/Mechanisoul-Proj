using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFadeTransition : MonoBehaviour
{
    private static SceneFadeTransition instance;

    [SerializeField] private FadingScript fadingScript;
    [SerializeField] private float fadeOutDuration = 2f;
    [SerializeField] private float delayBeforeLoad = 0.5f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (fadingScript == null)
        {
            fadingScript = GetComponent<FadingScript>();
        }

        if (fadingScript == null)
        {
            Debug.LogError("SceneFadeTransition: FadingScript component not found!");
        }
    }

    public static void TransitionToScene(string sceneName, float fadeDuration = 2f, float delay = 0.5f)
    {
        if (instance != null)
        {
            instance.StartTransition(sceneName, fadeDuration, delay);
        }
        else
        {
            Debug.LogError("SceneFadeTransition: No instance found! Loading scene directly.");
            SceneManager.LoadScene(sceneName);
        }
    }

    public void StartTransition(string sceneName, float fadeDuration, float delay)
    {
        StartCoroutine(TransitionCoroutine(sceneName, fadeDuration, delay));
    }

    private IEnumerator TransitionCoroutine(string sceneName, float fadeDuration, float delay)
    {
        if (fadingScript != null)
        {
            fadingScript.FadeOut(fadeDuration);
            yield return new WaitForSeconds(fadeDuration);
        }

        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }

        Debug.Log($"SceneFadeTransition: Loading scene '{sceneName}'");
        SceneManager.LoadScene(sceneName);
    }
}
