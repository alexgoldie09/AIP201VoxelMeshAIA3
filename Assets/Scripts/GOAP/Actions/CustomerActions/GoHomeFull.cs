/*
 * GoHomeFull.cs
 * -------------
 * This class represents the GOAP action triggered when a customer fails to get seated (restaurant is full).
 *
 * Extras:
 *  - This is a fallback action triggered if seating fails during `WaitForTable`.
 */

using UnityEngine;

public class GoHomeFull : GAction
{
    /*
     * PrePerform() is the actions performed before the agent begins moving to its destination.
     * - Updates the world state to reflect that the restaurant was full
     * - Prevents the customer from idling further
     */
    public override bool PrePerform()
    {
        agent.stoppingDistance = 2f;
        thisAgent.inIdle = false;
        agent.updateRotation = true;

        GWorld.Instance.GetWorld().ModifyState("Restaurant_Full", 1);
        Debug.Log($"[GoHomeFull] The restaurant was full. {GetComponent<Customer>().name} going home!");

        return true;
    }

    /*
     * PostPerform() is the actions performed after the agent has reached it's destination.
     * - Destroys the customer GameObject, simulating them leaving the scene
     */
    public override bool PostPerform()
    {
        running = false;
        Destroy(gameObject);
        return true;
    }
}
