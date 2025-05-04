/*
 * GAgent.cs
 * ---------
 * This script is the core agent behaviour handling dynamic planning, sequential action execution, goal selection, and prioritization.
 *
 * Tasks:
 *  - The SubGoal class represents a single goal with value and optional "one-time" removal behavior.
 *  - The GAgent class is the core agent controller that works via:
 *      - Start(), gathering all attached GAction components and storing them.
 *      - LateUpdate(), evaluating if replanning is needed or if actions can execute.
 *      - Each action completion, triggering a post-effects and replanning cooldown.
 *      - Maintaining a goal queue (Dictionary<SubGoal, int>) prioritized by weight.
 *
 * Extras:
 *  - Supports replanning delay to avoid over-calculating every frame.
 *  - Allows dynamic goal removal (e.g. when a condition is no longer needed).
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Reflection;

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
    public List<GAction> actions = new List<GAction>();   // All possible actions available to this agent
    public GAction currentAction;                         // Action currently being executed

    public Dictionary<SubGoal, int> goals = new();        // Set of possible goals with associated priority
    private SubGoal currentGoal;                          // Currently active goal (being pursued)

    public GInventory inventory = new GInventory();       // Inventory component used by actions
    public WorldStates beliefs = new WorldStates();       // Beliefs/knowledge the agent holds

    private GPlanner planner;                             // Planner used to generate action sequences
    private Queue<GAction> actionQueue;                   // Queue of planned actions to execute

    private bool invoked = false;                         // Ensures CompleteAction() is only invoked once
    private bool waitingForReplan = false;                // Tracks if replanning delay is active
    private float replanCooldown = 0.25f;                 // Cooldown in seconds before replanning
    private float replanTimer = 0f;                       // Internal timer for cooldown
    public bool inIdle = false;                           // Optional flag for freezing the agent manually


    /* 
     * Start() is called once when the agent initializes
     * - Finds all attached GAction components and registers them to this agent
     */
    public virtual void Start()
    {
        GAction[] acts = GetComponents<GAction>();
        foreach (GAction act in acts)
        {
            actions.Add(act);
        }
    }

    /* 
     * LateUpdate() runs every frame AFTER all Update() calls
     * - Handles action execution, pathing, goal selection, and replanning logic 
    */
    private void LateUpdate()
    {
        // Handle replanning delay timer
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

        // Check if current action is running and has reached destination
        if (currentAction != null && currentAction.running)
        {
            if (!currentAction.agent.pathPending && currentAction.agent.remainingDistance <= currentAction.agent.stoppingDistance + 0.5f)
            {
                if (!invoked)
                {
                    // Begin the action duration wait before completion
                    Invoke("CompleteAction", currentAction.duration);
                    invoked = true;
                }
            }
            return;  // Wait for action to finish
        }

        // If no actions left in queue, reset planner state
        if (actionQueue != null && actionQueue.Count == 0)
        {
            if (currentGoal != null && currentGoal.remove)
            {
                goals.Remove(currentGoal); // Remove goal if flagged for one-time use
            }

            currentGoal = null;
            planner = null;
            actionQueue = null;
        }

        // Attempt to replan if needed
        if (planner == null || actionQueue == null)
        {
            ////Optional Debug for printing goal
            //Debug.Log($"[{name}] Planning goals:");
            //foreach (var goal in goals)
            //{
            //    foreach (var kvp in goal.Key.sGoals)
            //        Debug.Log($"[{name}] Goal: {kvp.Key} = {kvp.Value}");
            //}

            planner = new GPlanner();

            // Sort goals by priority descending
            var sortedGoals = from entry in goals orderby entry.Value descending select entry;

            foreach (var sg in sortedGoals)
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

        // Execute next action in plan queue
        if (actionQueue != null && actionQueue.Count > 0)
        {
            currentAction = actionQueue.Dequeue();
            // Attempt to perform any setup logic before executing (e.g., animations, resource claims)
            if (currentAction.PrePerform())
            {
                if (inIdle) { return; } // Prevent execution if manually paused

                // Resolve target via tag if not already assigned
                if (currentAction.target == null && currentAction.targetTag != "")
                {
                    currentAction.target = GameObject.FindWithTag(currentAction.targetTag);
                }

                // Begin navigation to the action target
                if (currentAction.target != null)
                {
                    currentAction.running = true;
                    currentAction.agent.SetDestination(currentAction.target.transform.position);
                }
            }
            else 
            {
                actionQueue = null; // If pre - perform fails(e.g., missing preconditions), abandon queue
            }
        }
    }

    /* 
     * CompleteActions() completes the currently running action.
     * - Calls PostPerform() to apply effects or cleanup
     * - Starts replan cooldown to prevent immediate new plans
    */
    private void CompleteAction()
    {
        currentAction.running = false;

        // Apply effects or other logic
        currentAction.PostPerform();
        invoked = false;

        // Start replanning cooldown
        waitingForReplan = true;
        replanCooldown = Random.Range(0.4f, 1.2f);
        replanTimer = replanCooldown;
    }

    /* 
     * RemoveGoal() removes a goal from the agent’s goal dictionary by key.
     * - Searches through each SubGoal's keys to find a match
    */
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
            // Debug.Log($"[{name}] Removed goal: {goalKey}");
        }
        else
        {
            Debug.LogWarning($"[{name}] Tried to remove non-existing goal: {goalKey}");
        }
    }
}
