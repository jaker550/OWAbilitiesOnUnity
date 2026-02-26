using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BlizzardAbility", menuName = "Abilities/Blizzard")]
public class BlizzardAbility : Ability
{
    public GameObject dronePrefab; // Reference to drone prefab
    public float throwForce = 10f; // Force with which drone is thrown
    public float spawnDistance = 1f; // Distance in front of the camera to spawn drone
    public float riseHeight = 5f; // Height of the drone
    public float riseSpeed = 2f; // Speed at which the drone rises at
    public float timeToDestroy = 5f; // Time before the drone destroys itself

    public override void Activate(GameObject player)
    {
        if (dronePrefab == null)
        {
            Debug.LogWarning("Drone prefab not set!");
            return;
        }

        Camera playerCamera = player.GetComponentInChildren<Camera>();
        if (playerCamera == null)
        {
            Debug.LogWarning("Player does not have a camera!");
            return;
        }

        // Calculate the spawn position a bit in front of the camera
        Vector3 spawnPosition = playerCamera.transform.position + playerCamera.transform.forward * spawnDistance;

        // Instantiate the drone object at the calculated position
        GameObject droneInstance = Instantiate(dronePrefab, spawnPosition, playerCamera.transform.rotation);

        // Initialize the drone with the player reference
        Drone droneScript = droneInstance.GetComponent<Drone>();
        if (droneScript != null)
        {
            droneScript.Initialize(player, riseHeight, riseSpeed, timeToDestroy);
        }

        Rigidbody rb = droneInstance.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Apply force to throw the drone in the direction the camera is facing
            rb.AddForce(playerCamera.transform.forward * throwForce, ForceMode.VelocityChange);
        }
        else
        {
            Debug.LogWarning("Drone prefab does not have a Rigidbody component!");
        }
    }
}
