/*
 * WaitStaff.cs
 * ------------
 * This class represents a GOAP agent responsible for taking orders from customers, delivering food, and returning to idle when unoccupied.
 *
 * Tasks:
 *  - Collect customer orders and deliver them to the kitchen.
 *  - Retrieve ready food and deliver to correct customers.
 *  - Handles a loop of "take order -> deliver food -> idle".
 *
 * Goals (in descending priority):
 *  - "OrderDelivered": Take order from customer and bring it to kitchen.
 *  - "HoldingFood": Pick up ready meals from counter.
 *  - "DeliveredOrderToCustomer": Deliver meals to customers.
 *  - "Idle": Fallback when no goals are active.
 *
 * Extras:
 *  - All goals persist across frames (`remove = false`) to allow cyclical tasking.
 *  - Used in tandem with a dynamic world state, seats, orders, and customer needs.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitStaff : GAgent
{
    public override void Start()
    {
        base.Start();

        // Goal 1: Handle taking customer orders to kitchen
        SubGoal s1 = new SubGoal("OrderDelivered", 1, false);
        // Goal 2: Pick up ready food orders
        SubGoal s2 = new SubGoal("HoldingFood", 1, false);
        // Goal 3: Deliver picked-up food to customers
        SubGoal s3 = new SubGoal("DeliveredOrderToCustomer", 1, false);
        // Goal 4: Fallback idle
        SubGoal s4 = new SubGoal("Idle", 1, false);

        goals.Add(s1, 5); // High priority
        goals.Add(s2, 3); // Medium priority
        goals.Add(s3, 2); // Low priority
        goals.Add(s4, 1); // Lowest priority fallback
    }
}
