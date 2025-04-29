using UnityEngine;

public class GoToTableCustomer : GAction
{
    private Customer customer;

    public override bool PrePerform()
    {
        agent.stoppingDistance = 0;
        thisAgent.inIdle = false;
        agent.updateRotation = true;

        customer = GetComponent<Customer>();
        if (customer == null || customer.assignedSeat == null)
        {
            Debug.LogWarning($"{name} has no assigned seat.");
            return false;
        }

        target = new GameObject("TempCustomerTarget");
        target.transform.position = customer.assignedSeat.transform.position;

        agent.SetDestination(target.transform.position);
        //Debug.Log($"{name} assigned to seat {target.name}, destination: {agent.destination}");

        return true;
    }

    public override bool PostPerform()
    {
        // Clean up the dummy target object if desired
        if (target != null)
        {
            Destroy(target);
        }
        GWorld.Instance.GetWorld().ModifyState("ReadyToOrder", 1);
        customer.isSeated = true;
        return true;
    }
}
