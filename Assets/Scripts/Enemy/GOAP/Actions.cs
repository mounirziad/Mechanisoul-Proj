using System.Collections.Generic;
using UnityEngine;

public class Actions
{
    public string Name { get; }
    public float Cost { get; private set; }

    public HashSet<AgentBelief> Preconditions { get; } = new(); //conditions must be met BEFORE action
    public HashSet<AgentBelief> Effects { get; } = new(); //After action, effects take place

    IActionStrategy strategy; //Decoupling

    public bool Complete => strategy.Complete; //tell the system if it is done running

    public void Start() => strategy.Start();

    public void Update(float deltaTime) //chck if the action can be performed
    {
        if (strategy.CanPerform)
        {
            strategy.Update(deltaTime);
        }

        if (!strategy.Complete) return;
    }

    public void Stop() => strategy.Stop();
}
