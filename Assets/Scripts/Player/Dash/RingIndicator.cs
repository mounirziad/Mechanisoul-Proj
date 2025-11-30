using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RingIndicator : MonoBehaviour
{
    [SerializeField] int segments = 64;
    [SerializeField] float width = 0.06f;
    [SerializeField] AnimationCurve alphaOverLife = AnimationCurve.EaseInOut(0, 1, 1, 0);
    [SerializeField] float yOffset = 0.02f;

    LineRenderer lr;
    float life;
    float t;
    Color baseColor;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.loop = true;
        lr.useWorldSpace = true;
        lr.positionCount = segments;
        lr.startWidth = width;
        lr.endWidth = width;
        var mat = lr.material;
        if (!mat)
        {
            // simple default material
            lr.material = new Material(Shader.Find("Sprites/Default"));
        }
    }

    public void Spawn(float radius, Color color, float lifetime)
    {
        baseColor = color;
        life = Mathf.Max(0.01f, lifetime);
        t = 0f;

        Vector3 c = transform.position;
        c.y += yOffset;
        float step = Mathf.PI * 2f / segments;
        for (int i = 0; i < segments; i++)
        {
            float a = step * i;
            Vector3 p = new Vector3(Mathf.Cos(a) * radius, 0f, Mathf.Sin(a) * radius);
            lr.SetPosition(i, c + p);
        }
        SetColor(1f);
    }

    void Update()
    {
        if (life <= 0f) return;
        t += Time.deltaTime;
        float k = Mathf.Clamp01(t / life);
        SetColor(alphaOverLife.Evaluate(k));
        if (k >= 1f) Destroy(gameObject);
    }

    void SetColor(float alphaMul)
    {
        Color c = baseColor; c.a *= alphaMul;
        lr.startColor = c;
        lr.endColor = c;
    }
}
