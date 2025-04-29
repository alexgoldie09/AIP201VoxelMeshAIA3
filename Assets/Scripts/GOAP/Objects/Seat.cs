using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seat : MonoBehaviour
{
    public int tableNumber;
    public Transform staffSpot;

    public bool isReserved = false;

    // Start is called before the first frame update
    private void Start()
    {
        GWorld.Instance.AddSeat(this.gameObject);
        GWorld.Instance.GetWorld().ModifyState("Free_Seat", 1);
    }
}
