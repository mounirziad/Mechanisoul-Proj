using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageFlashEffect : MonoBehaviour
{
    [Header("Flash Settings")]
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private float flashIntensity = 2f;
    [SerializeField] private AnimationCurve flashCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private List<Material> originalMaterials = new List<Material>();
    private List<Renderer> renderers = new List<Renderer>();
    private Coroutine flashCoroutine;

    private static readonly int EmissionColorProperty = Shader.PropertyToID("_EmissionColor");
    private static readonly int BaseColorProperty = Shader.PropertyToID("_BaseColor");
    private static readonly int Emissive_ColorProperty = Shader.PropertyToID("_Emissive_Color");
    private static readonly int BaseMapProperty = Shader.PropertyToID("_BaseMap");
    private static readonly int ColorProperty = Shader.PropertyToID("_Color");

    private void Awake()
    {
        GetAllRenderers();
        StoreOriginalMaterials();
    }

    private void GetAllRenderers()
    {
        renderers.AddRange(GetComponentsInChildren<SkinnedMeshRenderer>());
        renderers.AddRange(GetComponentsInChildren<MeshRenderer>());
    }

    private void StoreOriginalMaterials()
    {
        foreach (var renderer in renderers)
        {
            if (renderer != null)
            {
                foreach (var mat in renderer.materials)
                {
                    originalMaterials.Add(new Material(mat));
                }
            }
        }
    }

    public void Flash()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        flashCoroutine = StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < flashDuration)
        {
            elapsedTime += Time.deltaTime;
            float lerpAmount = flashCurve.Evaluate(elapsedTime / flashDuration);
            
            int materialIndex = 0;
            foreach (var renderer in renderers)
            {
                if (renderer != null)
                {
                    foreach (var mat in renderer.materials)
                    {
                        if (materialIndex < originalMaterials.Count)
                        {
                            Color originalColor = Color.white;
                            Color flashValue = flashColor * flashIntensity * lerpAmount;
                            
                            if (mat.HasProperty(BaseColorProperty))
                            {
                                originalColor = originalMaterials[materialIndex].GetColor(BaseColorProperty);
                                mat.SetColor(BaseColorProperty, Color.Lerp(originalColor, flashValue, lerpAmount));
                            }
                            else if (mat.HasProperty(ColorProperty))
                            {
                                originalColor = originalMaterials[materialIndex].GetColor(ColorProperty);
                                mat.SetColor(ColorProperty, Color.Lerp(originalColor, flashValue, lerpAmount));
                            }
                            
                            if (mat.HasProperty(EmissionColorProperty))
                            {
                                mat.SetColor(EmissionColorProperty, flashValue);
                            }
                            if (mat.HasProperty(Emissive_ColorProperty))
                            {
                                mat.SetColor(Emissive_ColorProperty, flashValue);
                            }
                        }
                        materialIndex++;
                    }
                }
            }

            yield return null;
        }

        RestoreOriginalMaterials();
        flashCoroutine = null;
    }

    private void RestoreOriginalMaterials()
    {
        int materialIndex = 0;
        foreach (var renderer in renderers)
        {
            if (renderer != null)
            {
                Material[] mats = renderer.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    if (materialIndex < originalMaterials.Count)
                    {
                        if (mats[i].HasProperty(BaseColorProperty))
                        {
                            Color originalColor = originalMaterials[materialIndex].GetColor(BaseColorProperty);
                            mats[i].SetColor(BaseColorProperty, originalColor);
                        }
                        else if (mats[i].HasProperty(ColorProperty))
                        {
                            Color originalColor = originalMaterials[materialIndex].GetColor(ColorProperty);
                            mats[i].SetColor(ColorProperty, originalColor);
                        }
                        
                        if (mats[i].HasProperty(EmissionColorProperty))
                        {
                            Color originalEmission = originalMaterials[materialIndex].GetColor(EmissionColorProperty);
                            mats[i].SetColor(EmissionColorProperty, originalEmission);
                        }
                        if (mats[i].HasProperty(Emissive_ColorProperty))
                        {
                            Color originalEmissive = originalMaterials[materialIndex].GetColor(Emissive_ColorProperty);
                            mats[i].SetColor(Emissive_ColorProperty, originalEmissive);
                        }
                        materialIndex++;
                    }
                }
            }
        }
    }

    private void OnDestroy()
    {
        foreach (var mat in originalMaterials)
        {
            if (mat != null)
            {
                Destroy(mat);
            }
        }
        originalMaterials.Clear();
    }
}
