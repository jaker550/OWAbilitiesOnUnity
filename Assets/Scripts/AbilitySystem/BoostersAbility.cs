using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "BoostersAbility", menuName = "Abilities/Boosters")]
public class BoostersAbility : Ability
{
    public float boostSpeed = 10f; // Speed of the boost
    public float boostDuration = 3f; // Duration of the boost
    public float slowdownRate = 2f; // Rate at which speed reduces after boost ends

    private bool isBoosting = false; // Flag to track if currently boosting
    private Coroutine boostCoroutine; // Reference to the active boost coroutine
    private float originalGravity; // Store the original gravity value

    public override void Activate(GameObject player)
    {
        if (!isBoosting)
        {
            StartBoost(player);
        }
        else
        {
            StopBoost(player);
        }
    }

    private void StartBoost(GameObject player)
    {
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            // Store the original gravity value
            originalGravity = playerMovement.Gravity;

            // Temporarily lower gravity during the boost
            playerMovement.Gravity = 2.2f;
        }

        // Start the boost coroutine on the player object
        boostCoroutine = player.GetComponent<MonoBehaviour>().StartCoroutine(BoostCoroutine(player));
    }

    private void StopBoost(GameObject player)
    {
        if (boostCoroutine != null)
        {
            player.GetComponent<MonoBehaviour>().StopCoroutine(boostCoroutine);
            boostCoroutine = null;
        }

        // Gradually reduce speed after boost ends
        player.GetComponent<MonoBehaviour>().StartCoroutine(SlowdownCoroutine(player));
    }

    private IEnumerator BoostCoroutine(GameObject player)
    {
        isBoosting = true;
        float boostEndTime = Time.time + boostDuration;
        CharacterController characterController = player.GetComponent<CharacterController>();

        while (Time.time < boostEndTime)
        {
            Vector3 moveDirection = GetBoostDirection(player) * boostSpeed;
            characterController.Move(moveDirection * Time.deltaTime);

            yield return null;
        }

        StopBoost(player);
    }

    private IEnumerator SlowdownCoroutine(GameObject player)
    {
        // Restore gravity to its original value during slowdown
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.Gravity = originalGravity;
        }

        float slowdownStartTime = Time.time;
        float currentSpeed = boostSpeed;
        CharacterController characterController = player.GetComponent<CharacterController>();

        // Slowdown the player when coming to an end of the booster
        while (currentSpeed > 0)
        {
            float slowdownProgress = (Time.time - slowdownStartTime) / slowdownRate;
            currentSpeed = Mathf.Lerp(boostSpeed, 0f, slowdownProgress);
            Vector3 moveDirection = GetBoostDirection(player) * currentSpeed;
            characterController.Move(moveDirection * Time.deltaTime);

            yield return null;
        }

        isBoosting = false;
    }

    // Gets the forward direction of the player
    private Vector3 GetBoostDirection(GameObject player)
    {
        Vector3 forwardDirection = player.transform.forward;

        Camera playerCamera = player.GetComponentInChildren<Camera>();
        if (playerCamera != null)
        {
            forwardDirection = playerCamera.transform.forward;
        }

        return forwardDirection.normalized;
    }
}
