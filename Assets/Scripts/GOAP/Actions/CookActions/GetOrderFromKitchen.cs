/*
 * GetOrderFromKitchen.cs
 * -----------------------
 * This class represents the GOAP action for a cook to collect an order from the kitchen queue.
 */

using System.Collections;
using UnityEngine;

public class GetOrderFromKitchen : GAction
{
    private Order pickedOrder;

    /*
     * PrePerform() is the actions performed before the agent begins moving to its destination.
     * - Clears idle flag and sets the stopping distance for a direct approach
     * - Attempts to retrieve the next order from the global queue
     * - Adds it to the agent’s inventory
     * - Adjusts world state to reflect kitchen queue change
     */
    public override bool PrePerform()
    {
        agent.stoppingDistance = 0;
        thisAgent.inIdle = false;

        pickedOrder = GWorld.Instance.RemoveOrder();
        if (pickedOrder == null)
        {
            return false;
        }

        GWorld.Instance.GetWorld().ModifyState("OrderAtKitchen", -1);
        inventory.AddOrder(pickedOrder);

        //Debug.Log($"[GetOrderFromKitchen] Picked up order #{pickedOrder.ticketNumber} with {pickedOrder.foodItems.Count} item(s).");

        return true;
    }

    /*
     * PostPerform() is the actions performed after the agent has reached it's destination.
     * - Notifies the world state that the order is ready to cook
     */
    public override bool PostPerform()
    {
        GWorld.Instance.GetWorld().ModifyState("OrderReadyToCook", 1);

        return true;
    }
}
