using System.Collections;
using UnityEngine;

public class GetOrderFromKitchen : GAction
{
    private Order pickedOrder;

    public override bool PrePerform()
    {
        // Get an order from kitchen queue
        pickedOrder = GWorld.Instance.RemoveOrder();
        if (pickedOrder == null)
        {
            return false;
        }

        // Update WorldState to reflect one less order
        GWorld.Instance.GetWorld().ModifyState("OrderAtKitchen", -1);

        // Store order into cook's inventory
        inventory.AddOrder(pickedOrder);

        Debug.Log($"[GetOrderFromKitchen] Picked up order #{pickedOrder.ticketNumber} with {pickedOrder.foodItems.Count} item(s).");

        agent.stoppingDistance = 0;
        thisAgent.inIdle = false;

        return true;
    }

    public override bool PostPerform()
    {
        // Update to next world state
        GWorld.Instance.GetWorld().ModifyState("OrderReadyToCook", 1);

        return true;
    }
}
