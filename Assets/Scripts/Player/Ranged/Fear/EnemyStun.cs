using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyStun : MonoBehaviour
{
    public bool IsStunned { get; private set; }
    NavMeshAgent agent;
    Coroutine c;

    void Awake() { agent = GetComponent<NavMeshAgent>(); }

    public void ApplyStun(float seconds)
    {
        if (seconds <= 0f) return;
        if (c != null) StopCoroutine(c);
        c = StartCoroutine(StunCR(seconds));
    }

    IEnumerator StunCR(float t)
    {
        IsStunned = true;
        if (agent) agent.isStopped = true;
        yield return new WaitForSeconds(t);
        if (agent) agent.isStopped = false;
        IsStunned = false;
        c = null;
    }
}
