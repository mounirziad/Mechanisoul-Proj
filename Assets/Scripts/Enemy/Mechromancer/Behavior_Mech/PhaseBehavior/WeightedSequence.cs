using System;
using System.Collections.Generic;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Composite = Unity.Behavior.Composite;

[Serializable, GeneratePropertyBag]
[NodeDescription(
    name: "Weighted Sequence", 
    story: "Agent attacks based off of PlayerTransform distance and random weights", 
    category: "Flow", 
    id: "c82d0e7ace1e70754fc1b944eb8ab7f2")]
public class WeightedSequence : Composite
{
    private int currentIndex = -1;

    private BehaviorGraphAgent agent;
    private Transform playerTransform;

    protected override Status OnStart()
    {
        currentIndex = -1;

        agent = this.agent as BehaviorGraphAgent;
        if (agent == null )
        {
            Debug.LogError("WeightedSequence: this.Agent is not a behavior graph agent");
            return Status.Failure;
        }

        agent.BlackboardReference.GetVariableValue("PlayerTransform", out playerTransform);
        if (playerTransform == null)
        {
            Debug.LogError("WeightedSequence: Missing PlayerTransform in blackboard");
            return Status.Failure;
        }

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Children.Count == 0)
            return Status.Failure;

        if (currentIndex != -1)
        {
            var result = StartNode(Children[currentIndex]);

            if (result == Status.Running)
                return Status.Running;

            currentIndex = -1;
            return Status.Success;
        }

        float distance = Vector3.Distance(agent.transform.position, playerTransform.position);

        List<int> indices = new List<int>();
        List<float> weights = new List<float>();
        float total = 0f;

        for (int i = 0; i < Children.Count; i++)
        {
            float w = GetWeightForChild(Children[i], distance);
            if (w > 0f)
            {
                indices.Add(i);
                weights.Add(w);
                total += w;
            }
        }

        if (indices.Count == 0)
        {
            return Status.Running;
        }

        float r = UnityEngine.Random.value * total;

        int chosenIndex = 0;
        for (int i = 0; i < weights.Count; i++)
        {
            r -= weights[i];
            if (r <= 0f)
            {
                chosenIndex = indices[i];
                break;
            }
        }

        currentIndex = chosenIndex;

        var startResult = StartNode(Children[currentIndex]);
        return startResult == Status.Running ? Status.Running : Status.Success;
    }

    private float GetWeightForChild(Node child, float distance)
    {
        string id = child.GetType().Name.ToLower();

        if (id.Contains("combo"))
        {
            if (distance < 4f) return 0.7f;
            if (distance < 12f) return 0.3f;
            return 0f;
        }

        if (id.Contains("lunge"))
        {
            if (distance < 4f) return 0.3f;
            if (distance < 12) return 0.7f;
            return 0f;
        }

        if (id.Contains("lightning"))
        {
            return distance > 12f ? 1f : 0f;
        }

        return 0f;
    }

    protected override void OnEnd()
    {
        currentIndex = -1;
    }
}

public interface IWeightedAttack
{
    float GetWeight(float distance);
}

