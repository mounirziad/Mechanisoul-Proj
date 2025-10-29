using System.Collections;
using UnityEngine;

public class EnemyCharm : MonoBehaviour
{
    public bool IsCharmed { get; private set; }
    public Transform CharmFocus { get; private set; }

    BasicEnemyHealth health;
    Coroutine c;

    void Awake()
    {
        health = GetComponent<BasicEnemyHealth>();
        if (health != null)
        {
            health.OnDeath += HandleDeath;
        }
    }

    void OnDestroy()
    {
        if (health != null)
        {
            health.OnDeath -= HandleDeath;
        }
    }

    void HandleDeath()
    {
        if (c != null)
        {
            StopCoroutine(c);
            c = null;
        }
        IsCharmed = false;
        CharmFocus = null;
    }

    public void ApplyCharm(float seconds, Transform focus = null)
    {
        if (seconds <= 0f) return;
        if (health != null && health.currentHealth <= 0) return;
        CharmFocus = focus;
        if (c != null) StopCoroutine(c);
        c = StartCoroutine(CharmCR(seconds));
    }

    IEnumerator CharmCR(float t)
    {
        IsCharmed = true;
        yield return new WaitForSeconds(t);
        IsCharmed = false;
        CharmFocus = null;
        c = null;
    }
}
