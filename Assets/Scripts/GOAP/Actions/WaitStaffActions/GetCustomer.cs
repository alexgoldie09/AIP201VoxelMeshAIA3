using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetCustomer : GAction
{
    private GameObject resource;

    public override bool PostPerform()
    {
        GWorld.Instance.GetWorld().ModifyState("Waiting", -1);
        if(target)
        {
            target.GetComponent<GAgent>().inventory.AddItem(resource);
        }
        return true;
    }

    public override bool PrePerform()
    {
        // Get the next customer from the waiting queue
        target = GWorld.Instance.RemoveCustomer();
        if (target == null)
        {
            //Debug.LogWarning("No customer found in the queue.");
            return false;
        }

        // Get the customer's assigned seat
        Customer customer = target.GetComponent<Customer>();
        if (customer == null || customer.assignedSeat == null)
        {
            //Debug.LogWarning("Customer has no assigned seat.");
            return false;
        }

        resource = customer.assignedSeat;

        // Unreserve the seat now that it's actively being used
        if (resource.TryGetComponent(out Seat seatComp))
        {
            seatComp.isReserved = false;
        }

        // Add seat to wait staff's inventory
        inventory.AddItem(resource);

        Debug.Log($"[GetCustomer] Wait staff got target: {target.name}, seat: {resource.name} (Table {customer.tableNumber})");

        // Update world state
        GWorld.Instance.GetWorld().ModifyState("Free_Seat", -1);
        return true;
    }
}
