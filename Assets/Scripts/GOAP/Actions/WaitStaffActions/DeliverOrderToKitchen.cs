/*
 * DeliverOrderToKitchen.cs
 * ------------------------
 * This class represents the GOAP action where the order has been delivered to the kitchen.
 * 
 * Extras:
 * - It modifies the global world state to inform the cook that a new order is waiting.
 */

using UnityEngine;

public class DeliverOrderToKitchen : GAction
{
    /*
     * PrePerform() is the actions performed before the agent begins moving to its destination.
     * - No pre-action logic required here
     */
    public override bool PrePerform()
    {
        return true;
    }

    /*
     * PostPerform() is the actions performed after the agent has reached it's destination.
     * - Modifies world state with 'OrderAtKitchen;
     */
    public override bool PostPerform()
    {
        GWorld.Instance.GetWorld().ModifyState("OrderAtKitchen", 1);

        return true;
    }
}
