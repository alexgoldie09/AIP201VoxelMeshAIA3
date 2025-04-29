using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class GWorld
{
    private static readonly GWorld instance = new GWorld();
    private static WorldStates world;
    private static Queue<GameObject> customers;
    private static Queue<GameObject> seats;
    private static Queue<GameObject> cookTops;
    private static Queue<Order> orders;
    private static Queue<Order> readyOrders;

    static GWorld()
    {
        world = new WorldStates();
        customers = new Queue<GameObject>();
        seats = new Queue<GameObject>();
        cookTops = new Queue<GameObject>();
        orders = new Queue<Order>();
        readyOrders = new Queue<Order>();
    }

    private GWorld()
    {
    }

    public void AddCustomer(GameObject c)
    {
        customers.Enqueue(c);
    }

    public GameObject RemoveCustomer()
    {
        if (customers.Count == 0)
        {
            return null;
        }
        return customers.Dequeue();
    }

    public void AddSeat(GameObject s)
    {
        seats.Enqueue(s);
    }

    public GameObject RemoveSeat()
    {
        if (seats.Count == 0)
        {
            return null;
        }
        return seats.Dequeue();
    }

    public Queue<GameObject> GetSeatQueue()
    {
        return seats;
    }

    public void AddCookTop(GameObject ct)
    {
        cookTops.Enqueue(ct);
    }

    public GameObject RemoveCookTop()
    {
        if (cookTops.Count == 0)
        {
            return null;
        }

        return cookTops.Dequeue();
    }

    public int GetCookTopCount()
    {
        return cookTops.Count;
    }

    public void AddOrder(Order o)
    {
        orders.Enqueue(o);
    }

    public Order RemoveOrder()
    {
        if (orders.Count == 0)
            return null;
        return orders.Dequeue();
    }

    public int GetOrderCount()
    {
        return orders.Count;
    }


    public void AddReadyOrder(Order o)
    {
        readyOrders.Enqueue(o);
    }

    public Order RemoveReadyOrder()
    {
        if (readyOrders.Count == 0)
        {
            return null;
        }
        return readyOrders.Dequeue();
    }

    public GameObject PeekAnySeat()
    {
        int seatCount = seats.Count;

        for (int i = 0; i < seatCount; i++)
        {
            GameObject seat = seats.Dequeue();
            seats.Enqueue(seat); // preserve order

            if (seat != null && seat.TryGetComponent(out Seat seatComp))
            {
                if (!seatComp.isReserved)
                {
                    Debug.Log($"[GWorld] Peeked unreserved seat: {seat.name}");
                    return seat;
                }
            }
        }

        Debug.LogWarning("[GWorld] No unreserved seat available.");
        return null;
    }

    public static GWorld Instance
    { 
        get { return instance; } 
    }

    public WorldStates GetWorld() => world;
}
