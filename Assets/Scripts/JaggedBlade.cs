using UnityEngine;

public class JaggedBlade : MonoBehaviour
{
    private Rigidbody rb;
    private Collider col;
    public float spinSpeed = 360f; // Speed of the spin in degrees per second
    public JaggedBladeAbility bladeAbility; // Reference to the ability script

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    private void Update()
    {
        // Rotate the blade around its local Y-axis to make it spin
        if (rb != null && !rb.isKinematic)
        {
            transform.Rotate(Vector3.forward, spinSpeed * Time.deltaTime, Space.Self);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the collided object is the player
        if (collision.gameObject.CompareTag("Player"))
        {
            // Do not stick to the player
            return;
        }

        // Make the blade stick to the object it collided with
        transform.SetParent(collision.transform);

        // Stop the blade from moving and spinning further
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        if (col != null)
        {
            col.enabled = false; // Disable collisions
        }

        // Notify the JaggedBladeAbility script
        if (bladeAbility != null)
        {
            bladeAbility.OnBladeHit(collision.gameObject);
        }
    }
}
