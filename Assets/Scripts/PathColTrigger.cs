using UnityEngine;

public class PathColTrigger : MonoBehaviour
{
    private float speedMultiplier = 1f; // Default speed multiplier
    private RushAbility rushAbility; // Reference to the RushAbility
    public PlayerMovement playerMovement;

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }

    public void SetAbilityReference(RushAbility ability)
    {
        rushAbility = ability;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerMovement = other.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.SetSpeedMultiplier(speedMultiplier);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerMovement = other.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.SetSpeedMultiplier(1f); // Reset to normal speed
            }
        }
    }
}
