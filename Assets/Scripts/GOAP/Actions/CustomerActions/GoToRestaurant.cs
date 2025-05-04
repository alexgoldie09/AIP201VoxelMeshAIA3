/*
 * GoToRestaurant.cs
 * -----------------
 * This class represents the GOAP action that moves the customer walking toward the restaurant entrance.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToRestaurant : GAction
{
    /*
     * PrePerform() is the actions performed before the agent begins moving to its destination.
     * - Clears idle flag and sets the stopping distance for a direct approach
     */
    public override bool PrePerform()
    {
        agent.stoppingDistance = 0;
        thisAgent.inIdle = false;

        return true;
    }

    /*
     * PostPerform() is the actions performed after the agent has reached it's destination.
     * - No post-action logic required here
     */
    public override bool PostPerform()
    {
        return true;
    }
}
