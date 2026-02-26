using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.HID;

[CreateAssetMenu(fileName = "RushAbility", menuName = "Abilities/Kitsune Rush")]
public class RushAbility : Ability
{
    public float maxPathLength = 10f; // Maximum length of the path
    public float growthDuration = 5f; // Duration to grow the path
    public float pathLifetimeAfterGrowth = 3f; // Time the path remains after fully grown
    public float speedMultiplier = 2f; // Speed multiplier for the player
    public float heightThreshold = 1f; // Define the height threshold below which the center will be destroyed
    public string obstacleTag = "Obstacle"; // Tag to check for obstacles
    public GameObject rushPathPrefab; // Reference to the rushPath prefab
    public GameObject centerChecker; // Reference to the centerChecker
    public GameObject kitsune; // Reference to the Kitsune

    private GameObject currentRushPath; // Reference to the current rushPath object
    private GameObject centerInstance; // Reference to the instantiated center
    private GameObject kitsuneInstance; // Reference to the instantiated kitsune
    private Coroutine moveCenterCoroutine; // Reference to the MoveCenter coroutine
    private Coroutine growPathCoroutine; // Reference to the GrowPath coroutine
    private Coroutine moveKitsuneCoroutine; // Reference to the MoveKitsune coroutine
    private PlayerMovement playerMovement; // Reference to the PlayerMovement script

    public override void Activate(GameObject player)
    {
        playerMovement = player.GetComponent<PlayerMovement>(); // Get the PlayerMovement component

        Vector3 forwardDirection = player.transform.forward; // Get the forward direction from the player's transform
        Vector3 spawnPosition = player.transform.position + forwardDirection * 1.25f;

        // Perform a downward raycast to find the floor
        RaycastHit hit;
        if (Physics.Raycast(spawnPosition, Vector3.down, out hit, Mathf.Infinity))
        {
            spawnPosition = hit.point + new Vector3(0, 0.01f, 0);
        }
        else
        {
            Debug.LogWarning("RushAbility: No floor detected within raycast distance.");
            return; // Exit the method if no floor is detected
        }

        // Calculate the new rotation by aligning with the player's forward direction
        Quaternion newRotation = Quaternion.LookRotation(forwardDirection);

        // Set the X rotation to 0 degrees for the rushPath object
        Vector3 eulerRotation = newRotation.eulerAngles;
        eulerRotation.x = 0;
        newRotation = Quaternion.Euler(eulerRotation);

        // If there's a current rushPath, destroy it, the center instance, and the Kitsune instance
        if (currentRushPath != null)
        {
            Destroy(currentRushPath);
            if (centerInstance != null)
            {
                Destroy(centerInstance);
            }
            if (kitsuneInstance != null)
            {
                Destroy(kitsuneInstance);
            }
        }

        // Instantiate the new rushPath object at the calculated position
        currentRushPath = Instantiate(rushPathPrefab, spawnPosition, newRotation);

        // Get the pathCol child object
        Transform pathColTransform = currentRushPath.transform.Find("pathCol");
        if (pathColTransform == null)
        {
            Debug.LogWarning("RushAbility: PathCol child object not found in rushPath.");
            return;
        }

        // Set the speed multiplier on the pathCol
        PathColTrigger pathColTrigger = pathColTransform.GetComponent<PathColTrigger>();
        if (pathColTrigger != null)
        {
            pathColTrigger.SetSpeedMultiplier(speedMultiplier);
            pathColTrigger.SetAbilityReference(this); // Pass the reference to the RushAbility
        }

        Vector3 centerPosition = spawnPosition + new Vector3(0, 0.5f, 0); // Adjust the height as needed
        centerInstance = Instantiate(centerChecker, centerPosition, Quaternion.LookRotation(forwardDirection));

        // Calculate the position to the left and a bit behind the center
        Vector3 leftOffset = Vector3.Cross(forwardDirection, Vector3.up).normalized * 1.25f;
        Vector3 kitsuneLeft = centerPosition - leftOffset - forwardDirection * 0.5f; // Adjust height and back offset as needed
        kitsuneInstance = Instantiate(kitsune, kitsuneLeft, Quaternion.LookRotation(forwardDirection));

        // Calculate the speed for the center and Kitsune
        float centerSpeed = maxPathLength / growthDuration;

        // Stop any existing coroutines
        if (moveCenterCoroutine != null)
        {
            player.GetComponent<MonoBehaviour>().StopCoroutine(moveCenterCoroutine);
        }
        if (growPathCoroutine != null)
        {
            player.GetComponent<MonoBehaviour>().StopCoroutine(growPathCoroutine);
        }
        if (moveKitsuneCoroutine != null)
        {
            player.GetComponent<MonoBehaviour>().StopCoroutine(moveKitsuneCoroutine);
        }

        // Start the coroutines
        moveCenterCoroutine = player.GetComponent<MonoBehaviour>().StartCoroutine(MoveCenter(centerSpeed));
        moveKitsuneCoroutine = player.GetComponent<MonoBehaviour>().StartCoroutine(MoveKitsune(centerSpeed, leftOffset, forwardDirection * 2f));
        growPathCoroutine = player.GetComponent<MonoBehaviour>().StartCoroutine(GrowPath(currentRushPath, pathColTransform));
    }

