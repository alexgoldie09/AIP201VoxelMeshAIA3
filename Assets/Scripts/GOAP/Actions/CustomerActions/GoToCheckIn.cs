/*
 * GoToCheckIn.cs
 * --------------
 * This class represents the GOAP action that moves the customer to the front desk or waiting area.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToCheckIn : GAction
{
    /*
     * PrePerform() is the actions performed before the agent begins moving to its destination.
     * - Sets a reasonable stopping distance so the customer doesn't crowd the reception
     */
    public override bool PrePerform()
    {
        agent.stoppingDistance = 3f;
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