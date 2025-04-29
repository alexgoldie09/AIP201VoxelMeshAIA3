using UnityEngine;

public class GetOrderForCustomer : GAction
{
    private Order orderPicked;

    public override bool PrePerform()
    {
        agent.stoppingDistance = 0;
        thisAgent.inIdle = false;

        orderPicked = GWorld.Instance.RemoveReadyOrder();
        if (orderPicked == null)
        {
            Debug.LogWarning("[GetOrderForCustomer] No ready order found!");
            return false;
        }

        GWorld.Instance.GetWorld().ModifyState("FoodReadyToDeliver", -1);

        // Store it to inventory
        inventory.AddOrder(orderPicked);

        Debug.Log($"[GetOrderForCustomer] Picked order #{orderPicked.ticketNumber}");

        return true;
    }

    public override bool PostPerform()
    {
        running = false;
        GWorld.Instance.GetWorld().ModifyState("FoodInHand", 1);
        return true;
    }
}
