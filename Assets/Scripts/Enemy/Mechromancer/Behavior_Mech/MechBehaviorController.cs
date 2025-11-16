using UnityEngine;
using Unity.Behavior;
using System;
using System.Collections;

public class MechBehaviorController : MonoBehaviour
{
    public BehaviorGraphAgent graphAgent;
    public ColliderTrigger colliderTrigger;

    public EventChannel phase1Channel;
    public EventChannel phase2Channel;
    public EventChannel rageChannel;
    public EventChannel deathChannel;

    private bool isActive = false;
    private bool isPlayerInRange = false;

    private void Start()
    {
        StartCoroutine(InitializeChannels());
    }

    private IEnumerator InitializeChannels()
    {
        yield return null;

        if (graphAgent == null)
        {
            graphAgent = GetComponent<BehaviorGraphAgent>();
        }

        Debug.Log($"Channels: {phase1Channel}, {phase2Channel}, {rageChannel}, {deathChannel}");

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

    public void TriggerPhase(string phaseName)
    {
        Debug.Log($"Mech entering {phaseName}");

        switch (phaseName)
        {
            case "Phase1":
                phase1Channel.SendEventMessage();
                break;

            case "Phase2":
                phase2Channel.SendEventMessage();
                break;

            case "Rage":
                rageChannel.SendEventMessage();
                break;

            case "Death":
                deathChannel.SendEventMessage();
                break;
        }
    }
}