    public void ResetPlayerSpeed()
    {
        if (playerMovement != null)
        {
            playerMovement.SetSpeedMultiplier(1f); // Reset to normal speed
        }
    }

    private IEnumerator MoveCenter(float speed)
    {
        float elapsedTime = 0f;

        while (centerInstance != null && elapsedTime < growthDuration)
        {
            // Move the center forward using its local forward direction
            centerInstance.transform.Translate(Vector3.forward * Time.deltaTime * speed);

            // Perform a forward raycast to check for obstacles in front of the center
            RaycastHit hit;
            if (Physics.Raycast(centerInstance.transform.position, centerInstance.transform.forward, out hit, 0.2f))
            {
                if (hit.collider.CompareTag(obstacleTag))
                {
                    Debug.Log("Center hit an obstacle and is being destroyed.");
                    Destroy(centerInstance);
                    centerInstance = null;
                    // Destroy the Kitsune when the center hits an obstacle
                    if (kitsuneInstance != null)
                    {
                        Destroy(kitsuneInstance);
                        kitsuneInstance = null;
                    }
                    yield break; // Exit the coroutine
                }
            }

            // Perform a downward raycast to find the floor
            if (Physics.Raycast(centerInstance.transform.position, Vector3.down, out hit, Mathf.Infinity))
            {
                float distanceToFloor = hit.distance;
                Debug.Log($"Distance from center to the floor: {distanceToFloor}");

                if (distanceToFloor > heightThreshold)
                {
                    Debug.Log("Center is too high above the floor and is being destroyed.");
                    Destroy(centerInstance);
                    centerInstance = null;
                    // Destroy the Kitsune when the center is too high above the floor
                    if (kitsuneInstance != null)
                    {
                        Destroy(kitsuneInstance);
                        kitsuneInstance = null;
                    }
                    yield break; // Exit the coroutine
                }
            }
            else
            {
                Debug.LogWarning("Center: No floor detected within raycast distance.");
            }

            // Update elapsed time
            elapsedTime += Time.deltaTime;

            yield return null; // Wait for the next frame
        }

        // Destroy the center after the path has finished growing if it is still alive
        if (centerInstance != null)
        {
            Destroy(centerInstance);
            Destroy(kitsuneInstance);
        }
    }

