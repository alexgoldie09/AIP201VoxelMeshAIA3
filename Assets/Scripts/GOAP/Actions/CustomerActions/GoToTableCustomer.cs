/*
 * GoToTableCustomer.cs
 * --------------------
 * This class represents the GOAP action that moves the customer to their assigned seat.
 */

using UnityEngine;

public class GoToTableCustomer : GAction
{
    private Customer customer;

    /*
     * PrePerform() is the actions performed before the agent begins moving to its destination.
     * - Clears idle flag and sets the stopping distance for a direct approach
     * - Retrieves the customer’s assigned seat
     * - Creates a temporary target at the seat’s position
     * - Navigates the customer to that location
     */
    public override bool PrePerform()
    {
        agent.stoppingDistance = 0;
        thisAgent.inIdle = false;
        agent.updateRotation = true;

        customer = GetComponent<Customer>();
        if (customer == null || customer.assignedSeat == null)
        {
            return false;
        }

        target = new GameObject("TempCustomerTarget");
        target.transform.position = customer.assignedSeat.transform.position;

        agent.SetDestination(target.transform.position);

        return true;
    }

    /*
     * PostPerform() is the actions performed after the agent has reached it's destination.
     * - Destroys the temporary target
     * - Marks the customer as seated and notifies the world state that the customer is ready to order
     */
    public override bool PostPerform()
    {
        if (target != null)
        {
            Destroy(target);
        }
        GWorld.Instance.GetWorld().ModifyState("ReadyToOrder", 1);
        customer.isSeated = true;
        return true;
    }
}
