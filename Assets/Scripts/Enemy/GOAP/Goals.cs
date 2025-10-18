using System.Collections.Generic;
using UnityEngine;

public class AgentGoal
{
    public string Name { get; }
    public float Priority { get; private set; }
    public bool Enabled { get; set; } = true;
    public HashSet<AgentBelief> DesiredEffects { get; } = new(); //what we would like to come true if we can reach this goal

    AgentGoal(string name)
    {
        Name = name;
    }

    public class Builder
    {
        readonly AgentGoal goal;

        public Builder(string name)
        {
            goal = new AgentGoal(name);
        }

        public Builder WithPriority(float priority)
        {
            goal.Priority = priority;
            return this;
        }

        public Builder WithDesiredEffect(AgentBelief effect)
        {
            goal.DesiredEffects.Add(effect);
            return this;
        }

        public Builder SetEnabled(bool enabled)
        {
            goal.Enabled = enabled;
            return this;
        }

        public AgentGoal Build()
        {
            return goal;
        }
    }
}
