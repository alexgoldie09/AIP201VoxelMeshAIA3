using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class GAction : MonoBehaviour
{
    public string actionName = "Action";
    public float cost = 1.0f;
    public GameObject target;
    public GameObject targetTag;
    public float duration = 0;
    public WorldState preConditions;
    public WorldState afterEffects;
    public NavMeshAgent agent;

    public WorldStates agentBeliefs;

    public bool running = false;

    public void Awake()
    {
        agent = this.gameObject.GetComponent<NavMeshAgent>();
    }

    public bool IsAchievable() => true;

    public bool IsAchievableGiven(WorldState conditions)
    {
        if (preConditions == null) return true;

        foreach (SerializableKeyValuePair<string, int> pair in preConditions)
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
