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

    [SerializeField]
    [CreateProperty]
    public string AttackId;

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
public partial class TriggerLightningAction : Action, IAttackCondition
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    [SerializeField] public float AoERadius = 3f;
    [SerializeField] public float Damage = 15f;

    [SerializeField]
    [CreateProperty]
    public string AttackId;

    private LightningController controller;
    private BehaviorGraphAgent bgAgent;

    public bool CanRun()
    {
        if (Agent?.Value == null) return false;

        bgAgent = Agent.Value.GetComponent<BehaviorGraphAgent>();

        bool finished = true;
        bgAgent.BlackboardReference.GetVariableValue("LightningFinished", out finished);

        return finished;
    }

    protected override Status OnStart()
    {
        if (Agent?.Value == null) return Status.Failure;

        controller = Agent.Value.GetComponent<LightningController>();
        bgAgent = Agent.Value.GetComponent<BehaviorGraphAgent>();

        bgAgent.BlackboardReference.SetVariableValue("LightningFinished", false);

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return Status.Failure;

        controller.CastLightningAtGround(player.transform.position, AoERadius, Damage);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        //Completes immediately, AoE is handled separately
        bool finished = false;
        bgAgent.BlackboardReference.GetVariableValue("LightningFinished", out finished);

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
public partial class TriggerResurrectionAction : Action, IAttackCondition
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    private Resurrection resurrection;

    public bool CanRun()
    {
        if (Agent?.Value == null) return false;

        resurrection = Agent.Value.GetComponent<Resurrection>();
        if (resurrection == null) return false;

        return !resurrection.HasResurrected;
    }

    protected override Status OnStart()
    {
        if (Agent?.Value == null)
        {
            Debug.LogError("Agent or Agent.Value is null");
            return Status.Failure;
        }

        resurrection = Agent.Value.GetComponent<Resurrection>();
        if (resurrection == null)
        {
            Debug.LogError("Resurrection component not found on Agent");
            return Status.Failure;
        }

        resurrection.StartResurrection();
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (resurrection == null)
        {
            Debug.LogError("Resurrection component not found");
            return Status.Failure;
        }
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
public partial class TriggerLungeAction : Action, IAttackCondition
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    [SerializeField] public float MaxRange = 6f;
    [SerializeField] public float LungeDuration = 0.75f;

    private MechLunge lunge;
    private Transform player;
    private float timer;

    public bool CanRun()
    {
        if (Agent?.Value == null) return false;

        GameObject mech = Agent.Value;
        var bgAgent = mech.GetComponent<BehaviorGraphAgent>();

        bgAgent.BlackboardReference.GetVariableValue("PlayerTransform", out player);
        if (player == null) return false;

        float distance = Vector3.Distance(mech.transform.position, player.position);
        return distance < MaxRange; //attacks only in range
    }

    protected override Status OnStart()
    {
        timer = 0f;

        if (Agent?.Value == null) return Status.Failure;

        lunge = Agent.Value.GetComponent<MechLunge>();
        if (lunge == null) return Status.Failure;

        GameObject mech = Agent.Value;
        var bgAgent = mech.GetComponent<BehaviorGraphAgent>();
        bgAgent.BlackboardReference.SetVariableValue("lungeFinished", false);

        lunge.StartLunge();

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        timer += Time.deltaTime;

        GameObject mech =Agent.Value;
        var bgAgent = mech.GetComponent<BehaviorGraphAgent>();

        bool finished = false;
        bgAgent.BlackboardReference.GetVariableValue("lungeFinished", out finished);

        if (timer >= LungeDuration && finished)
        {
            return Status.Success;
        }

        return Status.Running;
    }
}