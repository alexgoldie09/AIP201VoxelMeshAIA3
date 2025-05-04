/*
 * Spawn.cs
 * --------
 * This class handles the timed spawning of customer GameObjects in a simulated restaurant environment.
 * It is used for testing and running continuous simulation loops.
 *
 * Tasks:
 *  - Spawns customer prefabs at a random interval between `minCustomerSpawnTime` and `maxCustomerSpawnTime`.
 *  - Controls the spawn flow using a maximum allowed number of customers (`maxAmountCustomers`).
 *  - Implements a global spawn cooldown period once the maximum is reached.
 *  - Includes a threshold (`maxThreshInfiniteSpawn`) to prevent infinite overpopulation.
 *
 * Extras:
 *  - This script is designed for debugging and simulation only, not gameplay deployment.
 *  - Newly spawned customers are named sequentially (e.g., "Customer_1", "Customer_2").
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Spawn : MonoBehaviour
{
    [Header("Customer Reference")]
    public GameObject customerPrefab;              // Prefab for customer GameObject to spawn
    [Header("Customer Amount")]
    public int maxAmountCustomers = 6;             // Initial max number of customers allowed at once
    private int numCustomers = 0;                  // Current number of spawned customers
    [Header("Customer Spawn Times")]
    public int minCustomerSpawnTime = 2;           // Minimum delay between spawns
    public int maxCustomerSpawnTime = 10;          // Maximum delay between spawns
    [Header("Global Spawn Variables")]
    public float globalSpawnCooldown = 120f;       // Cooldown applied once max customer limit is hit
    private bool waitingForGlobalRespawn = false;  // Tracks if cooldown is active
    private float globalSpawnTimer;                // Timer for global cooldown
    public int maxThreshInfiniteSpawn = 100;       // Cap to prevent runaway spawning
    private bool stopInfiniteSpawn = false;        // Flag to halt all future spawns

    /*
     * Start()
     * - Waits 5 seconds before initiating the first customer spawn
     */
    private void Start()
    {
        Invoke("SpawnPatient", 5);
    }

    /*
     * Update()
     * - Handles countdown timer for global cooldown after max customers reached
     * - When the timer expires, it triggers a new spawn
     */
    private void Update()
    {
        // Handle respawning delay timer
        if (waitingForGlobalRespawn)
        {
            globalSpawnTimer -= Time.deltaTime;
            if (globalSpawnTimer <= 0f)
            {
                waitingForGlobalRespawn = false;
                SpawnPatient();
            }
        }
    }

    /*
     * SpawnPatient() spawns a new customer if not in cooldown and spawn cap 
     * hasn't been exceeded.
     * - If the cap is hit, enables a cooldown before the next batch
     * - Also progressively increases the max allowed customers (doubling) 
     *   to simulate traffic ramping
     */
    private void SpawnPatient()
    {
        if (!waitingForGlobalRespawn && !stopInfiniteSpawn)
        {
            if (numCustomers < maxAmountCustomers)
            {
                numCustomers++;
                Debug.Log($"Number of customers currently: {numCustomers}");

                GameObject newCustomer = Instantiate(customerPrefab, transform.position, Quaternion.identity);
                newCustomer.name = $"Customer_{numCustomers}";

                // Schedule next spawn randomly
                Invoke("SpawnPatient", Random.Range(minCustomerSpawnTime, maxCustomerSpawnTime));
            }
            else
            {
                Debug.Log("Restaurant booked!");
                waitingForGlobalRespawn = true;
                globalSpawnTimer = globalSpawnCooldown;

                // Double the max allowed customers for next cycle
                maxAmountCustomers *= 2;

                // If threshold exceeded, halt all spawns
                if (maxAmountCustomers > maxThreshInfiniteSpawn)
                {
                    stopInfiniteSpawn = true;
                }
            }
        }
    }
}
