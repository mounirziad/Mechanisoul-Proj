using System.Collections.Generic;
using UnityEngine;

public class DoorOutline : MonoBehaviour
{
    [SerializeField] private Material outlineMaterial;
    [SerializeField] private Color outlineColor = Color.green;
    [SerializeField] private float outlineThickness = 0.02f;
    [SerializeField] private List<MeshRenderer> doorRenderers = new List<MeshRenderer>();
    private List<Material[]> originalMaterials = new List<Material[]>();
    private bool outlineEnabled;
    
    void Awake()
    {
        SaveOriginalMaterials();
    }

    private void SaveOriginalMaterials()
    {
        originalMaterials.Clear();
        foreach (MeshRenderer renderer in doorRenderers)
        {
            if (renderer != null)
            {
                originalMaterials.Add(renderer.sharedMaterials);
            }
        }
    }

    public void EnableOutline()
    {
        if (outlineEnabled || outlineMaterial == null) return;

        for (int i = 0; i < doorRenderers.Count; i++)
        {
            if (doorRenderers[i] == null) continue;

            Material[] currentMaterials = doorRenderers[i].sharedMaterials;
            Material[] newMaterials = new Material[currentMaterials.Length + 1];
            
            for (int j = 0; j < currentMaterials.Length; j++)
            {
                newMaterials[j] = currentMaterials[j];
            }

            Material outlineInstance = new Material(outlineMaterial);
            outlineInstance.SetColor("_Outline_Color", outlineColor);
            outlineInstance.SetFloat("_Outline_Thickness", outlineThickness);
            
            newMaterials[currentMaterials.Length] = outlineInstance;
            doorRenderers[i].materials = newMaterials;
        }

        outlineEnabled = true;
    }

    public void DisableOutline()
    {
        if (!outlineEnabled) return;

        for (int i = 0; i < doorRenderers.Count; i++)
        {
            if (doorRenderers[i] != null && i < originalMaterials.Count)
            {
                doorRenderers[i].materials = originalMaterials[i];
            }
        }

        outlineEnabled = false;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DisableOutline();
        }
    }
}
