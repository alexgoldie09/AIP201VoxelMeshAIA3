/*
 * FinishOrder.cs
 * --------------
 * This class represents the GOAP action which has the cook bring the food to the expeditor, indicating that the food is done and ready for pickup.
 */

using System.Collections;
using UnityEngine;

public class FinishOrder : GAction
{
    /*
     * PrePerform() is the actions performed before the agent begins moving to its destination.
     * - No pre-action logic required here
     */
    public override bool PrePerform()
    {
        return true;
    }

    /*
     * PostPerform() is the actions performed after the agent has reached it's destination.
     * - Notifies the world state that the order is ready to deliver
     */
    public override bool PostPerform()
    {
        running = false;
        GWorld.Instance.GetWorld().ModifyState("FoodReadyToDeliver", 1);

        return true;
    }
}
