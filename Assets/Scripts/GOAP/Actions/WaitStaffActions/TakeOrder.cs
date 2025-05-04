/*
 * TakeOrder.cs
 * ------------
 * This class represents the GOAP action that the wait staff records a seated customer's food order.
 *
 * Extras:
 *  - Moves to that customer’s table (this is only done because the actions require movement else the agent will move somewhere else randomly).
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakeOrder : GAction
{
    private Customer customer;
    private GameObject seat;

    /*
     * PrePerform() is the actions performed before the agent begins moving to its destination.
     * - Locates the seat (stored earlier by ServeSeatedCustomer)
     * - Finds the matching customer using the seat
     * - Navigates to the seat’s StaffSpot to simulate interaction.
     */
    public override bool PrePerform()
    {
        seat = inventory.FindItemWithName("Resource");
        if (seat == null)
        {
            return false;
        }

        foreach (GameObject c in GameObject.FindGameObjectsWithTag("Customer"))
        {
            Customer cust = c.GetComponent<Customer>();
            if (cust != null && cust.assignedSeat == seat)
            {
                customer = cust;
                break;
            }
        }

        if (customer == null)
        {
            return false;
        }

        target = new GameObject("TempCustomerTarget");
        target.transform.position = seat.GetComponent<Seat>().staffSpot.transform.position;
        agent.SetDestination(target.transform.position);
        
        return true;
    }

    /*
     * PostPerform() is the actions performed after the agent has reached it's destination.
     * - Generates a random order and adds it to GWorld and the customer’s inventory
     * - Updates world state to reflect pending food
     * - Cleans up temp objects and inventory
     */
    public override bool PostPerform()
    {
        if (target != null)
        {
            Destroy(target);
        }

        inventory.RemoveItem(seat);

        Order order = new Order(Random.Range(1000, 9999)); // Random ticket number

        int numberOfItems = Random.Range(1, 4); // Randomly 1 to 3 items per order

        for (int i = 0; i < numberOfItems; i++)
        {
            var item = MenuManager.Instance.GetRandomMenuItem();
            if (!order.foodItems.ContainsKey(item.Key))
            {
                order.AddItem(item.Key, item.Value);
            }
            else
            {
                i--; // Retry if duplicate
            }
        }

        GWorld.Instance.AddOrder(order);

        ////Optional Debug print
        //string orderedItems = "";

        //foreach (var item in order.foodItems)
        //{
        //    orderedItems += $"{item.Value}, ";
        //}

        //orderedItems = orderedItems.TrimEnd(',', ' '); // Remove last comma

        //Debug.Log($"[TakeOrder] Created order #{order.ticketNumber} with {order.foodItems.Count} item(s): {orderedItems}");

        if (customer != null)
        {
            customer.inventory.AddOrder(order);
            customer.beingServed = false;
        }

        GWorld.Instance.GetWorld().ModifyState("BeingServed", -1);
        GWorld.Instance.GetWorld().ModifyState("WaitingOnFood", 1);

        return true;
    }
}
