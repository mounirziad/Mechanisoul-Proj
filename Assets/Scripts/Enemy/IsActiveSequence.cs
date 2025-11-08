using System;
using Unity.Behavior;
using UnityEngine;
using Composite = Unity.Behavior.Composite;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Is Active", story: "[isActive]", category: "Action/Conditional", id: "3f32db274d22c8fc1bcd824a14ec563d")]
public partial class IsActiveSequence : Composite
{
    [SerializeReference] public BlackboardVariable<bool> IsActive;

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

