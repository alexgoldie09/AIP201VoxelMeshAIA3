using System.Collections;
using UnityEngine;

public class FinishOrder : GAction
{
    public override bool PrePerform()
    {
        // REMOVE the orderCooked because it's finished
        // GWorld.Instance.GetWorld().ModifyState("orderCooked", -1);

        Debug.Log("[FinishOrder] Finished and delivered food to expediter.");

        return true;
    }

    public override bool PostPerform()
    {
        running = false;

        // Add new world state
        GWorld.Instance.GetWorld().ModifyState("FoodReadyToDeliver", 1);

        return true;
    }
}
