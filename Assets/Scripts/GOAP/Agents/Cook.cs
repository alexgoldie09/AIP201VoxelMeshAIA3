using UnityEngine;

public class CookAgent : GAgent
{
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();

        SubGoal g1 = new SubGoal("orderReady", 1, false);
        goals.Add(g1, 5);

        SubGoal g2 = new SubGoal("finishedOrder", 1, false);
        goals.Add(g2, 4);

        SubGoal g3 = new SubGoal("Idle", 1, false);
        goals.Add(g3, 1);
    }
}