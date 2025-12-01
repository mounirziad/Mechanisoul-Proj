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
    //[SerializeReference] public BlackboardVariable<GameObject> Agent;
    //[SerializeReference] public BlackboardVariable<Transform> PlayerTransform;

    //[SerializeField] public List<string> AttackIds = new List<string>();

    private int currentIndex = -1;

    protected override Status OnStart()
    {
        currentIndex = -1;
        Debug.Log("WeightedSequence: OnStart called");
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (Children.Count == 0)
        {
            Debug.LogWarning("WeightedSequence: No children nodes found");
            return Status.Failure;
        }

        //if child is running, continue running
        if (currentIndex != -1)
        {
            var child = Children[currentIndex];
            Debug.Log($"WeightedSequence: Continuing running child {currentIndex}: {child}");
            var result = StartNode(child);

            if (result == Status.Running)
            {
                return Status.Running;
            }

            //once finished, reset and return success
            currentIndex = -1;
            Debug.Log($"WeightedSequence: Child {child} finished with result {result}");
            return result;
        }

        currentIndex = UnityEngine.Random.Range(0, Children.Count);
        var startResult = StartNode(Children[currentIndex]);

        if (startResult == Status.Running)
        {
            return Status.Running;
        }

        currentIndex = -1;
        return startResult;

        //make sure blackboard values exist
        /*if (Agent?.Value == null || PlayerTransform?.Value == null)
        {
            Debug.LogWarning("WeightedSequence: Agent or PlayerTransform is null");
            return Status.Failure;
        }

        float distance = Vector3.Distance(Agent.Value.transform.position, PlayerTransform.Value.position);
        Debug.Log($"WeightedSequence: Distance to player: {distance}");

        //compute weights for attacks
        var weights = new List<float>(Children.Count);
        float total = 0f;

        for (int i = 0; i < Children.Count; i++)
        {
            var child = Children[i];
            string attackId = null;

            //read property "AttackId" string using API
            //returns true if the property exists and outputs the value
            if (!PropertyContainer.TryGetValue(child, "AttackId", out attackId) || string.IsNullOrEmpty(attackId))
            {
                var bbVarField = child.GetType().GetField("AttackId");
                if (bbVarField != null)
                {
                    var bbVar = bbVarField.GetValue(child) as BlackboardVariable<string>;
                    if (bbVar != null)
                    {
                        attackId = bbVar.Value;
                    }
                }

                if (string.IsNullOrEmpty(attackId))
                {
                    Debug.Log($"WeightedSequence: Child {child} has no AttackId or empty. Weight = 0");
                    weights.Add(0f);
                    continue;
                }
            }

            float w = GetWeightForAttack(attackId, distance);
            weights.Add(w);
            total += w;
            Debug.Log($"WeightedSequence: Child {child} AttackId = {attackId}, Weight = {w}");
        }

        if (total <= 0f)
        {
            Debug.LogWarning("WeightedSequence: No child returns a positive weight");
            return Status.Failure;
        }

        //weighted random selection
        float randomPick = UnityEngine.Random.value * total;
        int chosenIndex = -1;

        for (int i = 0; i < weights.Count; i++)
        {
            randomPick -= weights[i];
            if (randomPick <= 0f)
            {
                chosenIndex = i;
                break;
            }
        }

        if (chosenIndex == -1)
        {
            chosenIndex = weights.Count - 1;
        }

        currentIndex = chosenIndex;
        Debug.Log($"WeightedSequence: Selected child index {currentIndex} ({Children[currentIndex]} to run");

        var startResult = StartNode(Children[chosenIndex]);
        if (startResult == Status.Running)
        {
            Debug.Log($"WeightedSequence: Child {Children[chosenIndex]} is running...");
            return Status.Running;
        }

        Debug.Log($"WeightedSequence: Child {Children[chosenIndex]} finished with result {startResult}");
        currentIndex = -1;
        return startResult;*/
    }

    protected override void OnEnd()
    {
        currentIndex = -1;
        Debug.Log("WeightedSequence: OnEnd called");
    }

    /*private float GetWeightForAttack(string id, float distance)
    {
        if (Agent?.Value == null)
        {
            Debug.LogWarning($"WeightedSequence: Agent is null in GetWeightForAttack for id: {id}");
            return 0f;
        }

        var scripts = Agent.Value.GetComponents<IWeightedAttack>();
        if (scripts == null || scripts.Length == 0)
        {
            Debug.LogWarning($"WeightedSequence: No IWeightAttack scripts found on Agent for id: {id}");
            return 0f;
        }

        foreach (var s in scripts)
        {
            if (s == null)
            {
                continue;
            }

            var name = s.GetType().Name;
            if (name.StartsWith(id, StringComparison.OrdinalIgnoreCase))
            {
                float weight = Mathf.Max(0f, s.GetWeight(distance));
                Debug.Log($"WeightedSequence: Matched {name} (StartsWith) with weight {weight}");
                return weight;
            }
        }

        foreach (var s in scripts)
        {
            if (s == null)
            {
                continue;
            }

            if (string.Equals(s.GetType().Name, id, StringComparison.OrdinalIgnoreCase))
            {
                float weight = Mathf.Max(0f, s.GetWeight(distance));
                Debug.Log($"WeightedSequence: Matched {s.GetType().Name} (Equals) with weight {weight}");
                return weight;
            }
        }

        Debug.LogWarning($"WeightedSequence: No matching IWeightedAttack found for id {id}");
        return 0f;
    }*/
}

public interface IWeightedAttack
{
    float GetWeight(float distance);
}

