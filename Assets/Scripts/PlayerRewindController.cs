using System.Collections.Generic;
using UnityEngine;

public class PlayerRewindController : MonoBehaviour
{
    public float rewindDuration = 3f; // Duration to rewind
    public float rewindSpeedMultiplier = 2f; // Multiplier to control the speed of rewind
    public List<PlayerState> states = new List<PlayerState>(); // List to store states
    private bool isRewinding = false; // Flag to check if rewinding
    private float rewindStartTime; // Time when rewind started

    void Start()
    {
        states = new List<PlayerState>();
    }

    void FixedUpdate()
    {
        if (!isRewinding)
        {
            RecordState();
        }
        else
        {
            PerformRewind();
        }
    }

    void RecordState()
    {
        PlayerState currentState = new PlayerState(transform.position, transform.rotation);
        states.Insert(0, currentState);

        // Limit the size of the list to the necessary number of states
        int maxStates = Mathf.RoundToInt(rewindDuration / Time.fixedDeltaTime);
        if (states.Count > maxStates)
        {
            states.RemoveAt(states.Count - 1);
        }
    }

    public void StartRewind(float duration)
    {
        if (states.Count == 0) return;

        isRewinding = true;
        rewindDuration = duration;
        rewindStartTime = Time.time;

        // Debug log starting rewind
        Debug.Log("Starting Rewind");
    }

    void PerformRewind()
    {
        float elapsed = Time.time - rewindStartTime;

        if (elapsed >= rewindDuration || states.Count == 0)
        {
            isRewinding = false;
            Debug.Log("Rewind Ended");
            return;
        }

        // Calculate the number of states to skip per frame to speed up the rewind
        int statesToSkip = Mathf.CeilToInt(rewindSpeedMultiplier * (elapsed / rewindDuration) * states.Count);

        // Ensure statesToSkip is at least 1
        statesToSkip = Mathf.Max(1, statesToSkip);

        // Set the player's position and rotation to the next state after skipping
        int index = Mathf.Min(statesToSkip - 1, states.Count - 1);
        transform.position = states[index].position;
        transform.rotation = states[index].rotation;

        // Remove the used states from the list
        states.RemoveRange(0, statesToSkip);

        // Debug log the rewinding state
        Debug.Log($"Rewinding to Position: {transform.position}, Rotation: {transform.rotation.eulerAngles}");
    }
}
