using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "SwiftStrikeAbility", menuName = "Abilities/Swift Strike")]
public class SwiftStrikeAbility : Ability
{
    public float dashDistance = 10f; // Distance of the dash
    public float dashDuration = 0.2f; // Duration of the dash

    private bool isDashing = false; // Flag to track if currently dashing
    private float originalGravity; // Store the original gravity value

    public override void Activate(GameObject player)
    {
        if (!isDashing)
        {
            PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                // Store the original gravity value
                originalGravity = playerMovement.Gravity;

                // Disable gravity for the dash
                playerMovement.Gravity = 0;
            }

            // Start the dash coroutine on the player object
            player.GetComponent<MonoBehaviour>().StartCoroutine(DashCoroutine(player));
        }
        else
        {
            Debug.Log("Already dashing. Cannot activate dash again.");
        }
    }

    private IEnumerator DashCoroutine(GameObject player)
    {
        isDashing = true;
        CharacterController characterController = player.GetComponent<CharacterController>();

        if (characterController == null)
        {
            Debug.LogError("CharacterController component is missing on the player.");
            yield break;
        }

        Vector3 dashDirection = GetDashDirection(player);
        Vector3 dashVelocity = dashDirection * (dashDistance / dashDuration);
        float elapsedTime = 0f;

        while (elapsedTime < dashDuration)
        {
            characterController.Move(dashVelocity * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isDashing = false;

        // Re-enable gravity after the dash ends
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.Gravity = originalGravity;
        }
    }

    private Vector3 GetDashDirection(GameObject player)
    {
        // Get the direction the player is looking
        Vector3 forwardDirection = player.transform.forward;

        // Get the camera's forward direction
        Camera playerCamera = player.GetComponentInChildren<Camera>();
        if (playerCamera != null)
        {
            forwardDirection = playerCamera.transform.forward;
        }

        // Return the normalized direction (ensures consistent speed regardless of distance)
        return forwardDirection.normalized;
    }
}