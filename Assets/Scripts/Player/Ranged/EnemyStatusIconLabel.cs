using UnityEngine;
using UnityEngine.UI;

public class EnemyStatusIconLabel : MonoBehaviour
{
    [Header("Icon Slots (in order left-to-right)")]
    public Image[] iconSlots;

    [Header("Effect Sprites")]
    public Sprite fearSprite;
    public Sprite loveSprite;
    public Sprite sadnessSprite;

    [Header("Behavior")]
    public bool hideWhenEmpty = true;

    Camera cam;

    void Awake()
    {
        cam = Camera.main;
        ClearIcons();
    }

    void LateUpdate()
    {
        if (cam == null)
        {
            cam = Camera.main;
            if (cam == null) return;
        }

        transform.forward = cam.transform.forward;
    }

    public void ShowEffects(System.Collections.Generic.IReadOnlyList<StatusEffectType> effects)
    {
        ClearIcons();

        if (effects == null || effects.Count == 0)
        {
            if (hideWhenEmpty)
                SetRootActive(false);
            return;
        }

        SetRootActive(true);

        int max = Mathf.Min(effects.Count, iconSlots.Length);
        for (int i = 0; i < max; i++)
        {
            var slot = iconSlots[i];
            if (slot == null) continue;

            Sprite sprite = GetSpriteForEffect(effects[i]);
            if (sprite == null) continue;

            slot.sprite = sprite;
            slot.enabled = true;
        }
    }

    void ClearIcons()
    {
        if (iconSlots == null) return;

        for (int i = 0; i < iconSlots.Length; i++)
        {
            var slot = iconSlots[i];
            if (slot == null) continue;

            slot.sprite = null;
            slot.enabled = false;
        }
    }

    Sprite GetSpriteForEffect(StatusEffectType type)
    {
        switch (type)
        {
            case StatusEffectType.Fear: return fearSprite;
            case StatusEffectType.Love: return loveSprite;
            case StatusEffectType.Sadness: return sadnessSprite;
            default: return null;
        }
    }

    void SetRootActive(bool active)
    {
        gameObject.SetActive(active);
    }
}
