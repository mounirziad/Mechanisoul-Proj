using System.Collections.Generic;

    public interface IGoapBehaviour
    {
        Dictionary<string, AgentBelief> ProvideBeliefs();
        HashSet<AgentAction> ProvideActions(Dictionary<string, AgentBelief> beliefs);
        HashSet<AgentGoal> ProvideGoals(Dictionary<string, AgentBelief> beliefs);
    }
