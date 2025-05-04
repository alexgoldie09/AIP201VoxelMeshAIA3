/*
 * ReturnToWaitArea.cs
 * -------------------
 *  This class represents the GOAP action where the wait staff finishes a task and returns to an idle state.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnToWaitArea : GAction
{
    /*
     * PrePerform() is the actions performed before the agent begins moving to its destination.
     * - Sets a reasonable stopping distance so the wait staff doesn't crowd the the waiting area
     */
    public override bool PrePerform()
    {
        agent.stoppingDistance = 1f;

        return true;
    }

    /*
     * PostPerform() is the actions performed after the agent has reached it's destination.
     * - Flags the agent is in an idle state meaning this doesn't constantly fire
     */
    public override bool PostPerform()
    {
        thisAgent.inIdle = true;
        return true;
    }

}
