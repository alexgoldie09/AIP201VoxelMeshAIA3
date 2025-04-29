using UnityEngine;

public class DeliverOrderToCustomer : GAction
{
    private Customer customerTarget;
    private Order orderToDeliver;

    public override bool PrePerform()
    {
        agent.stoppingDistance = 0;
        thisAgent.inIdle = false;

        // Get first Order from typed inventory
        orderToDeliver = inventory.GetFirstOrder();

        if (orderToDeliver == null)
        {
            return false;
        }

        // Find correct customer based on matching order
        GameObject[] allCustomers = GameObject.FindGameObjectsWithTag("Customer");
        foreach (var obj in allCustomers)
        {
            Customer cust = obj.GetComponent<Customer>();
            if (cust != null && cust.inventory.HasOrder(orderToDeliver))
            {
                customerTarget = cust;
                break;
            }
        }

        if (customerTarget == null)
        {
            Debug.LogWarning("[DeliverOrderToCustomer] No customer found for ticket!");
            return false;
        }

        // Modify world state
        GWorld.Instance.GetWorld().ModifyState("FoodInHand", -1);

        // Now find the StaffSpot
        if (customerTarget.assignedSeat == null)
        {
            Debug.LogWarning("[DeliverOrderToCustomer] Customer assigned seat missing!");
            return false;
        }

        Transform staffSpot = customerTarget.assignedSeat.transform.Find("StaffSpot");
        if (staffSpot == null)
        {
            Debug.LogWarning("[DeliverOrder] No StaffSpot child under assigned seat.");
            return false;
        }

        // Update the target to the StaffSpot location instead
        GameObject staffTarget = new GameObject("TempDeliverTarget");
        staffTarget.transform.position = staffSpot.position;
        target = staffTarget;

        agent.SetDestination(target.transform.position);

        Debug.Log($"[DeliverOrderToCustomer] Heading to deliver Order #{orderToDeliver.ticketNumber} to {customerTarget.name}.");

        return true;
    }

    public override bool PostPerform()
    {
        running = false;

        if (orderToDeliver != null)
        {
            inventory.RemoveOrder(orderToDeliver);

            // 40% chance to deliver incorrect order
            bool giveWrongOrder = Random.value < 0.4f;

            if (giveWrongOrder)
            {
                // Create a fake incorrect order
                Order wrongOrder = new Order(Random.Range(1000, 9999));
                var randomItem = MenuManager.Instance.GetRandomMenuItem();
                wrongOrder.AddItem(randomItem.Key, randomItem.Value);

                customerTarget.deliveredOrder = wrongOrder;
                Debug.Log($"[DeliverOrderToCustomer] Delivered WRONG order to {customerTarget.name}!");
            }
            else
            {
                customerTarget.deliveredOrder = orderToDeliver;
                Debug.Log($"[DeliverOrderToCustomer] Delivered correct order #{orderToDeliver.ticketNumber} to {customerTarget.name}");
            }
        }

        // After successful delivery, signal customer they received food
        customerTarget.beliefs.ModifyState("ReceivedFood", 1);

        // Finished order delivery
        GWorld.Instance.GetWorld().ModifyState("WaitingOnFood", -1);

        return true;
    }
}
