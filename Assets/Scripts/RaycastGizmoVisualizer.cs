using UnityEngine;

public class RaycastGizmoVisualizer : MonoBehaviour
{
    public Vector3 origin;
    public Vector3 direction;
    public float length;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(origin, direction * length);
    }
}
