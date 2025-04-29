using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class GInventory
{
    private List<GameObject> items = new List<GameObject>();
    private List<Order> orders = new List<Order>();

    public void AddItem(GameObject i)
    {
        items.Add(i);
    }

    public GameObject GetItem(string name)
    {
        foreach (GameObject item in items)
        {
            if (item != null && item.name == name)
            {
                return item;
            }
        }
        return null;
    }

    public GameObject FindItemWithTag(string tag)
    {
        foreach (GameObject i in items)
        {
            if (i.tag == tag) { return i; }
        }
        return null;
    }

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

    public void AddOrder(Order o)
    {
        orders.Add(o);
    }

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

    public Order GetFirstOrder()
    {
        if (orders.Count > 0)
            return orders[0];
        else
            return null;
    }

    public List<Order> GetAllOrders()
    {
        return orders;
    }

    public bool HasOrder(Order o)
    {
        return orders.Contains(o);
    }
}
