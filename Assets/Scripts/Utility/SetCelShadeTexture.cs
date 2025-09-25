using UnityEngine;

public class SetCelShadeTexture : MonoBehaviour
{
    [Header("Material")]
    private Renderer objectRenderer;
    private Material objectMaterial;
    [SerializeField] Texture2D objectTexture;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        objectMaterial = objectRenderer.material;

        if (objectTexture != null)
        {
            objectMaterial.SetTexture("_Texture2D", objectTexture);
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        objectRenderer = GetComponent<Renderer>();
        objectMaterial = objectRenderer.sharedMaterial;

        if (objectTexture != null)
        {
            objectMaterial.SetTexture("_Texture2D", objectTexture);
        }
    }
#endif
}
