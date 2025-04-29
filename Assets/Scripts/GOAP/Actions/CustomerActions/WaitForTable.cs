using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitForTable : GAction
{
    private GameObject receptionStaff;

    public override bool PostPerform()
    {
        running = false;
        return true;
    }

    public override bool PrePerform()
    {
        Debug.Log("Finding reception staff...");
        agent.updateRotation = false;
        receptionStaff = GameObject.FindWithTag("ReceptionStaff");

        // Try to get an unreserved seat
        GameObject seat = GWorld.Instance.PeekAnySeat();

        if (seat == null)
        {
            Debug.LogWarning("No unreserved seat available at check-in.");
            // Set fallback belief to leave
            Customer customer = GetComponent<Customer>();
            if (customer != null)
            {
                customer.beliefs.ModifyState("RestaurantFull", 1);
                customer.RemoveGoal("isWaiting"); // Remove the goal to CheckIn
                customer.RemoveGoal("isSeated");  // (Optional) Remove sit-down goal if exists
                Debug.Log($"[{name}] Set Restaurant_Full belief - leaving.");
            }
            return false;
        }

        // Reserve the seat
        if (seat.TryGetComponent(out Seat seatComp))
        {
            if (seatComp.isReserved)
            {
                Debug.LogWarning("Somehow grabbed a seat that was already reserved.");
                return false;
            }

            seatComp.isReserved = true; // Mark as reserved

            Customer customer = GetComponent<Customer>();
            if (customer != null)
            {
                customer.assignedSeat = seat;
                customer.tableNumber = seatComp.tableNumber;
                beliefs.ModifyState("RestaurantAvailable", 1);
                Debug.Log($"Customer {name} assigned to table {customer.tableNumber} at seat {seat.name}");
            }
        }
        else
        {
            Debug.LogWarning("Seat does not have a Seat component.");
            return false;
        }

        // Add to world state
        GWorld.Instance.AddCustomer(this.gameObject);
        beliefs.ModifyState("atRestaurant", 1);

        StartCoroutine(TurnAgent());

        // Mark this action as running to block planner progression
        running = true;

        // Schedule action completion
        Invoke(nameof(CompleteAction), duration);

        return true;
    }

    private void CompleteAction()
    {
        GWorld.Instance.GetWorld().ModifyState("Free_Seat", -1);
        running = false;
    }

    private IEnumerator TurnAgent()
    {
        if (receptionStaff != null)
        {
            Vector3 direction = receptionStaff.transform.position - transform.position;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                Quaternion initialRotation = transform.rotation;

                float rotateDuration = 1f;
                float elapsed = 0f;

                while (elapsed < rotateDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / rotateDuration);
                    transform.rotation = Quaternion.Slerp(initialRotation, lookRotation, t);
                    yield return null;
                }

                transform.rotation = lookRotation; // snap to exact rotation
            }
        }

        yield return new WaitForSeconds(duration); // Optional: delay after rotating
    }
}
