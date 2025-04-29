using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToCheckIn : GAction
{

    public override bool PostPerform()
    {
        return true;
    }

    public override bool PrePerform()
    {
        agent.stoppingDistance = 3f;
        thisAgent.inIdle = false;

        return true;
    }
}