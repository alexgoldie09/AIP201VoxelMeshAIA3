/*
 * Customer.cs
 * -----------
 * This class represents a GOAP agent acting as a restaurant customer.
 * 
 * Tasks:
 *  - Wait for staff, move to seating, eat food, and then leave.
 *  - Driven by goals that are progressively removed after completion.
 *
 * Goals (in priority order):
 *  - "isFull": Highest priority (restaurant is filled up so go home, overrides all else).
 *  - "isWaiting": Wait to be served (initial state).
 *  - "isSeated": Move to seat once one becomes available.
 *  - "isHome": Leave restaurant after eating.
 *
 * Extras:
 *  - All goals are set with `remove = true`, meaning they are one-time goals and removed when satisfied.
 *  - The customer’s flow is sequential: wait -> sit -> eat -> leave.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Customer : GAgent
{
    public GameObject assignedSeat;       // Reference to assigned seat GameObject
    public int tableNumber = -1;          // Assigned table number
    public bool beingServed = false;      // Indicates waiter is helping the customer
    public bool isSeated = false;         // Is the customer seated?
    public Order deliveredOrder = null;   // Stores their delivered food order

    public override void Start()
    {
        base.Start();
        // Goal 1: Wait at the front or entry
        SubGoal s1 = new SubGoal("isWaiting", 1, true);
        // Goal 2: Move to and sit at assigned table
        SubGoal s2 = new SubGoal("isSeated", 1, true);
        // Goal 3: Check order and go home
        SubGoal s3 = new SubGoal("isHome", 1, true);
        // Goal 4: Restuarant is full (takes top priority)
        SubGoal s4 = new SubGoal("isFull", 1, true);

        goals.Add(s1, 3); // Higher priority
        goals.Add(s2, 2); // Lower priority — activates after isWaiting completes
        goals.Add(s3, 1); // Lowest priority — activates upon receiving food
        goals.Add(s4, 10); // Highest priority to override any attempt to sit
    }
}
