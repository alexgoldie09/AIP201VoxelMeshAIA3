using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitStaff : GAgent
{
    // Start is called before the first frame update
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
