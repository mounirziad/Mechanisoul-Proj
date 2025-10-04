using UnityEngine;
using UnityEngine.UI;

public class LockOnLetterbox : MonoBehaviour
{
    [Header("UI References")]
    public GameObject topBar;
    public GameObject bottomBar;

    [Header("Animation Settings")]
    public float transitionSpeed = 5f; // speed of fade/slide

    private RectTransform topRect;
    private RectTransform bottomRect;
    private Vector2 topTarget;
    private Vector2 bottomTarget;
    private bool barsVisible = false;

    private void Awake()
    {
        if (topBar != null) topRect = topBar.GetComponent<RectTransform>();
        if (bottomBar != null) bottomRect = bottomBar.GetComponent<RectTransform>();

        // start hidden
        if (topBar) topBar.SetActive(false);
        if (bottomBar) bottomBar.SetActive(false);
    }

    private void Update()
    {
        if (topRect != null)
            topRect.anchoredPosition = Vector2.Lerp(topRect.anchoredPosition, topTarget, transitionSpeed * Time.deltaTime);

        if (bottomRect != null)
            bottomRect.anchoredPosition = Vector2.Lerp(bottomRect.anchoredPosition, bottomTarget, transitionSpeed * Time.deltaTime);
    }

    public void ShowBars()
    {
        barsVisible = true;
        if (topBar) topBar.SetActive(true);
        if (bottomBar) bottomBar.SetActive(true);

        topTarget = Vector2.zero;
        bottomTarget = Vector2.zero;
    }

    public void HideBars()
    {
        barsVisible = false;

        // slide them off screen
        if (topRect != null) topTarget = new Vector2(0, 200);   // adjust to bar height
        if (bottomRect != null) bottomTarget = new Vector2(0, -200);

        // disable after animation
        Invoke(nameof(DisableBars), 0.3f);
    }

    private void DisableBars()
    {
        if (!barsVisible)
        {
            if (topBar) topBar.SetActive(false);
            if (bottomBar) bottomBar.SetActive(false);
        }
    }
}
