using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BlinkAbility", menuName = "Abilities/Blink")]
public class BlinkAbility : Ability
{
    public float blinkDistance = 1f;
    public LayerMask obstacleMask;

    public override void Activate(GameObject player)
    {
        Debug.Log("Blink ability activated");

        if (player == null)
        {
            Debug.LogError("Player GameObject is null");
            return;
        }

        Vector3 direction = Vector3.zero;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        if (x != 0 || z != 0)
        {
            // If there is movement input, calculate the direction based on the input
            direction = player.transform.right * x + player.transform.forward * z;
        }
        else
        {
            // Default to forward direction if no movement input
            direction = player.transform.forward;
        }

        direction.y = 0;
        direction.Normalize();

        Debug.Log($"Blink direction: {direction}");

        RaycastHit hit;
        bool hasHit = Physics.Raycast(player.transform.position, direction, out hit, blinkDistance, obstacleMask);

        Vector3 newPosition;
        if (hasHit)
        {
            Debug.Log($"Obstacle hit at distance: {hit.distance}, Hit Point: {hit.point}");
            newPosition = hit.point - direction * 0.5f;
        }
        else
        {
            newPosition = player.transform.position + direction * blinkDistance;
        }

        Debug.Log($"New position: {newPosition}");

        // Temporarily disable the CharacterController to manually set the position
        CharacterController characterController = player.GetComponent<CharacterController>();
        if (characterController != null)
        {
            characterController.enabled = false;
        }

        player.transform.position = newPosition;

        // Re-enable the CharacterController
        if (characterController != null)
        {
            characterController.enabled = true;
        }
    }
}
