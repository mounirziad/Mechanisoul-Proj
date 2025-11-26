using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription
    (name: "Loop Until Health Below", 
    story: "Loops until health < threshold", 
    category: "Action", 
    id: "47decb8800f89832980d1e35f64d4449")]
public class LoopUntilHealthBelow : Action
{
    [SerializeField] private Mechromancer mech;
    [SerializeField] private float threshold = 20f;

    protected override Status OnUpdate()
    {
        if (mech == null)
        {
            return Status.Failure;
        }

        if (mech.currentHealth < threshold)
        {
            return Status.Success;
        }

        return Status.Running;
    }
}

