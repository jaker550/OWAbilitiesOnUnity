using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TumbleweedMovement : MonoBehaviour
{
    public float tumbleSpeed = 50f;    // Speed at which the tumbleweed rolls
    private Vector3 windDirection;     // Wind direction vector
    private float windForce = 20f;     // Force of the wind pushing the tumbleweed
    private Rigidbody rb;              // Rigidbody component for physics movement

    public float drag = 1f;            // Linear drag to slow down movement
    public float angularDrag = 2f;     // Angular drag to slow down tumbling
    public float destroyAfterSeconds = 7f; // Time after which the tumbleweed will be destroyed

    void Start()
    {
        // Ensure the tumbleweed has a Rigidbody for physics-based movement
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.isKinematic = false;  // Ensure physics is applied

        // Set drag values to slow down over time
        rb.linearDamping = drag;             // Slows linear movement
        rb.angularDamping = angularDrag;   // Slows down rotation

        // Apply an initial force based on wind direction and force
        ApplyInitialForce();

        // Start coroutine to destroy the tumbleweed after a delay
        StartCoroutine(DestroyAfterTime(destroyAfterSeconds));
    }

    // Set the wind direction from the ability script
    public void SetWindDirection(Vector3 direction)
    {
        windDirection = direction.normalized;
    }

    // Set the wind force from the ability script
    public void SetWindForce(float force)
    {
        windForce = force;
    }

    private void ApplyInitialForce()
    {
        // Apply an initial wind force to the tumbleweed
        Vector3 windForceVector = windDirection * windForce;
        rb.AddForce(windForceVector, ForceMode.VelocityChange);

        // Apply an initial rolling effect
        ApplyInitialRollingEffect();
    }

    private void ApplyInitialRollingEffect()
    {
        // Apply an initial torque to simulate tumbling as the object moves
        Vector3 rollAxis = Vector3.Cross(windDirection, Vector3.up).normalized;  // Rolling axis perpendicular to wind direction and upwards
        float rollAngle = tumbleSpeed;  // Set the initial rolling speed

        // Apply the torque to the Rigidbody
        rb.AddTorque(rollAxis * rollAngle, ForceMode.VelocityChange);
    }

    // Coroutine to destroy the tumbleweed after a specified time
    IEnumerator DestroyAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}
