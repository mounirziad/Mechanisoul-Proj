using System;
using Unity.Behavior;
using UnityEngine;
using Composite = Unity.Behavior.Composite;
using Unity.Properties;
using System.Collections.Generic;
using System.Reflection;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Weighted Selector", 
    story: "Selects attack based on distance and weighted random choice", 
    category: "Flow", 
    id: "weighted-selector")]
public partial class WeightedSelector : Composite
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    private int currentIndex = -1;

    private static readonly MethodInfo updateMethod = typeof(Node).GetMethod("Update", BindingFlags.Instance | BindingFlags.NonPublic);

    protected override Status OnStart()
    {
        currentIndex = -1;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (currentIndex == -1)
        {
            currentIndex = ChooseAttack();

            if (currentIndex == -1)
            {
                return Status.Failure; //No valid attacks
            }
        }

        var child = Children[currentIndex];

        if (child == null )
        {
            return Status.Failure;
        }

        updateMethod.Invoke(child, null);
        var result = child.CurrentStatus;

        switch (result)
        {
            case Status.Running:
                return Status.Running;

            case Status.Success:
                currentIndex = -1;
                return Status.Success;

            case Status.Failure:
                currentIndex = -1;
                return Status.Failure;
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        currentIndex = -1;
    }

    private int ChooseAttack()
    {
        float distance = GetDistanceToPlayer();

        List<(int index, float weight)> valid = new();

        for (int i = 0; i < Children.Count; i++)
        {
            if (Children[i] is IWeightedAttack attack)
            {
                float weight = attack.GetWeight(distance);

                if (weight > 0)
                {
                    valid.Add((i, weight));
                }
            }
        }

        if (valid.Count == 0)
        {
            return -1;
        }

        float total = 0;
        foreach (var v in valid)
        {
            total += v.weight;
        }

        float roll = UnityEngine.Random.value * total;

        foreach (var v in valid)
        {
            if (roll < v.weight)
            {
                return v.index;
            }

            roll -= v.weight;
        }

        return valid[valid.Count - 1].index;
    }

    private float GetDistanceToPlayer()
    {
        //var self = BlackboardReference.GetVariable<GameObject>("Self");
        if (Agent == null || Agent.Value == null)
        {
            Debug.LogError("Agent is not assigned in the blackboard");
            return float.MaxValue;
        }

        var player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player gameobject not found");
            return float.MaxValue;
        }

        return Vector3.Distance(Agent.Value.transform.position, player.transform.position);
    }
}

public interface IWeightedAttack
{
    float GetWeight(float distance);
}

