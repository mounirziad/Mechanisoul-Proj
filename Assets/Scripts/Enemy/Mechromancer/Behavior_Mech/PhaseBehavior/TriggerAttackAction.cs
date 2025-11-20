using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Trigger Attack", story: "[Agent] attacks with [AttackTrigger]", category: "Action", id: "7a6b38fc4553bd031472f1a7fbc608dd")]
public partial class TriggerAttackAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    [CreateProperty]
    [SerializeField]
    public BlackboardVariable<string> AttackTrigger;

    private Mechromancer mech;

    protected override Status OnStart()
    {
        if (Agent == null || Agent.Value == null)
        {
            return Status.Failure;
        }

        mech = Agent.Value.GetComponent<Mechromancer>();
        if (mech == null)
        {
            return Status.Failure;
        }

        mech.TriggerAttack(AttackTrigger.Value);

        return Status.Success;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Trigger Lightning",
    story: "[Agent] casts Lightning",
    category: "Action",
    id: "lightning-action-node"
)]
public partial class TriggerLightningAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    [SerializeField] public float AoERadius = 3f;
    [SerializeField] public float Damage = 15f;

    private LightningController controller;

    protected override Status OnStart()
    {
        if (Agent?.Value == null) return Status.Failure;

        controller = Agent.Value.GetComponent<LightningController>();
        if (controller == null) return Status.Failure;

        var blackboard = Agent.Value.GetComponent<BehaviorGraphAgent>().BlackboardReference;
        blackboard.SetVariableValue("LightningFinished", false);

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return Status.Failure;

        controller.CastLightningAtGround(player.transform.position, AoERadius, Damage);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        //Completes immediately, AoE is handled separately
        bool finished = false;
        Agent.Value.GetComponent<BehaviorGraphAgent>().BlackboardReference.GetVariableValue("LightningFinished", out finished);
        return finished ? Status.Success : Status.Running;
    }

    protected override void OnEnd()
    {
        //Optional if I need to fix anything after attack
    }
}

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Trigger Resurrection",
    story: "[Agent] resurrects minions",
    category: "Action",
    id: "resurrect-action-node"
)]
public partial class TriggerResurrectionAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    private Resurrection resurrection;

    protected override Status OnStart()
    {
        Debug.Log("TriggerResurrectionAction: OnStart()");

        if (Agent?.Value == null)
        {
            Debug.LogError("TriggerResurrectionAction: Agent is NULL");
            return Status.Failure;
        }

        resurrection = Agent.Value.GetComponent<Resurrection>();
        if (resurrection == null)
        {
            Debug.LogError("TriggerResurrectionAction: No Resurrection component found on Agent");
            return Status.Failure;
        }

        Debug.Log("TriggerResurrectionAction: HasResurrected = " + resurrection.HasResurrected);

        if (resurrection.HasResurrected)
        {
            Debug.LogWarning("TriggerResurrectionAction: Already resurrected is FAILING");
            return Status.Failure;
        }

        Debug.Log("TriggerResurrectionAction: Calling StartResurrection()");
        resurrection.StartResurrection();
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return resurrection.IsResurrectionActive ? Status.Running : Status.Success;
    }
}

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Trigger Lunge",
    story: "[Agent] lunges",
    category: "Action",
    id: "lunge-action-node"
)]
public partial class TriggerLungeAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
}