using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class DashVFXBehavior : MonoBehaviour
{
    [SerializeField] private ParticleSystem ripples_PS;
    [SerializeField] private ParticleSystem ripples_PS2;
    [SerializeField] private ParticleSystem sparks_PS;
    [SerializeField] private ParticleSystem trails_PS;

    public void Spawn(float lifetime)
    {
        // Start all fade coroutines
        StartCoroutine(VFXFade(lifetime, ripples_PS));
        StartCoroutine(VFXFade(lifetime, ripples_PS2));
        StartCoroutine(VFXFade(lifetime, sparks_PS));
        StartCoroutine(VFXFade(lifetime, trails_PS));
        Destroy(gameObject, lifetime + 1f);

        Debug.Log("VFX START");
    }

    private IEnumerator VFXFade(float life, ParticleSystem particleSystem)
    {
        if (particleSystem == null) yield break;

        float currentTime = 0f;

        // Get the initial color
        var mainModule = particleSystem.main;
        Color initialColor = mainModule.startColor.color;

        while (currentTime < life)
        {
            currentTime += Time.deltaTime;
            float alpha = Mathf.Lerp(initialColor.a, 0f, currentTime / life);

            // Create new color with updated alpha
            Color newColor = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);

            // Apply the new color
            mainModule.startColor = newColor;
            Debug.Log("Update");
            yield return null;
        }

        // Ensure alpha is 0 at the end
        Color finalColor = new Color(initialColor.r, initialColor.g, initialColor.b, 0f);
        mainModule.startColor = finalColor;
    }
}
