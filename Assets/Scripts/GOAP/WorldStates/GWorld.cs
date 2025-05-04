/*
 * GWorld.cs
 * ---------
 * This class implements a singleton global environment manager to store shared state and resource queues.
 *
 * Tasks:
 *  - Maintains shared world conditions (representing persistent facts).
 *  - Maintains queues for key interactable objects in the scene: customers, seats, cooktops, orders, etc.
 *      - Customers represent the agents who go to the restaurant and order food.
 *      - Seats represent the seat gameobjects used for determining how many customers 
 *        can be at the restaurant as well as for the other agents to interact with.
 *      - Cooktops represent the cooktop gameobjects used for the cook agents and determining
 *        where they can cook and how many there are to interact with.
 *      - Orders represent the Order class housing a number of variables 
 *        for a customer's specific order.
 *      - ReadyOrders represent the Order class housing a number of variables 
 *        for when a cook finishes an order, which is then saved as reference to the waiter.
 *  - Agents can access, modify, or query global state through this class.
 *
 * Extras:
 *  - Designed as a thread-safe singleton with lazy initialization.
 *  - Used heavily in GOAP planning and agent actions.
 *
 */

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

    // Initialiasitation
    static GWorld()
    {
        world = new WorldStates();
        customers = new Queue<GameObject>();
        seats = new Queue<GameObject>();
        cookTops = new Queue<GameObject>();
        orders = new Queue<Order>();
        readyOrders = new Queue<Order>();
    }

    // Default Constructor
    private GWorld()
    {
    }

    // Accessors
    public static GWorld Instance => instance;

    /* 
     * GetWorld() returns the current World States.
     */
    public WorldStates GetWorld() => world;

    #region ***Customer-Specific Methods***
    /* 
     * AddCustomer() adds a new customer gameobject to the customers queue.
     */
    public void AddCustomer(GameObject c) => customers.Enqueue(c);
    /* 
     * RemoveCustomer() safely removes a customer gameobject from the customers queue.
     */
    public GameObject RemoveCustomer() => customers.Count == 0 ? null : customers.Dequeue();
    #endregion

    #region***Seat-Specific Methods***
    /* 
     * AddSeat() adds a new seat gameobject to the seats queue.
     */
    public void AddSeat(GameObject s) => seats.Enqueue(s);
    /* 
     * RemoveSeat() safely removes a seat gameobject from the seat queue.
     */
    public GameObject RemoveSeat() => seats.Count == 0 ? null : seats.Dequeue();
    /* 
     * GetSeatQueue() returns the seats queue.
     */
    public Queue<GameObject> GetSeatQueue() => seats;
    /* 
     * PeekAnySeat() returns a free and available seat gameobject.
     * - loops through the seats
     * - removes from queue to store a reference and then adds that reference to 
     *   preserve the order
     * - if the seat is not reserved, return that seat, otherwise there is not seat available
     */
    public GameObject PeekAnySeat()
    {
        int count = seats.Count;
        for (int i = 0; i < count; i++)
        {
            GameObject seat = seats.Dequeue();
            seats.Enqueue(seat);
            if (seat != null && seat.TryGetComponent(out Seat seatComp) && !seatComp.isReserved)
            {
                //Debug.Log($"[GWorld] Peeked unreserved seat: {seat.name}");
                return seat;
            }
        }

        //Debug.LogWarning("[GWorld] No unreserved seat available.");
        return null;
    }
    #endregion

    #region***Cooktop-Specific Methods***
    /* 
     * AddCookTop() adds a new cooktop gameobject to the cooktops queue.
     */
    public void AddCookTop(GameObject ct) => cookTops.Enqueue(ct);
    /* 
     * RemoveCookTop() safely removes a cooktop gameobject from the cooktops queue.
     */
    public GameObject RemoveCookTop() => cookTops.Count == 0 ? null : cookTops.Dequeue();
    /* 
     * GetCookTopCount() returns the number of cooktops that exist.
     */
    public int GetCookTopCount() => cookTops.Count;
    #endregion
    #region***Order-Specific Methods***
    // --- Orders ---
    /* 
     * AddOrder() adds a new order object to the orders queue.
     */
    public void AddOrder(Order o) => orders.Enqueue(o);
    /* 
     * RemoveOrder() safely removes an order object from the orders queue.
     */
    public Order RemoveOrder() => orders.Count == 0 ? null : orders.Dequeue();
    /* 
     * GetOrderCount() returns the number of orders that exist.
     */
    public int GetOrderCount() => orders.Count;

    // --- Ready Orders ---
    /* 
     * AddReadyOrder() adds a new order object to the ready orders queue.
     */
    public void AddReadyOrder(Order o) => readyOrders.Enqueue(o);
    /* 
     * RemoveReadyOrder() safely removes an order object from the ready orders queue.
     */
    public Order RemoveReadyOrder() => readyOrders.Count == 0 ? null : readyOrders.Dequeue();
    #endregion
}
