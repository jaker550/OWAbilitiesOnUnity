using System.Collections;
using UnityEngine;

public class Drone : MonoBehaviour
{
    public GameObject player;
    public GameObject snow;
    private float riseSpeed;      // Speed at which the drone rises
    private float riseHeight;     // How high the drone should rise
    private float destroyTime;    // Time before drone is destroyed after reaching peak height
    private Coroutine riseCoroutine;  // Reference to the rise coroutine
    private Rigidbody droneRigidbody; // Reference to the drone's Rigidbody

    // Method to initialize the drone with parameters for rise height, speed, and destroy time
    public void Initialize(GameObject player, float riseHeight, float riseSpeed, float destroyTime)
    {
        this.player = player;
        this.riseHeight = riseHeight;
        this.riseSpeed = riseSpeed;
        this.destroyTime = destroyTime;
        Debug.Log("Drone initialized with rising parameters.");

        // Get the Rigidbody component
        droneRigidbody = GetComponent<Rigidbody>();
    }

    // Handle collision events
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Drone collided with tag {collision.gameObject.tag}");

        if (player != null && collision.gameObject.CompareTag("Ground"))
        {
            Debug.Log("Touched ground");

            // Start the rising coroutine
            if (riseCoroutine != null)
            {
                StopCoroutine(riseCoroutine);
            }
            riseCoroutine = StartCoroutine(RiseUp());
        }
        else
        {
            Debug.Log("Player or ground not found!");
        }
    }

    // Coroutine to make the drone rise and then destroy after a delay
    private IEnumerator RiseUp()
    {
        Vector3 initialPosition = transform.position;
        Vector3 targetPosition = initialPosition + Vector3.up * riseHeight;

        // Disable gravity and zero out the velocity
        if (droneRigidbody != null)
        {
            droneRigidbody.useGravity = false;
            droneRigidbody.linearVelocity = Vector3.zero; // Stop any existing movement
            droneRigidbody.angularVelocity = Vector3.zero; // Stop any existing rotation

            // Freeze the X and Z axes
            droneRigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        }

        float elapsedTime = 0f;
        while (elapsedTime < riseHeight / riseSpeed)
        {
            // Smoothly move the drone upwards
            transform.position = Vector3.Lerp(initialPosition, targetPosition, elapsedTime / (riseHeight / riseSpeed));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure the drone reaches the target height
        transform.position = targetPosition;

        var newSnow = Instantiate(snow, this.gameObject.transform.position, Quaternion.identity);
        newSnow.transform.SetParent(this.gameObject.transform);
        newSnow.transform.position = initialPosition;

        // Wait for the specified destroy time before destroying the drone
        yield return new WaitForSeconds(destroyTime);

        Destroy(gameObject);
    }
}
