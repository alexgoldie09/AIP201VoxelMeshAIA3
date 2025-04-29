using UnityEngine;

public class GoToTableStaff : GAction
{
    public override bool PrePerform()
    {
        // Get the seat from the inventory (assumed to be added in GetCustomer.cs)
        GameObject seat = inventory.FindItemWithTag("Seat");
        if (seat == null)
        {
            Debug.LogWarning("Wait Staff: No seat found in inventory.");
            return false;
        }

        // Access the Seat component and its assigned StaffSpot
        if (!seat.TryGetComponent<Seat>(out Seat seatComp))
        {
            Debug.LogWarning("Wait Staff: Seat does not have a Seat component.");
            return false;
        }

        if (seatComp.staffSpot == null)
        {
            Debug.LogWarning("Wait Staff: StaffSpot is not assigned on the Seat.");
            return false;
        }

        // Create a temporary target object for NavMesh to navigate to
        target = new GameObject("TempStaffTarget");
        target.transform.position = seatComp.staffSpot.position;

        agent.SetDestination(target.transform.position);
        return true;
    }

    public override bool PostPerform()
    {
        // Clean up the dummy target object if desired
        if (target != null)
        {
            Destroy(target);
        }

        return true;
    }
}
