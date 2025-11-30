using System;
using System.Reflection;
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

    private static readonly MethodInfo updateMethod = typeof(Node).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic);

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

        if (currentChild == 0 || currentChild >= Children.Count)
        {
            return Status.Failure;
        }

        var child = Children[currentChild];

        if (child == null)
        {
            Debug.LogError($"Child node is null at index {currentChild}");
            currentChild = -1;
            childStarted = false;
            return Status.Failure;
        }

        updateMethod.Invoke(child, null);

        Status result = child.CurrentStatus;

        if (result == Status.Running)
        {
            return Status.Running;
        }

        EndNode(child);

        currentChild = -1;
        childStarted = false;

        return result;
    }

    protected override void OnEnd()
    {
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
            }
            else
            {
                //if no condition interface then it is always allowed
                return i;
            }
        }

        return -1;
    }
}

public interface IAttackCondition
{
    bool CanRun();
}