    private IEnumerator MoveKitsune(float speed, Vector3 leftOffset, Vector3 backOffset)
    {
        float elapsedTime = 0f;
        bool movingToOtherSide = false;
        bool isLeftSide = true;
        Vector3 currentOffset = leftOffset;

        while (kitsuneInstance != null && elapsedTime < growthDuration)
        {
            if (centerInstance == null)
            {
                yield break; // Exit if the center is destroyed
            }

            // Move the Kitsune forward
            kitsuneInstance.transform.Translate(Vector3.forward * Time.deltaTime * speed);

            // Smoothly transition to the other side if necessary
            if (movingToOtherSide)
            {
                float t = Mathf.Min(elapsedTime / 0.25f, 1f);
                kitsuneInstance.transform.position = centerInstance.transform.position + Vector3.Lerp(currentOffset, -currentOffset, t) - backOffset;

                if (t >= 1f)
                {
                    movingToOtherSide = false;
                    isLeftSide = !isLeftSide;
                    currentOffset = -currentOffset;
                }
            }
            else
            {
                kitsuneInstance.transform.position = centerInstance.transform.position + currentOffset - backOffset;

                // Check for obstacles in front of the Kitsune
                RaycastHit hit;
                if (Physics.Raycast(kitsuneInstance.transform.position, kitsuneInstance.transform.forward, out hit, 2f))
                {
                    if (hit.collider.CompareTag(obstacleTag))
                    {
                        if (movingToOtherSide)
                        {
                            Debug.Log("Kitsune hit an obstacle while transitioning and is being destroyed.");
                            Destroy(kitsuneInstance);
                            kitsuneInstance = null;
                            yield break; // Exit the coroutine
                        }
                        else
                        {
                            Debug.Log("Kitsune hit an obstacle and is moving to the opposite side.");
                            movingToOtherSide = true;
                            elapsedTime = 0f; // Reset elapsed time for smooth transition
                        }
                    }
                }
            }

            // Check the distance to the floor
            RaycastHit floorHit;
            if (Physics.Raycast(kitsuneInstance.transform.position, Vector3.down, out floorHit, Mathf.Infinity))
            {
                float distanceToFloor = floorHit.distance;
                Debug.Log($"Distance from Kitsune to the floor: {distanceToFloor}");

                if (distanceToFloor > heightThreshold)
                {
                    Debug.Log("Kitsune is too high above the floor and is being destroyed.");
                    Destroy(kitsuneInstance);
                    kitsuneInstance = null;
                    yield break;
                }
            }
            else
            {
                Debug.LogWarning("Kitsune: No floor detected within raycast distance.");
            }

            // Update elapsed time for transition
            if (movingToOtherSide)
            {
                elapsedTime += Time.deltaTime;
            }

            yield return null;
        }

        // Destroy the Kitsune after the path has finished growing if it is still alive
        if (kitsuneInstance != null)
        {
            Destroy(kitsuneInstance);
        }
    }


    private IEnumerator GrowPath(GameObject rushPath, Transform pathColTransform)
    {
        float elapsedTime = 0f;
        float initialLength = 0f;

        while (elapsedTime < growthDuration && initialLength < maxPathLength)
        {
            if (centerInstance == null)
            {
                break; // Exit the loop if the center is destroyed
            }

            // Calculate the amount to grow based on time
            float growthAmount = (maxPathLength / growthDuration) * Time.deltaTime;

            // Ensure the path does not exceed the maximum length
            float newLength = Mathf.Min(initialLength + growthAmount, maxPathLength);

            // Adjust the path's scale or length accordingly (assuming the path grows along the Z axis)
            rushPath.transform.localScale = new Vector3(rushPath.transform.localScale.x, rushPath.transform.localScale.y, newLength);

            // Update the initial length for the next iteration
            initialLength = newLength;

            // Update elapsed time
            elapsedTime += Time.deltaTime;
            
            yield return null;
        }

        // Wait till ability runs out
        yield return new WaitForSeconds(pathLifetimeAfterGrowth);

        // Destroy the path after it has finished growing and the ability runs out
        Destroy(rushPath);

        // Reset player speed after the path is destroyed
        ResetPlayerSpeed();
    }
}
