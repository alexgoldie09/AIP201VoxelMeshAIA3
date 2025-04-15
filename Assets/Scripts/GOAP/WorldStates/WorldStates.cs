using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorldState : SerializableDictionary<string, int> { }

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
        if(states.ContainsKey(key))
        {
            states[key] += value;
            if(states[key] <= 0)
            {
                RemoveState(key);
            }
        }
        else
        {
            states.Add(key, value);
        }
    }

    public WorldState GetStates() => states;
}
