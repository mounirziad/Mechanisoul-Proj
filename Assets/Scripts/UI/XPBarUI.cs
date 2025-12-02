using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class XPBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] XPManager xpManager;
    [SerializeField] Slider xpSlider;
    [SerializeField] TextMeshProUGUI xpText;

    void Awake()
    {
        if (xpManager == null && XPManager.Instance != null)
        {
            xpManager = XPManager.Instance;
        }

        if (xpSlider != null)
        {
            xpSlider.minValue = 0f;
            xpSlider.maxValue = 1f;
            xpSlider.wholeNumbers = false;
            xpSlider.interactable = false;
        }
    }

    void Update()
    {
        if (xpManager == null || xpSlider == null)
        {
            return;
        }

        float percent = xpManager.CurrentXPPercent;
        xpSlider.value = percent;

        if (xpText != null)
        {
            int current = xpManager.CurrentXPInLevel;
            int span = xpManager.CurrentLevelXPSpan;
            xpText.text = current.ToString() + " / " + span.ToString();
        }
    }
}
