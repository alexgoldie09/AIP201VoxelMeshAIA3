/*
 * CookTop.cs
 * ----------
 * This class represents a cooking station in the restaurant used by Cook agent.
 *
 * Tasks:
 *  - Registers itself with GWorld as an interactable cooktop.
 *  - Updates the global state to reflect available cooktop ("Free_CookTop").
 *  - Tracks if it is currently in use (via `isOccupied`).
 *
 * Extras:
 *  - Used in GOAP planning as a world condition ("Free_CookTop").
 *  - Expected to be reserved or released through GOAP actions.
 */

using UnityEngine;

public class CookTop : MonoBehaviour
{
    public bool isOccupied = false; // Set true when assigned to a cook

    private void Start()
    {
        GWorld.Instance.AddCookTop(gameObject);
        GWorld.Instance.GetWorld().ModifyState("Free_CookTop", 1);
    }
}