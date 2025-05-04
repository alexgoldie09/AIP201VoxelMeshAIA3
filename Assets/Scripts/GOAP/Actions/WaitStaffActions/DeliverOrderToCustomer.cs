/*
 * DeliverOrderToCustomer.cs
 * -------------------------
 * This class represents the GOAP action where the wait staff delivers the meal to the correct customer.
 * 
 * Extras:
 * - There is a 40% chance of delivering the wrong meal (to simulate human error).
 */

using UnityEngine;

public class DeliverOrderToCustomer : GAction
{
    private Customer customerTarget;
    private Order orderToDeliver;

    /*
     * PrePerform() is the actions performed before the agent begins moving to its destination.
     * - Clears idle flag and sets the stopping distance for a direct approach
     * - Retrieves the current order from inventory
     * - Searches for the matching customer by comparing inventory orders
     * - Navigates to the StaffSpot of the customer’s seat
     */
    public override bool PrePerform()
    {
        agent.stoppingDistance = 0;
        thisAgent.inIdle = false;

        orderToDeliver = inventory.GetFirstOrder();

        if (orderToDeliver == null)
        {
            return false;
        }

        foreach (var obj in GameObject.FindGameObjectsWithTag("Customer"))
        {
            Customer cust = obj.GetComponent<Customer>();
            if (cust != null && cust.inventory.HasOrder(orderToDeliver))
            {
                customerTarget = cust;
                break;
            }
        }

        if (customerTarget == null || customerTarget.assignedSeat == null)
        {
            return false;
        }

        Transform staffSpot = customerTarget.assignedSeat.transform.Find("StaffSpot");
        if (staffSpot == null)
        {
            return false;
        }

        target = new GameObject("TempDeliverTarget");
        target.transform.position = staffSpot.position;
        agent.SetDestination(target.transform.position);

        // Debug.Log($"[DeliverOrderToCustomer] Heading to deliver Order #{orderToDeliver.ticketNumber} to {customerTarget.name}.");

        GWorld.Instance.GetWorld().ModifyState("FoodInHand", -1);

        return true;
    }

    /*
     * PostPerform() is the actions performed after the agent has reached it's destination.
     * - 40% chance of assigning an incorrect order
     * - Sets the deliveredOrder on the customer and modifies world state
     */
    public override bool PostPerform()
    {
        running = false;
        inventory.RemoveOrder(orderToDeliver);

        bool giveWrongOrder = Random.value < 0.4f;

        if (giveWrongOrder)
        {
            var wrongItem = MenuManager.Instance.GetRandomMenuItem();
            Order wrongOrder = new Order(Random.Range(1000, 9999));
            wrongOrder.AddItem(wrongItem.Key, wrongItem.Value);
            customerTarget.deliveredOrder = wrongOrder;
            //Debug.Log($"[DeliverOrderToCustomer] Delivered WRONG order to {customerTarget.name}!");
        }
        else
        {
            customerTarget.deliveredOrder = orderToDeliver;
            //Debug.Log($"[DeliverOrderToCustomer] Delivered correct order #{orderToDeliver.ticketNumber} to {customerTarget.name}");
        }

        customerTarget.beliefs.ModifyState("ReceivedFood", 1);
        GWorld.Instance.GetWorld().ModifyState("WaitingOnFood", -1);

        return true;
    }
}
