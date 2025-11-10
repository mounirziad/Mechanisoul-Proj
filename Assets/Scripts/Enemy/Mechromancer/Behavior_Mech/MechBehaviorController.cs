using UnityEngine;
using Unity.Behavior;

public class MechBehaviorController : MonoBehaviour
{
    public BehaviorGraphAgent graphAgent;
    public ColliderTrigger colliderTrigger;

    private bool isActive = false;
    private bool isPlayerInRange = false;

    private void Awake()
    {
        if (graphAgent == null)
        {
            graphAgent = GetComponent<BehaviorGraphAgent>();
        }
    }

    private void Update()
    {
        if (colliderTrigger != null && colliderTrigger.hasTriggered)
        {
            SetActive(true);
            InRange(true);
        }
    }

    public void SetActive(bool active)
    {
        isActive = active;
        graphAgent.BlackboardReference.SetVariableValue("isActive", active);
    }

    public void InRange(bool inRange)
    {
        isPlayerInRange = inRange;
        graphAgent.BlackboardReference.SetVariableValue("isPlayerInRange", inRange);
    }
}
