using System.Collections;
using UnityEngine;

public class EnemyCharm : MonoBehaviour
{
    public bool IsCharmed { get; private set; }
    public Transform CharmFocus { get; private set; }

    Coroutine c;

    public void ApplyCharm(float seconds, Transform focus = null)
    {
        if (seconds <= 0f) return;
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
