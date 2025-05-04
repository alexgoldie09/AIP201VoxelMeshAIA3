/*
 * Node.cs
 * -------
 * This class represents a single step (or world state) within the GOAP planning graph.
 *
 * Tasks:
 *  - Stores a reference to the parent node (for backtracking).
 *  - Maintains cumulative cost and current simulated world state.
 *  - Holds the action taken to reach this node from its parent.
 *
 * References:
 *  Integral to GPlanner’s recursive state simulation.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Node
{
    public Node parent;
    public float cost;
    public WorldState state;
    public GAction action;

    // Constructor for a standard node from a parent and action
    public Node(Node _parent, float _cost, WorldState _allStates, GAction _action)
    {
        parent = _parent;
        cost = _cost;
        state = new WorldState(_allStates);
        action = _action;
    }


    // Constructor for root node – merges world state and belief state
    public Node(Node _parent, float _cost, WorldState _allStates, WorldState _beliefStates, GAction _action)
    {
        parent = _parent;
        cost = _cost;
        state = new WorldState(_allStates);
        foreach(var b in _beliefStates.GetLivePairs())
        {
            if(!state.ContainsKey(b.Key))
                state.Add(b.Key, b.Value);
        }
        action = _action;
    }
}