/*
 * CookOrder.cs
 * ------------
 * This class represents the GOAP action for a cook to begin cooking the order assigned.
 */

using System.Collections;
using UnityEngine;

public class CookOrder : GAction
{
    private GameObject cookTopObject;
    private Order currentOrder;

    /*
     * PrePerform() is the actions performed before the agent begins moving to its destination.
     * - Clears idle flag and sets the stopping distance for a direct approach
     * - Retrieves the current order from inventory
     * - Reserves a free cooktop from the world and marks it as occupied
     * - Updates the world state to reflect that cooking has begun
     */
    public override bool PrePerform()
    {
        agent.stoppingDistance = 0;
        thisAgent.inIdle = false;

        currentOrder = inventory.GetFirstOrder();
        if (currentOrder == null)
        {
            return false;
        }

        cookTopObject = GWorld.Instance.RemoveCookTop();
        if (cookTopObject == null)
        {
            return false;
        }

        CookTop top = cookTopObject.GetComponent<CookTop>();
        if (top != null)
        {
            top.isOccupied = true;
            GWorld.Instance.GetWorld().ModifyState("Free_CookTop", -1);
        }

        GWorld.Instance.GetWorld().ModifyState("OrderReadyToCook", -1);
        GWorld.Instance.GetWorld().ModifyState("OrderCooking", 1);

        target = cookTopObject;

        //Debug.Log($"[CookOrder] Moving to cook at cooktop: {cookTopObject.name}");

        return true;
    }

    /*
     * PostPerform() is the actions performed after the agent has reached it's destination.
     * - Frees the cooktop and re-adds it to the world
     * - Removes the order from inventory and adds it to the "ready orders" list
     * - Updates the world state to reflect that cooking is finished
     */
    public override bool PostPerform()
    {
        running = false;

        if (cookTopObject != null)
        {
            CookTop top = cookTopObject.GetComponent<CookTop>();
            if (top != null)
            {
                top.isOccupied = false;
            }

            GWorld.Instance.AddCookTop(cookTopObject);
            GWorld.Instance.GetWorld().ModifyState("Free_CookTop", 1);
        }

        GWorld.Instance.GetWorld().ModifyState("OrderCooking", -1);

        //Debug.Log($"[CookOrder] Finished cooking Order #{currentOrder.ticketNumber}. Ready for delivery.");

        if (currentOrder != null)
        {
            inventory.RemoveOrder(currentOrder);
            GWorld.Instance.AddReadyOrder(currentOrder);
            //Debug.Log($"[CookOrder] Removed cooked order #{currentOrder.ticketNumber} from inventory and added to ready orders");
        }

        return true;
    }
}
