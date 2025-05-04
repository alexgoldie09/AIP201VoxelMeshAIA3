/*
 * ServeSeatedCustomer.cs
 * -----------------------
 * This class represents the GOAP action that waitstaff move to a seated customer and prepare to take their order.
 */

using UnityEngine;

public class ServeSeatedCustomer : GAction
{
    private GameObject resource;

    /*
     * PrePerform() is the actions performed before the agent begins moving to its destination.
     * - Clears idle flag and sets the stopping distance for a direct approach
     * - Pulls a customer from the queue and checks if they are seated and not already being served
     * - Marks the customer as "beingServed"
     * - Navigates to the customer's assigned seat’s StaffSpot
     * - Adds the seat to the waitstaff’s inventory
     */
    public override bool PrePerform()
    {
        agent.stoppingDistance = 0;
        thisAgent.inIdle = false;

        target = GWorld.Instance.RemoveCustomer();
        if (target == null)
        {
            return false;
        }

        Customer customer = target.GetComponent<Customer>();

        if(!customer.isSeated)
        {
            GWorld.Instance.AddCustomer(target); // Re-queue if they’re not ready
            return false;
        }

        if (customer.assignedSeat != null && !customer.beingServed)
        {
            resource = customer.assignedSeat;
            resource.name = "Resource";
            customer.beingServed = true;

            inventory.AddItem(resource);

            Transform staffSpot = resource.transform.Find("StaffSpot");

            if (staffSpot != null)
            {
                target = new GameObject("TempStaffTarget");
                target.transform.position = staffSpot.position;
                agent.SetDestination(target.transform.position);
                return true;
            }
        }
        return false;
    }

    /*
     * PostPerform() is the actions performed after the agent has reached it's destination.
     * - Destroys temporary target
     * - Updates the world state to track how many customers are being served
     */
    public override bool PostPerform()
    {
        if (target != null)
        {
            Destroy(target);
        }

        GWorld.Instance.GetWorld().ModifyState("ReadyToOrder", -1);
        GWorld.Instance.GetWorld().ModifyState("BeingServed", 1);
        return true;
    }
}
