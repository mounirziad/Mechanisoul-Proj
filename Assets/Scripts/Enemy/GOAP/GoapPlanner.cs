using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IGoapPlanner
{
    ActionPlan Plan(GoapAgent agent, HashSet<AgentGoal> goals, AgentGoal mostRecentGoal = null); //going to return action plans
}

public class GoapPlanner : IGoapPlanner
{
    public ActionPlan Plan(GoapAgent agent, HashSet<AgentGoal> goals, AgentGoal mostRecentGoal = null)
    {
        //Order goals by descending priority
        List<AgentGoal> orderedGoals = goals //HashSet
            .Where(AgentGoal => AgentGoal.DesiredEffects.Any(AgentBelief => !AgentBelief.Evaluate())) //using Linq //IEnumerable<AgentGoal>
            .OrderByDescending(AgentGoal => AgentGoal == mostRecentGoal ? AgentGoal.Priority - 0.01 : AgentGoal.Priority)
            .ToList(); //So we dont keep trying to do the same goal over and over again

        //Try to solve each goal in order
        foreach (var goal in orderedGoals)
        {
            Node goalNode = new Node(null, null, goal.DesiredEffects, 0);

            //If we can find a path to the goal, return the plan
            if (FindPath(goalNode, agent.actions))
            {
                //If the goal node has no leaves and no action to perform try a different goal
                if (goalNode.IsLeafDead) continue;

                Stack<AgentAction> actionStack = new Stack<AgentAction>();
                while (goalNode.Leaves.Count > 0)
                {
                    var cheapestLeaf = goalNode.Leaves.OrderBy(leaf => leaf.Cost).First();
                    goalNode = cheapestLeaf;
                    actionStack.Push(cheapestLeaf.Action);
                }

                return new ActionPlan(goal, actionStack, goalNode.Cost);
            }
        }

        Debug.LogWarning("No plan found");
        return null;
    }

    bool FindPath(Node parent, HashSet<AgentAction> actions)
    {
        //order actions by ascending cost
        var orderedActions = actions.OrderBy(AgentAction => AgentAction.Cost);

        foreach (var action in orderedActions)
        {
            var requiredEffects = parent.RequiredEffects;

            //Remove any effects thast evaluate to true there is no action to take
            requiredEffects.RemoveWhere(b => b.Evaluate());

            //If there are no required effects to fulfill, we have a plan
            if (requiredEffects.Count == 0)
            {
                return true;
            }

            if (action.Effects.Any(requiredEffects.Contains))
            {
                var newRequiredEffects = new HashSet<AgentBelief>(requiredEffects);
                newRequiredEffects.ExceptWith(action.Effects);
                newRequiredEffects.UnionWith(action.Preconditions);

                var newAvailableActions = new HashSet<AgentAction>(actions);
                //newAvailableActions.Remove(action);

                var newNode = new Node(parent, action, newRequiredEffects, parent.Cost + action.Cost);

                //Explore the new node
                if (FindPath(newNode, newAvailableActions))
                {
                    parent.Leaves.Add(newNode);
                    newRequiredEffects.ExceptWith(newNode.Action.Preconditions);
                }

                //if all effects are satisfied then return true
                if (newRequiredEffects.Count == 0)
                {
                    return true;
                }
            }
        }

        return false;
    }
}

public class Node
{
    public Node Parent { get; }
    public AgentAction Action { get; }
    public HashSet<AgentBelief> RequiredEffects { get; }
    public List<Node> Leaves { get; }
    public float Cost { get; }

    public bool IsLeafDead => Leaves.Count == 0 && Action == null;

    public Node(Node parent, AgentAction action, HashSet<AgentBelief> effects, float cost)
    {
        Parent = parent;
        Action = action;
        RequiredEffects = new HashSet<AgentBelief>(effects);
        Leaves = new List<Node>();
        Cost = cost;
    }

    public static implicit operator Node(Unity.Behavior.Node v)
    {
        throw new NotImplementedException();
    }

    internal Unity.Behavior.Node.Status Tick()
    {
        throw new NotImplementedException();
    }
}

public class ActionPlan
{
    public AgentGoal AgentGoal {  get; }
    public Stack<AgentAction> Actions { get; }
    public float TotalCost { get; set; }

    public ActionPlan(AgentGoal goal, Stack<AgentAction> actions, float totalCost)
    {
        AgentGoal = goal;
        Actions = actions;
        TotalCost = totalCost;
    }
}
