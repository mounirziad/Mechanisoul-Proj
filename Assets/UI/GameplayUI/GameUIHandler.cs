using UnityEngine;
using UnityEngine.UIElements;

public class GameUIHandler : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealthScript;
    [SerializeField] private UIDocument UIDoc;
    private Label healthLabel;
    private VisualElement healthBarMask;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerHealthScript.onHealthChanged.AddListener(HealthChanged);
        healthLabel = UIDoc.rootVisualElement.Q<Label>("HealthLabel");
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
        healthLabel.text = $"{playerHealthScript.GetCurrentHealth()} / {playerHealthScript.GetMaxHealth()}";

        float healthRatio = playerHealthScript.GetHealthPercent();
        float healthPercent = Mathf.Lerp(8, 88, healthRatio); // Change 8 and 88 when changing health bar sprites
        healthBarMask.style.width = Length.Percent(healthPercent);
    }
}
