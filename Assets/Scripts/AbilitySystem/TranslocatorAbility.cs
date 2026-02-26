using UnityEngine;

[CreateAssetMenu(fileName = "TranslocatorAbility", menuName = "Abilities/Translocator")]
public class TranslocatorAbility : Ability
{
    public GameObject translocatorPrefab; // Reference to the translocator prefab
    public float throwForce = 10f; // Force with which the translocator is thrown
    public float spawnDistance = 1f; // Distance in front of the camera to spawn the translocator
    public float timeToDestroy = 5f; // Time before the translocator destroys itself

    public override void Activate(GameObject player)
    {
        if (translocatorPrefab == null)
        {
            Debug.LogWarning("Translocator prefab not set!");
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

        // Instantiate the translocator object at the calculated position
        GameObject translocatorInstance = Instantiate(translocatorPrefab, spawnPosition, playerCamera.transform.rotation);

        // Initialize the translocator with the player reference
        Translocator translocatorScript = translocatorInstance.GetComponent<Translocator>();
        if (translocatorScript != null)
        {
            translocatorScript.Initialize(player, timeToDestroy);
        }

        Rigidbody rb = translocatorInstance.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Apply force to throw the translocator in the direction the camera is facing
            rb.AddForce(playerCamera.transform.forward * throwForce, ForceMode.VelocityChange);
        }
        else
        {
            Debug.LogWarning("Translocator prefab does not have a Rigidbody component!");
        }
    }
}
