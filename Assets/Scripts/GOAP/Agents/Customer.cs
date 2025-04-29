using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Customer : GAgent
{
    public GameObject assignedSeat;
    public int tableNumber = -1;
    public bool beingServed = false;
    public bool isSeated = false;
    public Order deliveredOrder = null;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        // First, customer must wait to be served
        SubGoal s1 = new SubGoal("isWaiting", 1, true);
        goals.Add(s1, 3); // Higher priority

        // Go to the table
        SubGoal s2 = new SubGoal("isSeated", 1, true);
        goals.Add(s2, 2); // Lower priority — activates after isWaiting completes

        // Check order and go home
        SubGoal s3 = new SubGoal("isHome", 1, true);
        goals.Add(s3, 1); // Lowest priority — activates upon receiving food

        // Last, customer must wait to be served
        SubGoal s4 = new SubGoal("isFull", 1, true);
        goals.Add(s4, 10); // Highest priority to override any attempt to seat
    }
}
