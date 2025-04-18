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

    private GPlanner planner;
    private Queue<GAction> actionQueue;

    private bool invoked = false;

    // Start is called before the first frame update
    public virtual void Start()
    {
        GAction[] acts = this.GetComponents<GAction>();
        foreach (GAction act in acts)
        {
            actions.Add(act);
        }
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        if (currentAction != null && currentAction.running)
        {
            if(currentAction.agent.hasPath && currentAction.agent.remainingDistance < 1f)
            {
                if(!invoked)
                {
                    Invoke("CompleteAction", currentAction.duration);
                    invoked = true;
                }
            }
            return;
        }

        if (planner == null || actionQueue == null)
        {
            planner = new GPlanner();

            var sortedGoals = from entry in goals orderby entry.Value descending select entry;

            foreach (KeyValuePair<SubGoal, int> sg in sortedGoals)
            {
                actionQueue = planner.Plan(actions, sg.Key.sGoals, null);
                if(actionQueue != null)
                {
                    currentGoal = sg.Key;
                    break;
                }
            }
        }

        if(actionQueue != null && actionQueue.Count == 0)
        {
            if(currentGoal.remove)
            {
                goals.Remove(currentGoal);
            }
            planner = null;
        }

        if(actionQueue != null && actionQueue.Count > 0)
        {
            currentAction = actionQueue.Dequeue();
            if (currentAction.PrePerform())
            {
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
    }
}
