using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Order
{
    public int ticketNumber;
    public Dictionary<int, string> foodItems = new Dictionary<int, string>();

    public Order(int ticketNumber)
    {
        this.ticketNumber = ticketNumber;
    }

    public void AddItem(int menuID, string foodName)
    {
        if (!foodItems.ContainsKey(menuID))
        {
            foodItems.Add(menuID, foodName);
        }
    }
}