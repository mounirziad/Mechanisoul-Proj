using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Composite = Unity.Behavior.Composite;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Selector", story: "Run children in order until one succeeds", category: "Flow", id: "cedf5bfa1f7458a780fca892df02c1dc")]
public partial class SelectorSequence : Composite
{
    private int currentChild = -1;
    private bool childStarted = false;

    protected override Status OnUpdate()
    {
        if (currentChild == -1)
        {
            int next = PickNextAttack();

            if (next == -1)
            {
                return Status.Failure;
            }

            currentChild = next;
            StartNode(Children[currentChild]);
            childStarted = true;
        }

        var child = Children[currentChild];
        var result = child.CurrentStatus;

        if (result == Status.Running)
        {
            return Status.Running;
        }

        EndNode(child);

        currentChild = -1;
        childStarted = false;

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (currentChild != -1 && childStarted)
        {
            EndNode(Children[currentChild]);
        }

        currentChild = -1;
        childStarted = false;
    }

    //returns index of the next valid attack in priority order
    private int PickNextAttack()
    {
        for (int i = 0; i < Children.Count; i++)
        {
            var node = Children[i];

            if (node is IAttackCondition cond)
            {
                if (cond.CanRun())
                {
                    return i;
                }
                else
                {
                    //if no condition interface then it is always allowed
                    return i;
                }
            }
        }

        return -1;
    }
}

public interface IAttackCondition
{
    bool CanRun();
}