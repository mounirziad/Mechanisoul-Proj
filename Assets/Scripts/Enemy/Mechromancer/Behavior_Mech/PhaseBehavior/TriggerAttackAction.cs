using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Trigger Attack", story: "[Agent] attacks", category: "Action", id: "7a6b38fc4553bd031472f1a7fbc608dd")]
public partial class TriggerAttackAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    [SerializeField] public string AttackTrigger;

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

        mech.TriggerAttack(AttackTrigger);

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

