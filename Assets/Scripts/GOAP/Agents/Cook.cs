/*
 * CookAgent.cs
 * ------------
 * This class represents a GOAP agent responsible for preparing and finishing food orders.
 *
 * Tasks:
 *  - Registers a prioritised set of goals the agent will pursue using GOAP planning.
 *  - The cook handles creating ready-to-serve food and marking completed orders.
 *
 * Goals (in descending priority):
 *  - "orderReady": Prepare an order and begin cooking.
 *  - "finishedOrder": Mark an order as cooked or completed.
 *  - "Idle": Default state when no other tasks are valid.
 *
 * Extras:
 *  - All goals are non-removable (`remove = false`) so they persist across cycles.
 *  - Agent uses GActions like "CookOrder" or "FinishCooking" (not shown here).
 */

using UnityEngine;

public class CookAgent : GAgent
{
    public override void Start()
    {
        base.Start();

        // Goal 1: Successfully prepare an order
        SubGoal g1 = new SubGoal("orderReady", 1, false);
        // Goal 2: Mark finished cooking phase (optional mid-stage)
        SubGoal g2 = new SubGoal("finishedOrder", 1, false);
        // Goal 3: Fallback idle
        SubGoal g3 = new SubGoal("Idle", 1, false);

        goals.Add(g1, 5); // Highest priority
        goals.Add(g2, 4); // Medium priority
        goals.Add(g3, 1); // Fallback goal if nothing else is valid
    }
}