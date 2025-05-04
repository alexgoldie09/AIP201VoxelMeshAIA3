/*
 * CheckOrderGoHome.cs
 * -------------------
 * This class represents the GOAP action for a customer to verify if the food delivered matches their original order,
 * then vacate their seat and exit the restaurant.
 *
 * Extras:
 *  - Removes itself after completion via `Destroy(gameObject)` (customer is removed).
 *  - Applies a satisfaction result to the global world state ("Customer_Satisfied", etc.).
 */

using UnityEngine;

public class CheckOrderGoHome : GAction
{
    private Order expectedOrder;
    private Order deliveredOrder;

    /*
     * PrePerform() is the actions performed before the agent begins moving to its destination.
     * - Retrieves the expected order from inventory and the delivered order from the customer
     * - Compares both orders
     * - Applies a satisfaction state to the global world
     * - Frees the reserved seat and resets the customer’s table values
     */
    public override bool PrePerform()
    {
        agent.stoppingDistance = 2f;
        thisAgent.inIdle = false;

        Customer customer = GetComponent<Customer>();
        if (customer == null)
        {
            return false;
        }

        expectedOrder = customer.inventory.GetFirstOrder();

        deliveredOrder = customer.deliveredOrder;

        if (expectedOrder == null || deliveredOrder == null)
        {
            return false;
        }

        bool satisfied = AreOrdersMatching(expectedOrder, deliveredOrder);

        if (satisfied)
        {
            GWorld.Instance.GetWorld().ModifyState("Customer_Satisfied", 1);
            Debug.Log($"[CheckOrderGoHome] {customer.name} is SATISFIED with their order!");
        }
        else
        {
            GWorld.Instance.GetWorld().ModifyState("Customer_Unsatisfied", 1);
            Debug.Log($"[CheckOrderGoHome] {customer.name} is UNSATISFIED with their order!");
        }

        if (customer.assignedSeat != null)
        {
            Seat seat = customer.assignedSeat.GetComponent<Seat>();
            if (seat != null)
            {
                seat.isReserved = false;
            }

            GWorld.Instance.GetWorld().ModifyState("Free_Seat", 1);
        }

        customer.assignedSeat = null;
        customer.tableNumber = -1;

        return true;
    }

    /*
     * PostPerform() is the actions performed after the agent has reached it's destination.
     * - Destroys the customer GameObject, simulating them leaving the scene
     */
    public override bool PostPerform()
    {
        running = false;
        Debug.Log($"[CheckOrderGoHome] {gameObject.name} has returned home.");
        Destroy(gameObject);
        return true;
    }

    /*
     * AreOrdersMatching() returns whether the two inputted orders match each other.
     * - Compares ticket number and food items between the expected and delivered order.
     * - Returns true if all match, false otherwise.
     */
    private bool AreOrdersMatching(Order expected, Order delivered)
    {
        if (expected.ticketNumber == delivered.ticketNumber)
            return true;

        if (expected.foodItems.Count != delivered.foodItems.Count)
            return false;

        foreach (var item in expected.foodItems)
        {
            if (!delivered.foodItems.TryGetValue(item.Key, out string value) || value != item.Value)
            {
                return false;
            }
        }

        return true;
    }
}
