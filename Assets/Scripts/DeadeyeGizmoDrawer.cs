using UnityEngine;

public class DeadeyeGizmoDrawer : MonoBehaviour
{
    public DeadeyeAbility deadeyeAbility; // Reference to your DeadeyeAbility

    private void OnDrawGizmos()
    {
        if (deadeyeAbility == null) return;

        Gizmos.color = Color.red; // Default color

        // Ensure visibilityRays are not null or empty
        if (deadeyeAbility.visibilityRays != null && deadeyeAbility.visibilityRays.Count > 0)
        {
            for (int i = 0; i < deadeyeAbility.visibilityRays.Count; i++)
            {
                // Set color based on hit result
                Gizmos.color = deadeyeAbility.rayHits[i] ? Color.green : Color.red;

                // Draw the ray
                Gizmos.DrawRay(deadeyeAbility.visibilityRays[i].origin, deadeyeAbility.visibilityRays[i].direction * deadeyeAbility.detectionRadius);
            }
        }
    }
}
