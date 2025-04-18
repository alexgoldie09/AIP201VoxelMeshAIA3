using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Customer : GAgent
{
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        SubGoal s1 = new SubGoal("isWaiting", 1, true);
        goals.Add(s1, 3);
    }
}
