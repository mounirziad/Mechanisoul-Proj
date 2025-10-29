using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemySlowStacks : MonoBehaviour
{
    [Range(0f, 1f)] public float minSpeedMultiplier = 0.2f;
    [SerializeField] float stackDecayDelay = 2.0f;
    [SerializeField] float checkInterval = 0.1f;

    int stacks;
    float lastAppliedTime;
    float perStack;
    int cap;

    NavMeshAgent agent;
    float baseSpeed;
    Coroutine loop;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (agent) baseSpeed = agent.speed;
    }

    public void ApplyStackingSlow(float slowPerStack, int maxStacks)
    {
        perStack = Mathf.Max(0f, slowPerStack);
        cap = Mathf.Max(1, maxStacks);
        stacks = Mathf.Clamp(stacks + 1, 1, cap);
        lastAppliedTime = Time.time;
        if (loop == null) loop = StartCoroutine(SlowLoop());
        ApplyNow();
    }

    IEnumerator SlowLoop()
    {
        while (stacks > 0)
        {
            if (Time.time - lastAppliedTime >= stackDecayDelay)
            {
                stacks = Mathf.Max(0, stacks - 1);
                lastAppliedTime = Time.time;
                ApplyNow();
            }
            yield return new WaitForSeconds(checkInterval);
        }
        ApplyNow();
        loop = null;
    }

    void ApplyNow()
    {
        float mult = Mathf.Clamp01(1f - stacks * perStack);
        mult = Mathf.Max(minSpeedMultiplier, mult);
        if (agent) agent.speed = baseSpeed * mult;
        CurrentSpeedMultiplier = mult;
    }

    public float CurrentSpeedMultiplier { get; private set; } = 1f;
    public int CurrentStacks => stacks;
}
