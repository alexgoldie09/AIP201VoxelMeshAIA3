using UnityEngine;

public class DeliverOrderToKitchen : GAction
{
    public override bool PrePerform()
    {
        return true;
    }

    public override bool PostPerform()
    {
        // Modify world state to indicate an order is now waiting
        GWorld.Instance.GetWorld().ModifyState("OrderAtKitchen", 1);

        return true;
    }
}
