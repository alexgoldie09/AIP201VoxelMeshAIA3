using System.Collections;
using UnityEngine;

public class CookOrder : GAction
{
    private GameObject cookTopObject;
    private Order currentOrder;

    public override bool PrePerform()
    {
        agent.stoppingDistance = 0;
        thisAgent.inIdle = false;

        // Get first Order from typed inventory
        currentOrder = inventory.GetFirstOrder();

        if (currentOrder == null)
        {
            return false;
        }

        // Try to claim a free cooktop
        cookTopObject = GWorld.Instance.RemoveCookTop();
        if (cookTopObject == null)
        {
            return false;
        }

        // Mark cooktop as occupied
        CookTop top = cookTopObject.GetComponent<CookTop>();
        if (top != null)
        {
            top.isOccupied = true;
            GWorld.Instance.GetWorld().ModifyState("Free_CookTop", -1);
        }

        // Modify world states
        GWorld.Instance.GetWorld().ModifyState("OrderReadyToCook", -1);
        GWorld.Instance.GetWorld().ModifyState("OrderCooking", 1);

        // Set target
        target = cookTopObject;

        Debug.Log($"[CookOrder] Moving to cook at cooktop: {cookTopObject.name}");

        return true;
    }

    public override bool PostPerform()
    {
        running = false;

        // Free the cooktop again
        if (cookTopObject != null)
        {
            CookTop top = cookTopObject.GetComponent<CookTop>();
            if (top != null)
            {
                top.isOccupied = false;
            }

            // Re-add cooktop to available queue
            GWorld.Instance.AddCookTop(cookTopObject);
            GWorld.Instance.GetWorld().ModifyState("Free_CookTop", 1);
        }

        // Finished cooking
        GWorld.Instance.GetWorld().ModifyState("OrderCooking", -1);

        Debug.Log($"[CookOrder] Finished cooking Order #{currentOrder.ticketNumber}. Ready for delivery.");

        // Remove the cooked order from inventory if you want
        if (currentOrder != null)
        {
            inventory.RemoveOrder(currentOrder);
            GWorld.Instance.AddReadyOrder(currentOrder);
            Debug.Log($"[CookOrder] Removed cooked order #{currentOrder.ticketNumber} from inventory and added to ready orders");
        }
        Debug.Log("[CookOrder] Cooking completed!");

        return true;
    }
}
