using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GPlanner
{
    public Queue<GAction> Plan(List<GAction> actions, WorldState goal, WorldStates beliefstates)
    {
        List<GAction> usableActions = new List<GAction>();
        foreach (GAction action in actions)
        {
            if (action.IsAchievable())
            {
                usableActions.Add(action);
            }
        }

        List<Node> nodes = new List<Node>();
        Node start = new Node(null, 0, GWorld.Instance.GetWorld().GetStates(), beliefstates.GetStates(), null);

        bool success = BuildGraph(start, nodes, usableActions, goal);

        if (!success)
        {
            return null;
        }

        // Find cheapest node
        Node cheapest = null;
        foreach (Node node in nodes)
        {
            if (cheapest == null || node.cost < cheapest.cost)
            {
                cheapest = node;
            }
        }

        // Build action queue
        List<GAction> result = new List<GAction>();
        Node n = cheapest;
        while (n != null)
        {
            if (n.action != null)
            {
                result.Insert(0, n.action);
            }
            n = n.parent;
        }

        Queue<GAction> queue = new Queue<GAction>();
        foreach (GAction action in result)
        {
            queue.Enqueue(action);
        }

        //Debug.Log("The plan is: ");
        //foreach (GAction action in queue)
        //{
        //    Debug.Log("Q: " + action.actionName);
        //}

        return queue;
    }

    private bool BuildGraph(Node parent, List<Node> nodes, List<GAction> usableActions, WorldState goal)
    {
        bool foundPath = false;

        foreach (GAction action in usableActions)
        {
            if(action.IsAchievableGiven(parent.state))
            {
                WorldState currentState = new WorldState(parent.state);

                // Apply effects
                foreach (KeyValuePair<string,int> effect in action.afterEffects.GetLivePairs())
                {
                    if (currentState.ContainsKey(effect.Key))
                    {
                        currentState[effect.Key] += effect.Value;
                    }
                    else
                    {
                        currentState.Add(effect.Key, effect.Value);
                    }
                }

                // Create a new node
                Node newNode = new Node(parent, parent.cost + action.cost, currentState, action);

                if (GoalAchieved(goal, currentState))
                {
                    nodes.Add(newNode);
                    foundPath = true;
                }
                else
                {
                    List<GAction> subset = ActionSubset(usableActions, action);
                    // Recurse
                    bool found = BuildGraph(newNode, nodes, subset, goal);
                    if (found)
                    {
                        foundPath = true;
                    }
                }
            }
        }
        return foundPath;
    }

    //private bool GoalAchieved(WorldState goal, WorldState current)
    //{
    //    foreach (KeyValuePair<string, int> goalEntry in goal.GetLivePairs())
    //    {
    //        if (goalEntry.Key == "Idle")
    //        {
    //            return true;
    //        }

    //        if (!current.ContainsKey(goalEntry.Key) || current[goalEntry.Key] < goalEntry.Value)
    //        {
    //            return false;
    //        }
    //    }
    //    return true;
    //}

    private bool GoalAchieved(WorldState goal, WorldState current)
    {
        foreach (KeyValuePair<string, int> goalEntry in goal.GetLivePairs())
        {
            if (!current.ContainsKey(goalEntry.Key) || current[goalEntry.Key] < goalEntry.Value)
            {
                return false;
            }
        }
        return true;
    }


    private List<GAction> ActionSubset(List<GAction> actions, GAction removeMe)
    {
        return actions.Where(a => a != removeMe).ToList();
    }
}
