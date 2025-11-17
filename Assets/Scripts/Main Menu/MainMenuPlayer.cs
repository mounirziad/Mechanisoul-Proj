using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
public class MainMenuPlayer : MonoBehaviour
{
    [SerializeField] private GameObject playerModel;
    private Material playerMaterial;
    private Color emissionMapColor;

    [Header("Turn On/Off Settings")]
    [SerializeField] private float flickerDuration = 0.5f;
    [SerializeField] private float turnOnDuration = 0.5f;
    [SerializeField] private float turnOffDuration = 0.5f;
    void Start()
    {
        playerMaterial = playerModel.GetComponent<Renderer>().material;
        emissionMapColor = playerMaterial.GetColor("_Emissive_Color");
        playerMaterial.SetColor("_Emissive_Color", emissionMapColor * 0);
    }

    public IEnumerator TurnOn()
    {
        float elapsed = 0f;
        float flickerRate = .05f;
        float nextFlickerTime = 0f;
        while (elapsed < flickerDuration)
        {
            elapsed += Time.deltaTime;

            if (elapsed >= nextFlickerTime)
            {
                playerMaterial.SetColor("_Emissive_Color", emissionMapColor * Random.Range(0.5f, 1.5f));
                nextFlickerTime = elapsed + flickerRate;
            }

            yield return null;
        }

        elapsed = 0f;
        while (elapsed < turnOnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / turnOnDuration;
            float intensity = Mathf.Lerp(0, 1, t);
            playerMaterial.SetColor("_Emissive_Color", emissionMapColor * intensity);
            yield return null;
        }

        playerMaterial.SetColor("_Emissive_Color", emissionMapColor);
    }

    public IEnumerator TurnOff()
    {
        float elapsed = 0f;

        while (elapsed < turnOffDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / turnOffDuration;
            float intensity = Mathf.Lerp(1, 0, t);
            playerMaterial.SetColor("_Emissive_Color", emissionMapColor * intensity);
            yield return null;
        }

        playerMaterial.SetColor("_Emissive_Color", emissionMapColor * 0);
    }

    public void StopCoroutines()
    {
        StopAllCoroutines();
    }
}
