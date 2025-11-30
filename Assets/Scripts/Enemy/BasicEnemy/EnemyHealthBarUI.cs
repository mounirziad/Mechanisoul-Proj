using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBarUI : MonoBehaviour
{
    public BasicEnemyHealth health;
    public Image fillImage;
    public bool hideWhenDead = true;

    void Awake()
    {
        if (!health)
            health = GetComponentInParent<BasicEnemyHealth>();
    }

    void OnEnable()
    {
        if (health != null)
            health.OnDeath += HandleDeath;
    }

    void OnDisable()
    {
        if (health != null)
            health.OnDeath -= HandleDeath;
    }

    void Update()
    {
        if (!health || !fillImage) return;

        float t = Mathf.Clamp01(health.HealthFraction);
        fillImage.fillAmount = t;
    }

    void HandleDeath()
    {
        if (hideWhenDead)
            gameObject.SetActive(false);
    }
}
