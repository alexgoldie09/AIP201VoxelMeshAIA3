using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    public GameObject customerPrefab;
    public int maxAmountCustomers = 6;
    private int numCustomers = 0;

    // Start is called before the first frame update
    private void Start()
    {
        Invoke("SpawnPatient", 5);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SpawnPatient()
    {
        if (numCustomers < maxAmountCustomers)
        {
            numCustomers++;
            Debug.Log($"Number of customers currently: {numCustomers}");
            Instantiate(customerPrefab, this.transform.position, Quaternion.identity);
            Invoke("SpawnPatient", Random.Range(2, 10));
        }
        else
        {
            Debug.Log("Restaurant booked!");
        }
    }
}
