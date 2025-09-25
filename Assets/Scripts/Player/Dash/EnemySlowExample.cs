using UnityEngine;

public class EnemySlowExample : MonoBehaviour, ISlowable
{
    float baseSpeed = 5f;
    float slowEndTime;
    float currentSlowMultiplier = 1f;

    public void AddSlow(float percent, float seconds)
    {
        currentSlowMultiplier = Mathf.Min(currentSlowMultiplier, 1f - percent);
        slowEndTime = Mathf.Max(slowEndTime, Time.time + seconds);
    }

    void Update()
    {
        if (Time.time > slowEndTime) currentSlowMultiplier = 1f;
        float speed = baseSpeed * currentSlowMultiplier;
    }
}
