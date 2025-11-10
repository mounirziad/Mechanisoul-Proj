using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealthScript;
    [SerializeField] private RectTransform barRect;
    [SerializeField] private RectMask2D mask;

    private float maxRightMask;
    private float initialRightMask;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    void Awake()
    {
        if (playerHealthScript == null)
        {
            playerHealthScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>();
        }

        playerHealthScript.onHealthChanged.AddListener(SetHealth);
    }
    
    void Start()
    {
        maxRightMask = barRect.rect.width - mask.padding.x - mask.padding.z;
        initialRightMask = mask.padding.z;
    }

    public void SetHealth(float currentHealth, float maxHealth)
    {
        var targetWidth = currentHealth * maxRightMask / maxHealth;
        var newRightMask = maxRightMask + initialRightMask - targetWidth;
        var padding = mask.padding;
        padding.z = newRightMask;
        mask.padding = padding;
    }
}
