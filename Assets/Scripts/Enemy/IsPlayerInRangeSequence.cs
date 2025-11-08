using System;
using Unity.Behavior;
using UnityEngine;
using Composite = Unity.Behavior.Composite;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Is Player In Range", story: "[isPlayerInRange]", category: "Flow", id: "c54d3eaa8104fd3b3e4e4982c3aa0aed")]
public partial class IsPlayerInRangeSequence : Composite
{
    [SerializeReference] public BlackboardVariable<bool> IsPlayerInRange;

    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

