using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SubGoal
{
    public WorldState sGoals;
    public bool remove;

    public SubGoal(string _newName, int _newIndex, bool _remove)
    {
        sGoals = new WorldState();
        sGoals.Add(_newName, _newIndex);
        remove = _remove;
    }
}

public class GAgent : MonoBehaviour
{
    [Header("Actions")]
    public List<GAction> actions = new List<GAction>();
    public GAction currentAction;

    public Dictionary<SubGoal, int> goals = new Dictionary<SubGoal, int>();
    private SubGoal currentGoal;


    public GInventory inventory = new GInventory();
    public WorldStates beliefs = new WorldStates();

    private GPlanner planner;
    private Queue<GAction> actionQueue;

    private bool invoked = false;
    private bool waitingForReplan = false;
    private float replanCooldown = 0.25f;  // Seconds to wait before replanning
    private float replanTimer = 0f;
    public bool inIdle = false;

    // Start is called before the first frame update
    public virtual void Start()
    {
        GAction[] acts = this.GetComponents<GAction>();
        foreach (GAction act in acts)
        {
            actions.Add(act);
            //Debug.Log($"[{name}] Added action: {act.GetType().Name}");
        }
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        if (waitingForReplan)
        {
            replanTimer -= Time.deltaTime;
            if (replanTimer <= 0f)
            {
                waitingForReplan = false;
            }
            else
            {
                return; // Still waiting, skip replanning this frame
            }
        }

        if (currentAction != null && currentAction.running)
        {
            // Check if agent arrived at destination
            if (!currentAction.agent.pathPending && currentAction.agent.remainingDistance <= currentAction.agent.stoppingDistance + 0.5f)
            {
                if (!invoked)
                {
                    Invoke("CompleteAction", currentAction.duration); // Wait duration
                    invoked = true;
                }
            }
            return;
        }

        // If actionQueue finished all actions
        if (actionQueue != null && actionQueue.Count == 0)
        {
            if (currentGoal != null && currentGoal.remove)
            {
                goals.Remove(currentGoal);
            }

            currentGoal = null;
            planner = null;
            actionQueue = null;
        }

        // Now check if replanning is allowed
        if (planner == null || actionQueue == null)
        {
            //Debug.Log($"[{name}] Planning goals:");
            //foreach (var goal in goals)
            //{
            //    foreach (var kvp in goal.Key.sGoals)
            //        Debug.Log($"[{name}] Goal: {kvp.Key} = {kvp.Value}");
            //}

            planner = new GPlanner();

            var sortedGoals = from entry in goals orderby entry.Value descending select entry;

            foreach (KeyValuePair<SubGoal, int> sg in sortedGoals)
            {
                var plan = planner.Plan(actions, sg.Key.sGoals, beliefs);

                if (plan != null)
                {
                    actionQueue = plan;
                    currentGoal = sg.Key;
                    break;
                }
            }
        }

        //if (actionQueue != null && actionQueue.Count == 0)
        //{
        //    if (currentGoal.remove)
        //    {
        //        goals.Remove(currentGoal);
        //    }
        //    planner = null;
        //}

        if (actionQueue != null && actionQueue.Count > 0)
        {
            currentAction = actionQueue.Dequeue();
            if (currentAction.PrePerform())
            {
                if(inIdle)
                {
                    return;
                }

                if (currentAction.target == null && currentAction.targetTag != "")
                {
                    currentAction.target = GameObject.FindWithTag(currentAction.targetTag);
                }

                if (currentAction.target != null)
                {
                    currentAction.running = true;
                    currentAction.agent.SetDestination(currentAction.target.transform.position);
                }
            }
            else 
            {
                actionQueue = null;
            }
        }
    }

    private void CompleteAction()
    {
        currentAction.running = false;
        currentAction.PostPerform();
        invoked = false;

        waitingForReplan = true;
        replanCooldown = Random.Range(0.4f, 1.2f);
        replanTimer = replanCooldown; // start countdown
    }

    public void RemoveGoal(string goalKey)
    {
        SubGoal targetGoal = null;

        foreach (var goal in goals.Keys)
        {
            if (goal.sGoals.ContainsKey(goalKey))
            {
                targetGoal = goal;
                break;
            }
        }

        if (targetGoal != null)
        {
            goals.Remove(targetGoal);
            Debug.Log($"[{name}] Removed goal: {goalKey}");
        }
        else
        {
            Debug.LogWarning($"[{name}] Tried to remove non-existing goal: {goalKey}");
        }
    }
}
