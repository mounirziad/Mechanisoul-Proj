using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Trigger Attack", story: "[Agent] attacks with [AttackTrigger]", category: "Action", id: "7a6b38fc4553bd031472f1a7fbc608dd")]
public partial class TriggerAttackAction : Action, IAttackCondition
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    [CreateProperty]
    [SerializeField]
    public BlackboardVariable<string> AttackTrigger;

    private Mechromancer mech;
    private MechAnimationController animationController;
    private BehaviorGraphAgent bgAgent;

    public bool CanRun()
    {
        if (Agent?.Value == null) return false;

        bgAgent = Agent.Value.GetComponent<BehaviorGraphAgent>();
        if (bgAgent == null) return true;

        bool finished = true;
        bgAgent.BlackboardReference.GetVariableValue("attackFinished", out finished);

        Debug.Log($"TriggerAttackAction.CanRun() for '{AttackTrigger?.Value}': attackFinished={finished}");
        return finished;
    }

    protected override Status OnStart()
    {
        Debug.Log($"TriggerAttackAction: OnStart called with trigger '{AttackTrigger?.Value}'");
        
        if (Agent == null || Agent.Value == null)
        {
            Debug.LogError("TriggerAttackAction: Agent is null!");
            return Status.Failure;
        }

        mech = Agent.Value.GetComponent<Mechromancer>();
        animationController = Agent.Value.GetComponent<MechAnimationController>();
        bgAgent = Agent.Value.GetComponent<BehaviorGraphAgent>();
        
        if (mech == null)
        {
            Debug.LogError("TriggerAttackAction: Mechromancer component not found!");
            return Status.Failure;
        }

        if (bgAgent != null)
        {
            bgAgent.BlackboardReference.SetVariableValue("attackFinished", false);
        }

        Debug.Log($"TriggerAttackAction: Triggering attack '{AttackTrigger.Value}'");
        mech.TriggerAttack(AttackTrigger.Value);

        if (animationController != null)
        {
            animationController.SetIsAttacking(true);
            Debug.Log($"TriggerAttackAction: Set IsAttacking=true for {AttackTrigger.Value}");
        }
        else
        {
            Debug.LogWarning("TriggerAttackAction: MechAnimationController is null!");
        }

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (bgAgent == null) return Status.Success;

        bool finished = false;
        bgAgent.BlackboardReference.GetVariableValue("attackFinished", out finished);

        return finished ? Status.Success : Status.Running;
    }

    protected override void OnEnd()
    {
        if (animationController != null)
        {
            animationController.SetIsAttacking(false);
            Debug.Log("TriggerAttackAction: Set IsAttacking=false");
        }

        if (bgAgent != null)
        {
            bgAgent.BlackboardReference.SetVariableValue("attackFinished", true);
        }
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

    private LightningController controller;
    private BehaviorGraphAgent bgAgent;
    private MechAnimationController animationController;

    public bool CanRun()
    {
        if (Agent?.Value == null) return false;

        bgAgent = Agent.Value.GetComponent<BehaviorGraphAgent>();

        bool lightningFinished = true;
        bgAgent.BlackboardReference.GetVariableValue("LightningFinished", out lightningFinished);

        return lightningFinished;
    }

    protected override Status OnStart()
    {
        if (Agent?.Value == null) return Status.Failure;

        controller = Agent.Value.GetComponent<LightningController>();
        bgAgent = Agent.Value.GetComponent<BehaviorGraphAgent>();
        animationController = Agent.Value.GetComponent<MechAnimationController>();

        if (controller == null)
        {
            return Status.Failure;
        }

        bgAgent.BlackboardReference.SetVariableValue("LightningFinished", false);
        if (animationController != null)
        {
            animationController.TriggerCastLightning();
        }

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) return Status.Failure;

        controller.CastLightningAtGround(player.transform.position, AoERadius, Damage);

        controller.CastLightning();


        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        //Completes immediately, AoE is handled separately
        bool finished = false;
        bgAgent.BlackboardReference.GetVariableValue("LightningFinished", out finished);

        if (finished)
        {
            return Status.Success;
        }

        return Status.Running;
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
        var animator = Agent.Value.GetComponent<Animator>();

        if (animator == null || resurrection == null)
        {
            Debug.LogError("Resurrection or animator component not found on Agent");
            return Status.Failure;
        }

        animator.SetTrigger("Resurrect");

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