using UnityEngine;

public class ServeSeatedCustomer : GAction
{
    private GameObject resource;

    public override bool PrePerform()
    {
        agent.stoppingDistance = 0;
        thisAgent.inIdle = false;

        // Get the next customer from the waiting queue
        target = GWorld.Instance.RemoveCustomer();
        if (target == null)
        {
            //Debug.LogWarning("No customer found in the queue.");
            return false;
        }

        // Get the customer's assigned seat
        Customer customer = target.GetComponent<Customer>();

        if(!customer.isSeated)
        {
            GWorld.Instance.AddCustomer(target);
            return false;
        }

        if (customer != null && customer.assignedSeat != null && !customer.beingServed)
        {
            resource = customer.assignedSeat;
            resource.name = "Resource";
            customer.beingServed = true; // Claim customer immediately

            // Add seat to wait staff's inventory
            inventory.AddItem(resource);

            // Find the StaffSpot offset near their seat
            Transform staffSpot = resource.transform.Find("StaffSpot");

            if (staffSpot != null)
            {
                // Create dummy target
                target = new GameObject("TempStaffTarget");
                target.transform.position = staffSpot.position;
                agent.SetDestination(target.transform.position);

                Debug.Log($"[WaitStaff] Going to serve seated customer: {customer.name} at seat {resource.name}");
                return true;
            }
        }
        return false;
    }

    public override bool PostPerform()
    {
        if (target != null)
        {
            Destroy(target);
        }

        // (Optional) Mark customer as "BeingServed" if you want
        GWorld.Instance.GetWorld().ModifyState("ReadyToOrder", -1);
        GWorld.Instance.GetWorld().ModifyState("BeingServed", 1);
        return true;
    }
}
