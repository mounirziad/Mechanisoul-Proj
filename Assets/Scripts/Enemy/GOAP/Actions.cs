using System.Collections.Generic;
using UnityEngine;

public class AgentAction
{
    public string Name { get; }
    public float Cost { get; private set; }

    public HashSet<AgentBelief> Preconditions { get; } = new(); //conditions must be met BEFORE action
    public HashSet<AgentBelief> Effects { get; } = new(); //After action, effects take place

    IActionStrategy strategy; //Decoupling what is happening with the action

    public bool Complete => strategy.Complete; //tell the system if it is done running

    AgentAction(string name)
    {
        Name = name;
    }

    public void Start() => strategy.Start();

    public void Update(float deltaTime) //check if the action can be performed and update the strategy
    {
        if (strategy.CanPerform)
        {
            strategy.Update(deltaTime);
        }

        if (!strategy.Complete) return; //if strategy is still trying to execute

        foreach (var effect in Effects) //apply effects into the world
        {
            effect.Evaluate();
        }
    }

    public void Stop() => strategy.Stop();

    public class Builder
    {
        readonly AgentAction action;

        public Builder (string name)
        {
            action = new AgentAction(name)
            {
                Cost = 1
            };
        }

        public Builder WithCost(float cost)
        {
            action.Cost = cost;
            return this;
        }

        public Builder WithStrategy(IActionStrategy strategy)
        {
            action.strategy = strategy;
            return this;
        }

        public Builder AddPrecondition(AgentBelief precondition)
        {
            action.Preconditions.Add(precondition);
            return this;
        }

        public Builder AddEffect(AgentBelief effect)
        {
            action.Effects.Add(effect);
            return this;
        }

        public AgentAction Build()
        {
            return action;
        }
    }
}
