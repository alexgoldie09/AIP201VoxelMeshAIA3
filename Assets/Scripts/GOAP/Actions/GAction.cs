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

    public virtual void Awake()
    {
        agent = this.gameObject.GetComponent<NavMeshAgent>();
        thisAgent = this.GetComponent<GAgent>();
        inventory = thisAgent.inventory;
        beliefs = thisAgent.beliefs;
    }

    public bool IsAchievable() => true;

    public bool IsAchievableGiven(WorldState conditions)
    {
        if (preConditions == null) return true;

        foreach (KeyValuePair<string, int> pair in preConditions.GetLivePairs())
        {
            if(!conditions.ContainsKey(pair.Key) || conditions[pair.Key] < pair.Value)
            {
                return false;
            }
        }
        return true;
    }

    public abstract bool PrePerform();
    public abstract bool PostPerform();
}
