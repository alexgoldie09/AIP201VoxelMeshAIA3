using UnityEngine;

public class CheckOrderGoHome : GAction
{
    private Order expectedOrder;
    private Order deliveredOrder;

    public override bool PrePerform()
    {
        agent.stoppingDistance = 0;
        thisAgent.inIdle = false;

        Customer customer = GetComponent<Customer>();
        if (customer == null)
        {
            Debug.LogWarning("[CheckOrderGoHome] No Customer component found.");
            return false;
        }

        // Get what the customer wanted
        expectedOrder = customer.inventory.GetFirstOrder();

        // Get what was delivered
        deliveredOrder = customer.deliveredOrder;

        if (expectedOrder == null || deliveredOrder == null)
        {
            Debug.LogWarning("[CheckOrderSatisfaction] Missing expected or delivered order!");
            return false;
        }

        // Compare the orders (ticket number OR contents)
        bool satisfied = AreOrdersMatching(expectedOrder, deliveredOrder);

        if (satisfied)
        {
            GWorld.Instance.GetWorld().ModifyState("Customer_Satisfied", 1);
            Debug.Log($"[CheckOrderGoHome] {customer.name} is satisfied with their order!");
        }
        else
        {
            GWorld.Instance.GetWorld().ModifyState("Customer_Unsatisfied", 1);
            Debug.LogWarning($"[CheckOrderGoHome] {customer.name} is UNSATISFIED with their order!");
        }

        if (customer.assignedSeat != null)
        {
            // Free up the seat
            Seat seat = customer.assignedSeat.GetComponent<Seat>();
            if (seat != null)
            {
                seat.isReserved = false;
            }

            // Add seat back
            GWorld.Instance.GetWorld().ModifyState("Free_Seat", 1);

            Debug.Log($"[CheckOrderGoHome] {customer.name} has freed up seat {customer.assignedSeat.name}.");
        }

        // Clear assignment
        customer.assignedSeat = null;
        customer.tableNumber = -1;

        return true;
    }

    public override bool PostPerform()
    {
        running = false;
        Debug.Log($"[CheckOrderGoHome] {gameObject.name} has returned home.");
        Destroy(gameObject);
        return true;
    }

    private bool AreOrdersMatching(Order expected, Order delivered)
    {
        // Quick check: Same ticket number
        if (expected.ticketNumber == delivered.ticketNumber)
            return true;

        // Deep check: Same food items
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
