using UnityEngine;

public class FollowMainCamera : MonoBehaviour
{
    public Transform mainCamera; // Reference to the main camera's transform

    void LateUpdate()
    {
        if (mainCamera != null)
        {
            // Match the position and rotation of the second camera to the main camera
            transform.position = mainCamera.position;
            transform.rotation = mainCamera.rotation;
        }
    }
}
