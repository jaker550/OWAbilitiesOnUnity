using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "JaggedBladeAbility", menuName = "Abilities/JaggedBlade")]
public class JaggedBladeAbility : Ability
{
    public GameObject bladePrefab;
    public float throwForce = 10f;
    public float spawnDistance = 1f;
    public float returnSpeed = 20f;
    public float pullForce = 50f; // Force to pull the enemy towards the player
    public float verticalLift = 5f; // Amount of vertical lift
    public float liftDuration = 0.5f; // Duration for the lift before re-enabling NavMeshAgent

    private GameObject bladeInstance;
    private bool bladeOut = false;
    private bool isReturning = false;
    private Transform attachedEnemyTransform = null;

    public override void Activate(GameObject player)
    {
        if (isReturning)
        {
            Debug.Log("Blade is currently returning. Cannot activate the ability.");
            return;
        }

        if (bladeOut)
        {
            ReturnBladeToPlayer(player);
        }
        else
        {
            ThrowBlade(player);
        }
    }

    private void ThrowBlade(GameObject player)
    {
        if (bladePrefab == null)
        {
            Debug.LogWarning("Blade prefab not set!");
            return;
        }

        Camera playerCamera = player.GetComponentInChildren<Camera>();
        if (playerCamera == null)
        {
            Debug.LogWarning("Player does not have a camera!");
            return;
        }

        Vector3 spawnPosition = playerCamera.transform.position + playerCamera.transform.forward * spawnDistance;
        Quaternion spawnRotation = playerCamera.transform.rotation * Quaternion.Euler(0, 90f, 0);

        bladeInstance = Instantiate(bladePrefab, spawnPosition, spawnRotation);

        if (bladeInstance == null)
        {
            Debug.LogError("Failed to instantiate blade instance!");
            return;
        }

        Rigidbody rb = bladeInstance.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddForce(playerCamera.transform.forward * throwForce, ForceMode.VelocityChange);
        }
        else
        {
            Debug.LogWarning("Blade prefab does not have a Rigidbody component!");
        }

        bladeOut = true;

        // Set a reference of this ability script in the blade instance
        JaggedBlade jaggedBlade = bladeInstance.GetComponent<JaggedBlade>();
        if (jaggedBlade != null)
        {
            jaggedBlade.bladeAbility = this;
        }
    }

    private void ReturnBladeToPlayer(GameObject player)
    {
        if (bladeInstance == null)
        {
            Debug.LogWarning("Blade instance not found!");
            bladeOut = false;
            return;
        }

        isReturning = true;
        player.GetComponent<MonoBehaviour>().StartCoroutine(ReturnBladeCoroutine(player));
    }

    private IEnumerator ReturnBladeCoroutine(GameObject player)
    {
        if (bladeInstance == null)
        {
            Debug.LogWarning("Blade instance not found when starting return coroutine!");
            isReturning = false;
            yield break;
        }

        Rigidbody rb = bladeInstance.GetComponent<Rigidbody>();
        Collider col = bladeInstance.GetComponent<Collider>();

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;

            if (col != null)
            {
                col.enabled = false;
            }

            while (bladeInstance != null)
            {
                bladeInstance.transform.position = Vector3.MoveTowards(bladeInstance.transform.position, player.transform.position, returnSpeed * Time.deltaTime);

                // Apply force only if the blade is attached to an enemy
                if (attachedEnemyTransform != null)
                {
                    Enemy enemy = attachedEnemyTransform.GetComponent<Enemy>();
                    if (enemy != null)
                    {
                        // Directly call the PullWithVerticalLift method
                        enemy.PullWithVerticalLift(player.transform.position, pullForce, verticalLift, liftDuration);
                    }

                    // Only apply force once
                    attachedEnemyTransform = null;
                }

                if (Vector3.Distance(bladeInstance.transform.position, player.transform.position) < 1f)
                {
                    Destroy(bladeInstance);
                    bladeInstance = null;
                    bladeOut = false;
                    isReturning = false;
                    yield break;
                }

                yield return null;
            }
        }
        else
        {
            Debug.LogWarning("Blade prefab does not have a Rigidbody component when returning!");
        }
    }

    // This method is called by the JaggedBlade script when it hits any object
    public void OnBladeHit(GameObject hitObject)
    {
        if (hitObject.CompareTag("Enemy"))
        {
            // Ensure the blade is a child of the hit object
            if (bladeInstance != null && bladeInstance.transform.IsChildOf(hitObject.transform))
            {
                attachedEnemyTransform = hitObject.transform;
                Debug.Log("Blade attached to enemy!");
            }
        }
    }
}
