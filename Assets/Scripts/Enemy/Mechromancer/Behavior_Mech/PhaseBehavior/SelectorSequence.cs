using System;
using Unity.Behavior;
using UnityEngine;
using Composite = Unity.Behavior.Composite;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Selector", story: "Run children in order until one succeeds", category: "Flow", id: "cedf5bfa1f7458a780fca892df02c1dc")]
public partial class SelectorSequence : Composite
{
    private int currentIndex = 0;

    /*protected override void OnStart()
    {
        currentIndex = 0;
    }

    protected override void OnUpdate()
    {
        if (Children.Count == 0)
        {
            return Status.Failure;
        }

        while (currentIndex < Children.Count)
        {
            var status = Children[currentIndex].Tick();

            switch (status)
            {
                case Status.Running:
                    return Status.Running;

                case Status.Success:
                    return Status.Success;

                case Status.Failure:
                    currentIndex++;
                    continue;
            }
        }

        return Status.Failure;
    }*/
}

