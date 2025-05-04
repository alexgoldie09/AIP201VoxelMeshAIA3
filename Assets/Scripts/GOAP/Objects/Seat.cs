/*
 * Seat.cs
 * -------
 * This class represents a seat or table in the restaurant.
 *
 * Tasks:
 *  - Registers itself with GWorld as an interactable seat.
 *  - Updates the global state to reflect available seating ("Free_Seat").
 *  - Tracks seat reservation status (`isReserved`).
 *
 * Extras:
 *  - Used by customer GOAP planning to find unreserved seating.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seat : MonoBehaviour
{
    public int tableNumber; // ID to distinguish seats/tables
    public Transform staffSpot; // Where waitstaff should stand to serve

    public bool isReserved = false; // Set true when assigned to a customer

    private void Start()
    {
        GWorld.Instance.AddSeat(gameObject);
        GWorld.Instance.GetWorld().ModifyState("Free_Seat", 1);
    }
}
