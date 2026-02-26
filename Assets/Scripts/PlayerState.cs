using UnityEngine;

[System.Serializable]
public struct PlayerState
{
    public Vector3 position;
    public Quaternion rotation;

    public PlayerState(Vector3 pos, Quaternion rot)
    {
        position = pos;
        rotation = rot;
    }
}
