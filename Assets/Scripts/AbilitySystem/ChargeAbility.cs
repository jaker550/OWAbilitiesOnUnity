using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "ChargeAbility", menuName = "Abilities/Charge")]
public class ChargeAbility : Ability
{
    #region Charge Parameters
    [Header("Charge Parameters")]

    [Tooltip("Speed of the charge. Higher values will make the charge faster.")]
    public float chargeSpeed = 10f;

    [Tooltip("Duration of the charge in seconds.")]
    public float chargeDuration = 3f;

    [Tooltip("Speed of lateral movement during charge.")]
    public float strafeSpeed = 5f;

    [Tooltip("Sensitivity of player rotation during strafe.")]
    public float rotationSensitivity = 1f;

    [Tooltip("Distance for the raycast to detect enemies.")]
    public float raycastDistance = 5f;

    [Tooltip("Distance in front of the player where the enemy should be positioned.")]
    public float enemyOffset = 1f;

    [Tooltip("Amount of damage to apply to enemy on collision with obstacles.")]
    public float damageOnCollision = 100f;
    #endregion

    #region Camera References
    [Header("Camera References")]

    [Tooltip("Main camera that follows the player.")]
    public GameObject mainCamera;

    [Tooltip("Camera used during charging to provide a different perspective.")]
    public GameObject secondCamera;
    #endregion

    #region Internal State
    [Header("Internal State")]

    [Tooltip("Flag to track if currently charging.")]
    public bool isCharging = false;

    [Tooltip("Reference to the active charge coroutine.")]
    private Coroutine chargeCoroutine;

    [Tooltip("Store the initial charge direction.")]
    private Vector3 initialChargeDirection;

    [Tooltip("Reference to the player game object.")]
    private GameObject player;

    [Tooltip("Store the pinned enemy.")]
    private GameObject enemyPinned;
    #endregion

    public override void Activate(GameObject player)
    {
        if (!isCharging)
        {
            this.player = player;

            // Find the cameras within the player hierarchy
            if (mainCamera == null)
            {
                mainCamera = player.transform.Find("Camera").gameObject;
            }
            if (secondCamera == null)
            {
                secondCamera = player.transform.Find("ChargeCamera").gameObject;
            }

            StartCharge(player);
        }
        else
        {
            StopCharge(player);
        }
    }

    private void StartCharge(GameObject player)
    {
        // Capture the initial direction the player is facing
        initialChargeDirection = player.transform.forward;

        // Disable the PlayerMovement script
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // Start the charge coroutine
        chargeCoroutine = player.GetComponent<MonoBehaviour>().StartCoroutine(ChargeCoroutine(player));
    }

    private void StopCharge(GameObject player)
    {
        isCharging = false;

        // Re-enable the PlayerMovement script
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }

        secondCamera.SetActive(false);
        mainCamera.SetActive(true);
        if (chargeCoroutine != null)
        {
            player.GetComponent<MonoBehaviour>().StopCoroutine(chargeCoroutine);
        }

        // Reset the X rotation of the main camera to 0
        if (mainCamera != null)
        {
            Vector3 currentRotation = mainCamera.transform.eulerAngles;
            currentRotation.x = 0; // Set X rotation to 0
            mainCamera.transform.eulerAngles = currentRotation; // Apply the new rotation
        }

