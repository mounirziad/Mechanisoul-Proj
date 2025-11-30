using System;
using Unity.Behavior;
using UnityEngine;
using Composite = Unity.Behavior.Composite;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Weighted Sequence", story: "Select attacks based off of distance and random weights", category: "Flow", id: "c82d0e7ace1e70754fc1b944eb8ab7f2")]
public partial class WeightedSequence : Composite
{

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

