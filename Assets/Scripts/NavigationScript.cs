using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavigationScript : MonoBehaviour
{
    public Transform point1;
    public Transform point2;
    private byte currentPoint = 1;
    private NavMeshAgent agent;
    private float waypointTolerance = 0.5f; // Tolerance for reaching waypoints
    private Vector3 lastDestination; // Track the last destination

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError("NavMeshAgent component is missing from this GameObject.");
            return;
        }

        // Set initial destination
        lastDestination = point1.position;
        SetDestination(lastDestination);
    }

    void Update()
    {
        if (agent == null || !agent.isActiveAndEnabled) return; // Exit if agent is not active

        // Check if agent has reached its destination with tolerance
        if (Vector3.Distance(transform.position, point1.position) < waypointTolerance && currentPoint == 1)
        {
            currentPoint = 2;
            lastDestination = point2.position;
            SetDestination(lastDestination);
        }
        else if (Vector3.Distance(transform.position, point2.position) < waypointTolerance && currentPoint == 2)
        {
            currentPoint = 1;
            lastDestination = point1.position;
            SetDestination(lastDestination);
        }
    }

    // Method to set the destination and ensure the agent is moving
    private void SetDestination(Vector3 destination)
    {
        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.destination = destination;
            agent.isStopped = false; // Ensure the agent is not stopped
            Debug.Log($"Destination set to: {destination}");
        }
        else
        {
            Debug.LogWarning("NavMeshAgent is not active or enabled. Cannot set destination.");
        }
    }

    // Method to disable and re-enable the agent, ensuring it resumes movement
    public void ToggleNavMeshAgent(bool enable)
    {
        if (agent != null)
        {
            if (enable)
            {
                agent.enabled = true;

                // Re-set the last destination after enabling the agent
                SetDestination(lastDestination);
            }
            else
            {
                agent.enabled = false;
            }
        }
    }

    // Call this method to resume normal movement after pulling
    public void ResumeMovement()
    {
        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.isStopped = false;
            SetDestination(lastDestination);
            Debug.Log("Movement resumed.");
        }
        else
        {
            Debug.LogWarning("NavMeshAgent is not active or enabled. Cannot resume movement.");
        }
    }
}
