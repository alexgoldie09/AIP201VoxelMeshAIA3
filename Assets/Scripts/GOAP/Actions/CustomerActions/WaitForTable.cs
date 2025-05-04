/*
 * WaitForTable.cs
 * ---------------
 * This class represents the GOAP action where the customer checks in and waits for a seat.
 * 
 * Extras:
 * - If a seat is found, it is reserved and the customer is assigned to it.
 * - If none are available, fallback logic is triggered to leave the restaurant.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitForTable : GAction
{
    private GameObject receptionStaff;

    /*
     * PrePerform() is the actions performed before the agent begins moving to its destination.
     * - Searches for available seat via GWorld
     * - If one exists, reserves it and assigns it to the customer
     * - If none exist, modifies beliefs and removes seating goals
     * - Begins coroutine to rotate toward reception staff for immersion
     */
    public override bool PrePerform()
    {
        Debug.Log("Finding reception staff...");
        agent.updateRotation = false;
        receptionStaff = GameObject.FindWithTag("ReceptionStaff");

        GameObject seat = GWorld.Instance.PeekAnySeat();

        if (seat == null)
        {
            Customer customer = GetComponent<Customer>();
            if (customer != null)
            {
                customer.beliefs.ModifyState("RestaurantFull", 1);
                customer.RemoveGoal("isWaiting");
                customer.RemoveGoal("isSeated");
            }
            return false;
        }

        if (seat.TryGetComponent(out Seat seatComp))
        {
            if (seatComp.isReserved)
            {
                return false;
            }

            seatComp.isReserved = true;

            Customer customer = GetComponent<Customer>();
            if (customer != null)
            {
                customer.assignedSeat = seat;
                customer.tableNumber = seatComp.tableNumber;
                beliefs.ModifyState("RestaurantAvailable", 1);
            }
        }
        else
        {
            return false;
        }

        GWorld.Instance.AddCustomer(gameObject);
        beliefs.ModifyState("atRestaurant", 1);

        StartCoroutine(TurnAgent());
        running = true;
        Invoke(nameof(CompleteAction), duration);

        return true;
    }

    /*
     * PostPerform() is the actions performed after the agent has reached it's destination.
     * - Marks the action as complete
     */
    public override bool PostPerform()
    {
        running = false;
        return true;
    }

    /*
     * CompleteAction() is called to finish the current action.
     * - Called by Invoke() after a short wait to simulate queueing
     * - Reduces the count of free seats in the world state
     */
    private void CompleteAction()
    {
        GWorld.Instance.GetWorld().ModifyState("Free_Seat", -1);
        running = false;
    }

    /*
     * TurnAgent() is a Coroutine that rotates the customer to face the reception staff over 1 second.
     * - Adds immersion and simulates waiting in line.
     */
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

                transform.rotation = lookRotation;
            }
        }

        yield return new WaitForSeconds(duration);
    }
}
