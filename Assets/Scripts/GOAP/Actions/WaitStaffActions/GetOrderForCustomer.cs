/*
 * GetOrderForCustomer.cs
 * ----------------------
 * This class represents the GOAP action where the wait staff picks up a completed meal from the kitchen for delivery.
 */

using UnityEngine;

public class GetOrderForCustomer : GAction
{
    private Order orderPicked;

    /*
     * PrePerform() is the actions performed before the agent begins moving to its destination.
     * - Clears idle flag and sets the stopping distance for a direct approach
     * - Retrieves a ready order from the kitchen’s ready queue
     * - Stores it in the inventory for delivery
     * - Updates world state to reflect that the agent is ready to deliver
     */
    public override bool PrePerform()
    {
        agent.stoppingDistance = 0;
        thisAgent.inIdle = false;

        orderPicked = GWorld.Instance.RemoveReadyOrder();
        if (orderPicked == null)
        {
            return false;
        }

        GWorld.Instance.GetWorld().ModifyState("FoodReadyToDeliver", -1);

        inventory.AddOrder(orderPicked);

        // Debug.Log($"[GetOrderForCustomer] Picked order #{orderPicked.ticketNumber}");

        return true;
    }

    /*
     * PostPerform() is the actions performed after the agent has reached it's destination.
     * - Notifies the world state that wait staff has the food in hand
     */
    public override bool PostPerform()
    {
        running = false;
        GWorld.Instance.GetWorld().ModifyState("FoodInHand", 1);
        return true;
    }
}
