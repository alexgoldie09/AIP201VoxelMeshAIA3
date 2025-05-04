/*
 * Order.cs
 * --------
 * This class represents a food order.
 *
 * Tasks:
 *  - Holds a ticket number and dictionary of food items.
 *  - Used throughout GOAP actions (e.g., by WaitStaff and CookAgent).
 *
 * Extras:
 *  - Supports multiple food items per order (e.g., for combos or multi-item meals).
 *  - Can be added to inventory GInventory or queued in GWorld.
 */

using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Order
{
    public int ticketNumber; // Unique identifier per customer/order
    public Dictionary<int, string> foodItems = new Dictionary<int, string>(); // Dictionary of food items ordered (menu ID -> name)

    // Default Constructor
    public Order(int newticketNumber)
    {
        ticketNumber = newticketNumber;
    }

    /* 
     * AddItem() adds a menu item by ID and name, ensuring no duplicate keys.
     */
    public void AddItem(int menuID, string foodName)
    {
        if (!foodItems.ContainsKey(menuID))
        {
            foodItems.Add(menuID, foodName);
        }
    }
}