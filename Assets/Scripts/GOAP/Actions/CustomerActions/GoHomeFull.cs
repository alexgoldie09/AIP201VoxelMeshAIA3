using UnityEngine;

public class GoHomeFull : GAction
{
    public override bool PrePerform()
    {
        agent.stoppingDistance = 0;
        thisAgent.inIdle = false;
        agent.updateRotation = true;

        //beliefs.ModifyState("RestaurantFull", -1);
        GWorld.Instance.GetWorld().ModifyState("Restaurant_Full", 1);
        Debug.Log($"[GoHomeFull] {GetComponent<Customer>().name} is UNSATISFIED and going home!");


        return true;
    }

    public override bool PostPerform()
    {
        running = false;
        Destroy(gameObject); // Remove agent
        return true;
    }
}
