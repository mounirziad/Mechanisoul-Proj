using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Composite = Unity.Behavior.Composite;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Weighted Sequence", story: "Select attacks based off of distance and random weights", category: "Flow", id: "c82d0e7ace1e70754fc1b944eb8ab7f2")]
public class WeightedSequence : Composite
{
    /*[SerializeField]
    [Tooltip("Blackboard GameObject variable that contains boss information")]
    public BlackboardVariable<GameObject> Owner;

    private List<MethodInfo> childMethods;
    private bool isInitialized;

    protected override void OnStart()
    {
        if (!isInitialized)
        {
            InitalizeReflection();
        }

        base.OnStart();
    }

    private void InitalizeReflection()
    {
        childMethods = new List<MethodInfo>();

        if (Owner == null || Owner.Value == null)
        {
            Debug.LogError("WeightedSequence: No Owner GameObject assigned in Blackboard");
            return;
        }

        var components = Owner.Value.GetComponent<MonoBehaviour>();

        foreach (var comp in components)
        {
            var methods = comp.GetType().GetMethods
        }
    }*/
}

public interface IWeightedAttack
{
    float GetWeight(float distance);
}

