/*
 * GPlanner.cs
 * -----------
 * This class generates an optimal sequence of actions (plan) that transitions the agent from the current world state
 * to a goal state using available actions and belief states.
 *
 * Tasks:
 *  - Builds a graph of possible state transitions by applying effects of actions to current state.
 *  - Evaluates action preconditions against the state at each node.
 *  - Finds the least-cost valid path that satisfies the goal.
 *
 * Extras:
 *  - Uses a depth-first recursive search (via BuildGraph).
 *  - Returns a queue of GActions that agents can execute sequentially.
 *
 * References:
 *  Based on Goal-Oriented Action Planning model used in De Byl's Unity AI Masterclass.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GPlanner
{
    /* 
     * Plan() returns a queue of available actions in the form of the agent's plan.
     */
    public Queue<GAction> Plan(List<GAction> actions, WorldState goal, WorldStates beliefstates)
    {
        // 1. Filter usable actions that are not blocked or disabled
        List<GAction> usableActions = actions.FindAll(a => a.IsAchievable());

        // 2. Initialize the root node (planner state includes world + agent beliefs)
        Node start = new Node(null, 0, GWorld.Instance.GetWorld().GetStates(), beliefstates.GetStates(), null);

        // 3. Recursively build out graph from initial state
        List<Node> leaves = new();
        bool success = BuildGraph(start, leaves, usableActions, goal);
        if (!success) { return null; }

        // 4. Find the cheapest path (lowest cost)
        Node cheapest = leaves.OrderBy(n => n.cost).FirstOrDefault();
        if (cheapest == null) { return null; }

        // 5. Trace back from goal node to root to build final action sequence
        List<GAction> result = new();
        for (Node n = cheapest; n != null; n = n.parent)
        {
            if (n.action != null)
                result.Insert(0, n.action); // Reverse order
        }

        return new Queue<GAction>(result);
    }

    /* 
     * BuildGraph() returns whether a tree is valid from its actions and states.
     */
    private bool BuildGraph(Node parent, List<Node> leaves, List<GAction> actions, WorldState goal)
    {
        bool foundPath = false;

        foreach (GAction action in actions)
        {
            // Only consider actions whose preconditions match the current state
            if (!action.IsAchievableGiven(parent.state)) continue;

            // Clone current state and apply action effects
            WorldState currentState = new WorldState(parent.state);
            foreach (var effect in action.afterEffects.GetLivePairs())
            {
                if (currentState.ContainsKey(effect.Key))
                    currentState[effect.Key] += effect.Value;
                else
                    currentState.Add(effect.Key, effect.Value);
            }

            // Create a new node from the modified state
            Node newNode = new(parent, parent.cost + action.cost, currentState, action);

            // If goal is satisfied, add to leaves
            if (GoalAchieved(goal, currentState))
            {
                leaves.Add(newNode);
                foundPath = true;
            }
            else
            {
                // Recursive expansion using remaining actions
                List<GAction> subset = actions.Where(a => a != action).ToList();
                if (BuildGraph(newNode, leaves, subset, goal))
                    foundPath = true;
            }
        }

        return foundPath;
    }

    /* 
     * GoalAchieved() returns whether a current state satisfies the goal conditions.
     */
    private bool GoalAchieved(WorldState goal, WorldState current)
    {
        foreach (var goalEntry in goal.GetLivePairs())
        {
            if (!current.ContainsKey(goalEntry.Key) || current[goalEntry.Key] < goalEntry.Value)
            {
                return false;
            }
        }
        return true;
    }
}
