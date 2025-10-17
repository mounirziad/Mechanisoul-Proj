using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class HitscanTracer : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private float lifeTime = 0.1f;
    private float fadeSpeed = 5f;
    
    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        Destroy(gameObject, lifeTime);
    }
    
    void Update()
    {
        if (lineRenderer != null)
        {
            Color startColor = lineRenderer.startColor;
            Color endColor = lineRenderer.endColor;
            
            startColor.a = Mathf.Lerp(startColor.a, 0, Time.deltaTime * fadeSpeed);
            endColor.a = Mathf.Lerp(endColor.a, 0, Time.deltaTime * fadeSpeed);
            
            lineRenderer.startColor = startColor;
            lineRenderer.endColor = endColor;
        }
    }
    
    public void SetupTracer(Vector3 start, Vector3 end, float duration = 0.1f, float fade = 5f)
    {
        lifeTime = duration;
        fadeSpeed = fade;
        
        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);
        }
    }
}
