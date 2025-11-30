using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Composite = Unity.Behavior.Composite;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Selector", story: "Selects first child whose conditions are valid, runs it, resets every frame", category: "Flow", id: "selector-sequence")]
public partial class SelectorSequence : Composite
{
    private int current = -1;

    protected override Status OnStart()
    {
        current = -1;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        current = PickNextAttack();

        if (current == -1)
        {
            return Status.Failure;
        }

        Node child = Children[current];

        Status result = child.Tick();

        if (result == Status.Running)
        {
            return Status.Running;
        }

        if (result == Status.Success)
        {
            return Status.Success;
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        current = -1;
    }

    private int PickNextAttack()
    {
        for (int i = 0; i < Children.Count; i++)
        {
            Node node = Children[i];

            if (node is IAttackCondition cond)
            {
                if (cond.CanRun())
                {
                    return i;
                }
            }
            else
            {
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