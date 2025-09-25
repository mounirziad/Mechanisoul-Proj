using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[DisallowMultipleComponent]
public class AoeRing : MonoBehaviour
{
    [Header("Shape")]
    public float radius = 1.5f;
    [Range(8, 256)] public int segments = 64;
    public float yOffset = 0.05f;

    [Header("Style")]
    public float lineWidth = 0.05f;
    public Color color = Color.red;
    public bool pulse = true;
    [Range(0f, 0.5f)] public float pulseAmplitude = 0.05f; // 5% radius
    public float pulseSpeed = 2f;
    public float rotateSpeed = 0f; // degrees/sec

    LineRenderer lr;
    float baseRadius;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.loop = true;
        lr.alignment = LineAlignment.View;
        lr.widthMultiplier = 1f;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.positionCount = segments;
        baseRadius = radius;
        ApplyColor(color);
        Rebuild();
    }

    void OnValidate()
    {
        if (segments < 8) segments = 8;
        if (!lr) lr = GetComponent<LineRenderer>();
        if (lr) { lr.positionCount = segments; lr.startWidth = lineWidth; lr.endWidth = lineWidth; }
        baseRadius = radius;
        Rebuild();
        ApplyColor(color);
    }

    void Update()
    {
        float r = baseRadius;
        if (pulse)
        {
            r *= 1f + Mathf.Sin(Time.time * Mathf.PI * 2f * pulseSpeed) * pulseAmplitude;
        }
        if (!Mathf.Approximately(r, radius))
        {
            radius = r;
            Rebuild();
        }
        if (!Mathf.Approximately(rotateSpeed, 0f))
        {
            transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime, Space.World);
        }
    }

    public void SetRadius(float r)
    {
        baseRadius = r;
        radius = r;
        Rebuild();
    }

    public void ApplyColor(Color c)
    {
        color = c;
        if (!lr) lr = GetComponent<LineRenderer>();
        if (lr)
        {
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
                new[] { new GradientAlphaKey(color.a, 0f), new GradientAlphaKey(color.a, 1f) }
            );
            lr.colorGradient = grad;
        }
    }

    void Rebuild()
    {
        if (!lr) lr = GetComponent<LineRenderer>();
        if (!lr) return;

        lr.positionCount = segments;
        float step = Mathf.PI * 2f / segments;
        Vector3 center = new Vector3(transform.position.x, transform.position.y + yOffset, transform.position.z);

        for (int i = 0; i < segments; i++)
        {
            float a = step * i;
            float x = Mathf.Cos(a) * radius;
            float z = Mathf.Sin(a) * radius;
            lr.SetPosition(i, center + new Vector3(x, 0f, z));
        }
    }
}
