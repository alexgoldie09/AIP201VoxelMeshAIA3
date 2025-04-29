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

    public Node(Node _parent, float _cost, WorldState _allStates, GAction _action)
    {
        this.parent = _parent;
        this.cost = _cost;
        this.state = new WorldState(_allStates);
        this.action = _action;
    }

    public Node(Node _parent, float _cost, WorldState _allStates, WorldState _beliefStates, GAction _action)
    {
        this.parent = _parent;
        this.cost = _cost;
        this.state = new WorldState(_allStates);
        foreach(KeyValuePair<string, int> b in _beliefStates.GetLivePairs())
        {
            if(!this.state.ContainsKey(b.Key))
            {
                this.state.Add(b.Key, b.Value);
            }
        }
        this.action = _action;
    }
}