using UnityEngine;
using Unity.Behavior;
using System;

public class MechBehaviorController : MonoBehaviour
{
    public BehaviorGraphAgent graphAgent;
    public ColliderTrigger colliderTrigger;

    public static event Action<MechBossPhases> OnPhaseChanged;
    private MechBossPhases currentPhase = MechBossPhases.Intro;

    private bool isActive = false;
    private bool isPlayerInRange = false;

    private void Start()
    {
        if (graphAgent == null)
        {
            graphAgent = GetComponent<BehaviorGraphAgent>();
        }

        graphAgent.BlackboardReference.SetVariableValue("isActive", false);
        graphAgent.BlackboardReference.SetVariableValue("isPlayerInRange", false);

        if (colliderTrigger != null)
        {
            colliderTrigger.OnPlayerEnterTrigger += HandlePlayerEnterTrigger;
        }
    }

    private void HandlePlayerEnterTrigger(object sender, EventArgs e)
    {
        Debug.Log("Activating Mech");
        SetActive(true);
        InRange(true);

        Debug.Log("Starting cinematic");
        TransitionToPhase(MechBossPhases.Intro);
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

    private void TransitionToPhase(MechBossPhases newPhase)
    {
        if (currentPhase == newPhase) return;

        currentPhase = newPhase;
        Debug.Log("Boss transitioned to phase: " + currentPhase);
        OnPhaseChanged?.Invoke(currentPhase);
    }
}

public enum MechBossPhases
{
    Intro,
    Phase1,
    Phase2,
    Rage,
    Death
}
