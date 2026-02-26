using UnityEngine;

public class PathColGizmo : MonoBehaviour
{
    public float raycastDistance = 10f; // Maximum distance for the raycast
    public Color gizmoColor = Color.red; // Color of the gizmo

    void Update()
    {
        // Perform the downward raycast
        RaycastHit hit;
        Vector3 raycastDirection = Vector3.down; // Raycast direction downward

        if (Physics.Raycast(transform.position, raycastDirection, out hit, raycastDistance))
        {
            // Calculate the distance to the hit point
            float distanceToFloor = hit.distance;
            // Print the distance to the floor
            Debug.Log($"Distance from center to the floor: {distanceToFloor}");
        }
        else
        {
            // If the raycast didn't hit anything, print that no hit was detected
            Debug.Log("centerGizmo: Raycast did not hit any floor.");
        }
    }

    void OnDrawGizmos()
    {
        // Set the gizmo color
        Gizmos.color = gizmoColor;

        // Perform the downward raycast for gizmo drawing
        RaycastHit hit;
        Vector3 raycastDirection = Vector3.down; // Raycast direction downward

        if (Physics.Raycast(transform.position, raycastDirection, out hit, raycastDistance))
        {
            // Draw a line to the hit point
            Gizmos.DrawLine(transform.position, hit.point);
            // Draw a sphere at the hit point
            Gizmos.DrawSphere(hit.point, 0.1f);
        }
        else
        {
            // Draw a line to the maximum raycast distance
            Gizmos.DrawLine(transform.position, transform.position + raycastDirection * raycastDistance);
        }
    }
}
