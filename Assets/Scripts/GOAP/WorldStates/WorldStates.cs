/*
 * WorldStates.cs
 * --------------
 * This script defines a container for the agent's belief states and the world's observable state,
 * used by GOAP (Goal-Oriented Action Planning) agents to reason about preconditions, effects, and goals.
 *
 * Tasks:
 *  - WorldState class is a specialized string-to-int serializable dictionary inheriting from SerializableDictionary.
 *  - WorldStates class provides state manipulation utility methods (Add, Remove, Modify).
 *      - Used for adding or modifying agent or world conditions.
 *      - Includes logic to handle automatic cleanup.
 *      - Supports defensive programming by warning when modifying nonexistent states.
 *
 * Extras:
 *  - Can be duplicated via its Clone() method (inherited).
 *  - Gracefully adds states that do not yet exist when using ModifyState().
 *
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WorldState : SerializableDictionary<string, int> 
{
    // Default Constructor
    public WorldState() : base() { }

    // Overloaded Constructors
    public WorldState(IDictionary<string, int> dict) : base(dict) { }

    public WorldState(WorldState other) : base(other.ToDictionary()) { }
}

public class WorldStates
{
    public WorldState states;

    // Default Constructor
    public WorldStates() 
    { 
        states = new WorldState();
    }

    /* 
     * HasState() checks if a state key exists.
     */
    public bool HasState(string key) => states.ContainsKey(key);

    /* 
     * AddState() adds a new state if it doesn't already exist.
     */
    public void AddState(string key, int value) => states.Add(key, value);

    /* 
     * RemoveState() removes a state safely.
     */
    public void RemoveState(string key)
    {
        if (states.ContainsKey(key))
        {
            states.Remove(key);
        }
    }

    /* 
     * SetState() sets a state to a specific value. 
     * - add if new
     * - replace if exists
     */
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

    /* 
     * ModifyState() adjusts a state by incrementing/decrementing.
     * - Adds the state if it doesn’t exist and value is positive
     * - Removes the state if its final value is zero or negative
     */
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
            if (value > 0)
            {
                states.Add(key, value);
            }
            else
            {
                Debug.LogWarning($"Trying to subtract from missing world state key: {key}");
            }
        }
        // Optional: Log state change (commented out)
        // Debug.Log($"Modified state [{key}] now has value: {states[key]}");
    }

    /* 
     * GetStates() returns the current dictionary of world states.
     */
    public WorldState GetStates() => states;
}
