using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorldState : SerializableDictionary<string, int> 
{
    public WorldState() : base() { }

    public WorldState(IDictionary<string, int> dict) : base(dict) { }

    public WorldState(WorldState other) : base(other.ToDictionary()) { }
}

public class WorldStates
{
    public WorldState states;

    public WorldStates() 
    { 
        states = new WorldState();
    }

    public bool HasState(string key) => states.ContainsKey(key);

    public void AddState(string key, int value) => states.Add(key, value);

    public void RemoveState(string key)
    {
        if (states.ContainsKey(key))
        {
            states.Remove(key);
        }
    }

    public void SetState(string key, int value)
    {
        if (states.ContainsKey(key))
        {
            states[key] = value;
        }
        else
        {
            states.Add(key, value);
        }
    }

    public void ModifyState(string key, int value)
    {
        if (states.ContainsKey(key))
        {
            states[key] += value;
            if (states[key] <= 0)
            {
                RemoveState(key);
            }
        }
        else
        {
            // Automatically add it if trying to increase
            if (value > 0)
            {
                states.Add(key, value);
            }
            else
            {
                Debug.LogWarning($"Trying to subtract from missing world state key: {key}");
            }
        }
        //Debug.Log($"Modified state [{key}] now has value: {states[key]}");
    }

    public WorldState GetStates() => states;
}
