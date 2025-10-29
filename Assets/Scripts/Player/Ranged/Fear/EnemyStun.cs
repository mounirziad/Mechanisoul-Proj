using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyStun : MonoBehaviour
{
    public bool IsStunned { get; private set; }
    NavMeshAgent agent;
    BasicEnemyHealth health;
    Coroutine c;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
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
        IsStunned = false;
    }

    public void ApplyStun(float seconds)
    {
        if (seconds <= 0f) return;
        if (health != null && health.currentHealth <= 0) return;
        if (c != null) StopCoroutine(c);
        c = StartCoroutine(StunCR(seconds));
    }

    IEnumerator StunCR(float t)
    {
        IsStunned = true;
        if (agent && agent.isOnNavMesh && agent.isActiveAndEnabled) agent.isStopped = true;
        yield return new WaitForSeconds(t);
        if (agent && agent.isOnNavMesh && agent.isActiveAndEnabled) agent.isStopped = false;
        IsStunned = false;
        c = null;
    }
}
