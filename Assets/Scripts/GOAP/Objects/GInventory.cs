/*
 * GInventory.cs
 * -------------
 * This class represents an agent's personal inventory system for holding physical objects (GameObjects) and logical tasks (Orders).
 *
 * Tasks:
 *  - Stores and manages a list of held GameObjects (e.g., items picked up, tools).
 *  - Stores and manages Orders (custom data structure used in GOAP tasks).
 *  - Provides utility methods for lookup, addition, removal, and query operations.
 *
 * Extras:
 *  - Items can be searched by name or tag (e.g., "Plate", "Order").
 *  - Designed for use in GAction tasks and accessed via thisAgent.inventory.
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class GInventory
{
    private List<GameObject> items = new List<GameObject>();
    private List<Order> orders = new List<Order>();

    #region***Items-Specific Methods***
    /* 
     * Add() adds a GameObject to the agent’s inventory (e.g., "Seat", "Food").
     */
    public void AddItem(GameObject i) => items.Add(i);

    /* 
     * FindItemWithName() returns the first GameObject in the inventory that
     * matches the provided name.
     * - Useful for strict location
     */
    public GameObject FindItemWithName(string name)
    {
        foreach (GameObject item in items)
        {
            if (item != null && item.name == name) { return item; }
        }
        return null;
    }

    /* 
     * FindItemWithName() returns the first GameObject in the inventory that
     * matches the provided tag.
     * - Useful for general categorisation
     */
    public GameObject FindItemWithTag(string tag)
    {
        foreach (GameObject item in items)
        {
            if (item != null && item.tag == tag) { return item; }
        }
        return null;
    }

    /* 
     * RemoveItem() removes a specific GameObject from the inventory by identity.
     * - Uses manual index lookup to ensure correct removal
     */
    public void RemoveItem(GameObject i)
    {
        int indexToRemove = -1;

        foreach (GameObject item in items)
        {
            indexToRemove++;
            if(item == i)
            {
                break;
            }
        }

        if (indexToRemove >= 0 && indexToRemove < items.Count)
        {
            items.RemoveAt(indexToRemove);
        }
    }
    #endregion
    #region***Orders-Specific Methods***
    /* 
     * Add() adds an Order object to the agent's order list (e.g., waiter receives order).
     */
    public void AddOrder(Order o) => orders.Add(o);
    /* 
     * RemoveOrder() removes a specific Order object from the inventory by identity.
     * - Uses manual index lookup to ensure correct removal
     */
    public void RemoveOrder(Order o)
    {
        int indexToRemove = -1;

        for (int i = 0; i < orders.Count; i++)
        {
            if (orders[i] == o)
            {
                indexToRemove = i;
                break;
            }
        }

        if (indexToRemove >= 0 && indexToRemove < orders.Count)
        {
            orders.RemoveAt(indexToRemove);
        }
    }
    /* 
     * GetFirstOrder() returns the first Order object in the inventory.
     * - Used when processing orders in FIFO manner (e.g., kitchen or delivery)
     */
    public Order GetFirstOrder() => orders.Count > 0 ? orders[0] : null;
    /* 
     * GetAllOrders() returns all orders in the inventory.
     * - Useful for inspection/debugging or logic where agent can hold multiple orders
     */
    public List<Order> GetAllOrders() => orders;
    /* 
     * HasOrder() checks if a specific order exists in the inventory.
     * - Used to prevent duplicates or verify order ownership
     */
    public bool HasOrder(Order o) => orders.Contains(o);
    #endregion
}