        // Clear the enemy reference
        enemyPinned = null;
    }

    private IEnumerator ChargeCoroutine(GameObject player)
    {
        isCharging = true;
        secondCamera.SetActive(true);
        mainCamera.SetActive(false);
        float chargeEndTime = Time.time + chargeDuration;

        // Get the player CharacterController if available
        CharacterController characterController = player.GetComponent<CharacterController>();

        while (Time.time < chargeEndTime)
        {
            // Continuously detect the enemy in front of the player
            DetectEnemy();

            // Check for collisions with obstacles
            if (CheckCollision())
            {
                StopCharge(player); // Stop the charge if a collision is detected
                yield break; // Exit the coroutine early
            }

            // Update the forward movement to follow the player's current facing direction
            Vector3 forwardMovement = player.transform.forward * chargeSpeed;

            // Lateral strafe movement
            float strafeInput = Input.GetAxis("Horizontal");
            Vector3 strafeMovement = player.transform.right * strafeInput * strafeSpeed;

            // Apply rotation based on strafe input
            float rotationAmount = strafeInput * rotationSensitivity; // Rotation sensitivity
            Quaternion rotation = Quaternion.Euler(0, rotationAmount, 0);
            player.transform.rotation = Quaternion.Slerp(player.transform.rotation, player.transform.rotation * rotation, Time.deltaTime * 5f);

            // Combine forward and strafe movement
            Vector3 totalMovement = forwardMovement + strafeMovement;

            // Apply downward movement to stick the player to the ground
            Vector3 groundRayOrigin = player.transform.position + Vector3.down * 0.5f; // Ray origin slightly below the player
            Vector3 groundRayDirection = Vector3.down; // Direction of the raycast to point directly downwards
            float groundRayLength = 0.5f; // Length of the ray to check the ground

            // Draw the raycast for visualization
            Debug.DrawRay(groundRayOrigin, groundRayDirection * groundRayLength, Color.green);

            // Check if the player is grounded
            bool isGrounded = Physics.Raycast(groundRayOrigin, groundRayDirection, out RaycastHit groundHit, groundRayLength);

            if (isGrounded)
            {
                // Snap the player to the ground position
                Vector3 newPlayerPosition = new Vector3(player.transform.position.x, groundHit.point.y, player.transform.position.z);
                player.transform.position = newPlayerPosition;

                // Ensure vertical velocity is zero
                if (characterController != null)
                {
                    characterController.Move(Vector3.down * (characterController.height / 2 - groundHit.distance));
                }
            }
            else
            {
                // Apply a downward movement if not grounded
                if (characterController != null)
                {
                    characterController.Move(Vector3.down * 9.81f * Time.deltaTime);
                }
            }

            // Move the player using the CharacterController
            if (characterController != null)
            {
                characterController.Move(totalMovement * Time.deltaTime);
            }

            // Apply the same movement to the enemy if it's being charged into
            if (enemyPinned != null)
            {
                // Apply force to the enemy's Rigidbody to move it in sync with the player
                Rigidbody enemyRigidbody = enemyPinned.GetComponent<Rigidbody>();
                if (enemyRigidbody != null)
                {
                    enemyRigidbody.linearVelocity = totalMovement;
                }

                // Check if the enemy hits something
                CheckEnemyCollision();
            }

            yield return null;
        }

        StopCharge(player); // Ensure the charge is stopped at the end of the duration
    }

    private bool CheckCollision()
    {
        RaycastHit hit;
        Vector3 rayDirection = initialChargeDirection.normalized * raycastDistance;
        Debug.DrawRay(player.transform.position, rayDirection, Color.red); // Visualize the raycast
        if (Physics.Raycast(player.transform.position, initialChargeDirection, out hit, raycastDistance))
        {
            if (hit.collider.CompareTag("Obstacle"))
            {
                // Collision with an obstacle detected, stop the charge
                Debug.Log("Collision with obstacle detected, stopping charge.");
                return true;
            }
        }
        return false;
    }

    private void DetectEnemy()
    {
        RaycastHit hit;
        Vector3 rayDirection = player.transform.forward * raycastDistance;
        Debug.DrawRay(player.transform.position, rayDirection, Color.red); // Visualize the raycast
        if (Physics.Raycast(player.transform.position, player.transform.forward, out hit, raycastDistance))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                enemyPinned = hit.collider.gameObject;
                if (enemyPinned != null)
                {
                    // Position the enemy directly in front of the player
                    Vector3 enemyPosition = player.transform.position + player.transform.forward * enemyOffset;
                    enemyPinned.transform.position = enemyPosition;

                    // Stop the enemy's existing motion
                    Rigidbody enemyRigidbody = enemyPinned.GetComponent<Rigidbody>();
                    if (enemyRigidbody != null)
                    {
                        enemyRigidbody.linearVelocity = Vector3.zero;
                    }
                }
            }
        }
        else
        {
            // Clear the enemy if no longer detected
            enemyPinned = null;
        }
    }

    private void CheckEnemyCollision()
    {
        if (enemyPinned == null)
            return;

        // Get the enemy's Rigidbody
        Rigidbody enemyRigidbody = enemyPinned.GetComponent<Rigidbody>();
        if (enemyRigidbody == null)
            return;

        // Check for collisions with obstacles
        RaycastHit hit;
        Vector3 rayDirection = enemyRigidbody.linearVelocity.normalized * raycastDistance;
        Debug.DrawRay(enemyRigidbody.position, rayDirection, Color.blue); // Visualize the raycast
        if (Physics.Raycast(enemyRigidbody.position, enemyRigidbody.linearVelocity.normalized, out hit, raycastDistance))
        {
            Debug.Log($"Raycast hit: {hit.collider.name} with tag {hit.collider.tag}");

            // If the raycast hits an obstacle
            if (hit.collider.CompareTag("Obstacle"))
            {
                Debug.Log("Collision with obstacle detected. Applying damage.");

                // Apply damage to the enemy
                IDamageable damageable = enemyPinned.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(damageOnCollision);
                }
                else
                {
                    Debug.LogWarning($"Enemy does not implement IDamageable: {enemyPinned.name}");
                }
                StopCharge(player);
            }
        }
    }
}
