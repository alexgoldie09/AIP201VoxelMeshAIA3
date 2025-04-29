using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReturnToWaitArea : GAction
{
    public override bool PrePerform()
    {
        agent.stoppingDistance = 1f; // Stop slightly before reaching target

        return true;
    }

    public override bool PostPerform()
    {
        thisAgent.inIdle = true;
        return true;
    }

}
