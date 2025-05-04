/*
 * GAction.cs
 * ----------
 * Abstract base class for all GOAP-compatible actions.
 *
 * Tasks:
 *  - Defines action name, preconditions, effects, duration, and cost.
 *  - Manages references to agent's NavMesh, beliefs, and inventory.
 *  - Provides utility for checking if action is usable (in general or in context).
 *  - Subclasses implement PrePerform() and PostPerform() to handle start/end logic.
 *
 * Extras:
 *  - Target objects can be assigned via tag or GameObject reference.
 *  - Running flag tracks if action is currently executing.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class GAction : MonoBehaviour
{
    public string actionName = "Action";
    public float cost = 1.0f;
    public GameObject target;
    public string targetTag;
    public float duration = 0;
    public WorldState preConditions;
    public WorldState afterEffects;
    public NavMeshAgent agent;
    public GAgent thisAgent;

    public WorldStates agentBeliefs;

    public GInventory inventory;
    public WorldStates beliefs;

    public bool running = false;

    // Called on instantiation to cache agent references and components.
    public virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        thisAgent = GetComponent<GAgent>();
        inventory = thisAgent.inventory;
        beliefs = thisAgent.beliefs;
    }

    /* 
     * IsAchievable() returns whether an action is achievable.
     * - Returns true by default
     * - Can be overridden to add custom rules
     */
    public virtual bool IsAchievable() => true;

    /* 
     * IsAchievableGiven() returns whether an action is achievable by the conditions given.
     * - If no preconditions, or conditions have been fulfilled it is achievable
     */
    public bool IsAchievableGiven(WorldState conditions)
    {
        if (preConditions == null) return true;

        foreach (var pair in preConditions.GetLivePairs())
        {
            if(!conditions.ContainsKey(pair.Key) || conditions[pair.Key] < pair.Value)
            {
                return false;
            }
        }
        return true;
    }

    // Abstract method to be overridden to handle action completion logic
    public abstract bool PrePerform();
    // Abstract method to be overridden to handle action completion logic
    public abstract bool PostPerform();
}
