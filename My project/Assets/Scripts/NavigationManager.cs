using System.Collections.Generic;
using UnityEngine;

public class NavigationManager : MonoBehaviour
{
    [Header("Instructor Setup")]
    [Tooltip("Drag the XR Origin / VR player object here (the thing that moves).")]
    public Transform playerRoot;

    [Tooltip("Assign all destination trigger objects here.")]
    public List<DestinationTrigger> destinations = new List<DestinationTrigger>();

    [Tooltip("Instructor chooses the destination ID to grade (must match a DestinationTrigger.destinationId).")]
    public string assignedDestinationId = "Radiology";

    [Header("Turn Tracking")]
    [Tooltip("Minimum yaw change (degrees) to count as a turn.")]
    public float turnAngleThreshold = 30f;

    [Tooltip("Seconds between turn counts to avoid double counting.")]
    public float turnCooldownSeconds = 0.35f;

    [Header("Distance / Steps")]
    [Tooltip("Meters per step used to estimate steps from distance. Adjust if needed.")]
    public float metersPerStep = 0.75f;

    // Runtime
    private bool isRunning;
    private float startTime;
    private float elapsedTime;

    private Vector3 lastPos;
    private float distanceMeters;

    private Vector3 lastForwardFlat;
    private int turnCount;
    private float lastTurnTime;

    private DestinationTrigger activeDestination;

    void Start()
    {
        BeginRun();
    }

    void Update()
    {
        if (!isRunning || playerRoot == null) return;

        // Time
        elapsedTime = Time.time - startTime;

        // Distance (steps)
        Vector3 currentPos = playerRoot.position;
        distanceMeters += Vector3.Distance(currentPos, lastPos);
        lastPos = currentPos;

        // Turns (yaw)
        Vector3 fwd = playerRoot.forward;
        fwd.y = 0f;

        if (fwd.sqrMagnitude > 0.0001f)
        {
            fwd.Normalize();
            float angle = Vector3.Angle(lastForwardFlat, fwd);

            if (angle >= turnAngleThreshold && (Time.time - lastTurnTime) >= turnCooldownSeconds)
            {
                turnCount++;
                lastTurnTime = Time.time;
                lastForwardFlat = fwd;
            }
        }
    }

    public void BeginRun()
    {
        if (playerRoot == null)
        {
            Debug.LogError("NavigationManager: playerRoot not assigned.");
            return;
        }

        // Find assigned destination
        activeDestination = null;
        foreach (var d in destinations)
        {
            if (d != null && d.destinationId == assignedDestinationId)
            {
                activeDestination = d;
                break;
            }
        }

        if (activeDestination == null)
        {
            Debug.LogError($"NavigationManager: No destination found with ID '{assignedDestinationId}'.");
            return;
        }

        // Reset metrics
        isRunning = true;
        startTime = Time.time;
        elapsedTime = 0f;

        lastPos = playerRoot.position;
        distanceMeters = 0f;

        Vector3 fwd = playerRoot.forward; fwd.y = 0f;
        lastForwardFlat = (fwd.sqrMagnitude > 0.0001f) ? fwd.normalized : Vector3.forward;

        turnCount = 0;
        lastTurnTime = -999f;

        Debug.Log($"Run started. Assigned destination: {assignedDestinationId}");
    }

    public void EndRun()
    {
        if (!isRunning) return;
        isRunning = false;

        float steps = (metersPerStep > 0f) ? (distanceMeters / metersPerStep) : 0f;

        Debug.Log("=== RUN COMPLETE ===");
        Debug.Log($"Destination: {assignedDestinationId}");
        Debug.Log($"Time (s): {elapsedTime:F2}");
        Debug.Log($"Distance (m): {distanceMeters:F2}");
        Debug.Log($"Estimated steps: {steps:F0}");
        Debug.Log($"Turns: {turnCount}");
    }

    // Call this from triggers
    public void NotifyReachedDestination(DestinationTrigger reached)
    {
        if (!isRunning) return;
        if (reached == null) return;

        // Only end if it's the assigned destination
        if (reached.destinationId == assignedDestinationId)
        {
            EndRun();
        }
    }
}
