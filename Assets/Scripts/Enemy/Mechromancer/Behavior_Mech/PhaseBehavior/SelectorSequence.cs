using System;
using System.Collections.Generic;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Composite = Unity.Behavior.Composite;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Selector", story: "Selects first child whose conditions are valid, runs it, resets every frame", category: "Flow", id: "selector-sequence")]
public partial class SelectorSequence : Composite
{
    private int current = -1;
    private float lastAttackTime = -999f;
    private const float ATTACK_COOLDOWN = 1.5f;

    protected override Status OnStart()
    {
        current = -1;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        Node child;
        Status result;

        if (current != -1)
        {
            child = Children[current];
            result = child.Tick();

            if (result == Status.Running)
            {
                return Status.Running;
            }

            if (result == Status.Success)
            {
                lastAttackTime = Time.time;
                current = -1;
                Debug.Log("SelectorSequence: Attack completed, starting cooldown");
                return Status.Success;
            }

            current = -1;
            return Status.Running;
        }

        if (Time.time - lastAttackTime < ATTACK_COOLDOWN)
        {
            return Status.Running;
        }

        current = PickNextAttack();

        if (current == -1)
        {
            Debug.LogWarning("SelectorSequence: No valid attacks found");
            return Status.Failure;
        }

        Debug.Log($"SelectorSequence: Selected attack index {current}");
        child = Children[current];
        result = child.Tick();

        if (result == Status.Running)
        {
            return Status.Running;
        }

        if (result == Status.Success)
        {
            lastAttackTime = Time.time;
            current = -1;
            Debug.Log("SelectorSequence: Attack completed immediately, starting cooldown");
            return Status.Success;
        }

        current = -1;
        return Status.Running;
    }

    protected override void OnEnd()
    {
        current = -1;
    }

    private int PickNextAttack()
    {
        List<int> validAttacks = new List<int>();

        for (int i = 0; i < Children.Count; i++)
        {
            Node node = Children[i];

            if (node is IAttackCondition cond)
            {
                if (cond.CanRun())
                {
                    validAttacks.Add(i);
                }
            }
            else
            {
                validAttacks.Add(i);
            }
        }

        if (validAttacks.Count == 0)
        {
            return -1;
        }

        int randomIndex = UnityEngine.Random.Range(0, validAttacks.Count);
        return validAttacks[randomIndex];
    }
}

public interface IAttackCondition
{
    bool CanRun();
}