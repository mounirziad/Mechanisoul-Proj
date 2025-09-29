using UnityEngine;
using UnityEngine.UIElements;

public class GameUIHandler : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealthScript;
    [SerializeField] private UIDocument UIDoc;
    private VisualElement healthBarMask;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerHealthScript.onHealthChanged.AddListener(HealthChanged);
        healthBarMask = UIDoc.rootVisualElement.Q<VisualElement>("HealthBarMask");

        HealthChanged(playerHealthScript.GetCurrentHealth(), playerHealthScript.GetMaxHealth());
    }

    void OnDestroy()
    {
        if (playerHealthScript != null)
        {
            playerHealthScript.onHealthChanged.RemoveListener(HealthChanged);
        }
    }

    void HealthChanged(float currentHealth, float maxHealth)
    {
        float healthRatio = playerHealthScript.GetHealthPercent();
        float healthPercent = Mathf.Lerp(27, 95, healthRatio); // Change 8 and 88 when changing health bar sprites
        healthBarMask.style.width = Length.Percent(healthPercent);
    }
}
