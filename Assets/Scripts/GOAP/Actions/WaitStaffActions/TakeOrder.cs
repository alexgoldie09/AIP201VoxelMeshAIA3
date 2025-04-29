using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakeOrder : GAction
{
    private Customer customer;
    private GameObject seat;

    public override bool PrePerform()
    {
        // Get the seat from inventory (should have been stored earlier)
        seat = inventory.GetItem("Resource");
        if (seat == null)
        {
            Debug.LogWarning("[TakeOrder] No seat found in inventory.");
            return false;
        }

        // Find the customer associated with that seat
        GameObject[] allCustomers = GameObject.FindGameObjectsWithTag("Customer");
        foreach (var c in allCustomers)
        {
            Customer cust = c.GetComponent<Customer>();
            if (cust != null && cust.assignedSeat == seat)
            {
                customer = cust;
                break;
            }
        }

        target = new GameObject("TempCustomerTarget");
        target.transform.position = seat.GetComponent<Seat>().staffSpot.transform.position;

        agent.SetDestination(target.transform.position);

        if (customer == null)
        {
            Debug.LogWarning("[TakeOrder] No customer assigned to this seat.");
            return false;
        }


        
        return true;
    }

    public override bool PostPerform()
    {
        // Clean up the dummy target object if desired
        if (target != null)
        {
            Destroy(target);
        }

        // Remove resource
        inventory.RemoveItem(seat);

        // Generate Order
        Order order = new Order(Random.Range(1000, 9999)); // Random ticket number

        int numberOfItems = Random.Range(1, 4); // Randomly 1 to 3 items per order

        // Pick random menu items
        for (int i = 0; i < numberOfItems; i++)
        {
            var randomItem = MenuManager.Instance.GetRandomMenuItem();
            if (!randomItem.Equals(default(KeyValuePair<int, string>)))
            {
                // Add random item to order
                if (!order.foodItems.ContainsKey(randomItem.Key))
                {
                    order.AddItem(randomItem.Key, randomItem.Value);
                }
                else
                {
                    i--; // If duplicate item, re-roll
                }
            }
        }

        // Add order to GWorld for the kitchen
        GWorld.Instance.AddOrder(order);

        // Debug print
        string orderedItems = "";

        foreach (var item in order.foodItems)
        {
            orderedItems += $"{item.Value}, ";
        }

        orderedItems = orderedItems.TrimEnd(',', ' '); // Remove last comma

        Debug.Log($"[TakeOrder] Created order #{order.ticketNumber} with {order.foodItems.Count} item(s): {orderedItems}");

        // Store the order inside the customer too
        if (customer != null)
        {
            customer.inventory.AddOrder(order);
            customer.beingServed = false;
            Debug.Log($"[TakeOrder] Customer {customer.name} is no longer being served.");
        }

        // Modify world state
        GWorld.Instance.GetWorld().ModifyState("BeingServed", -1);
        GWorld.Instance.GetWorld().ModifyState("WaitingOnFood", 1);

        Debug.Log("[TakeOrder] Order taken. Updated world state to WaitingOnFood.");

        return true;
    }

    private void CompleteAction()
    {
        running = false;
    }

    private IEnumerator TurnAgent()
    {
        if (customer != null)
        {
            Vector3 direction = customer.transform.position - transform.position;
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
