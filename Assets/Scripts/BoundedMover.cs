using UnityEngine;

/// <summary>
/// Moves this GameObject in a wandering pattern, constrained to a box region
/// defined by a center Transform plus per-axis inspector-settable ranges.
///
/// Set an axis range to 0 to lock movement on that axis entirely,
/// effectively reducing motion to 2D or even 1D.
///
/// Inspector fields:
///   centerTransform  – The Transform whose position acts as the region center.
///                      Leave null to use the world origin (0,0,0).
///   rangeX           – Half-extent on X. Set to 0 to lock X.
///   rangeY           – Half-extent on Y. Set to 0 to lock Y.
///   rangeZ           – Half-extent on Z. Set to 0 to lock Z.
///   moveSpeed        – How fast the object travels toward each waypoint.
///   waypointRadius   – How close the object must get before picking a new waypoint.
///   changeInterval   – Max seconds before forcing a new waypoint (safety net).
/// </summary>
public class BoundedMover : MonoBehaviour
{
    [Header("Region Center")]
    [Tooltip("The object whose position defines the center of the movement region. Leave empty to use world origin.")]
    public Transform centerTransform;

    [Header("Movement Bounds — Half-Extents From Center (0 = locked axis)")]
    [Tooltip("Half-extent on the X axis. Set to 0 to disable X movement.")]
    public float rangeX = 5f;

    [Tooltip("Half-extent on the Y axis. Set to 0 to disable Y movement.")]
    public float rangeY = 3f;

    [Tooltip("Half-extent on the Z axis. Set to 0 to disable Z movement.")]
    public float rangeZ = 0f;

    [Header("Movement Settings")]
    [Tooltip("Movement speed in units per second.")]
    public float moveSpeed = 2f;

    [Tooltip("Distance to the waypoint at which a new one is chosen.")]
    public float waypointRadius = 0.2f;

    [Tooltip("Maximum seconds to travel toward a waypoint before forcing a new one.")]
    public float changeInterval = 4f;

    // ── private state ──────────────────────────────────────────────────────────
    private Vector3 _currentWaypoint;
    private float   _waypointTimer;

    // ── Unity lifecycle ────────────────────────────────────────────────────────

    private void Start()
    {
        PickNewWaypoint();
    }

    private void Update()
    {
        // If the center is moving, re-clamp waypoint every frame so it
        // stays inside the (possibly shifted) bounds.
        _currentWaypoint = ClampToBounds(_currentWaypoint);

        // Move toward waypoint
        transform.position = Vector3.MoveTowards(
            transform.position,
            _currentWaypoint,
            moveSpeed * Time.deltaTime
        );

        // Tick the safety timer
        _waypointTimer -= Time.deltaTime;

        // Pick a new waypoint when we arrive or time runs out
        bool arrived  = Vector3.Distance(transform.position, _currentWaypoint) <= waypointRadius;
        bool timedOut = _waypointTimer <= 0f;

        if (arrived || timedOut)
        {
            PickNewWaypoint();
        }
    }

    // ── Gizmos ─────────────────────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        Vector3 center = GetCenter();

        // Size of the gizmo box — locked axes show as flat (no depth on that axis)
        Vector3 boxSize = new Vector3(
            Mathf.Max(rangeX * 2f, 0.01f),   // never truly zero so gizmo is visible
            Mathf.Max(rangeY * 2f, 0.01f),
            Mathf.Max(rangeZ * 2f, 0.01f)
        );

        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.4f);
        Gizmos.DrawWireCube(center, boxSize);

        // Mark locked axes with a small red cross-slab so it's obvious
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.25f);
        if (rangeX == 0f) Gizmos.DrawWireCube(center, new Vector3(0.02f, boxSize.y, boxSize.z));
        if (rangeY == 0f) Gizmos.DrawWireCube(center, new Vector3(boxSize.x, 0.02f, boxSize.z));
        if (rangeZ == 0f) Gizmos.DrawWireCube(center, new Vector3(boxSize.x, boxSize.y, 0.02f));

        // Draw current waypoint while playing
        if (Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(_currentWaypoint, 0.15f);
            Gizmos.DrawLine(transform.position, _currentWaypoint);
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    /// <summary>Returns the world-space center of the bounding region.</summary>
    private Vector3 GetCenter()
    {
        return centerTransform != null ? centerTransform.position : Vector3.zero;
    }

    /// <summary>
    /// Picks a random point inside the bounding box.
    /// Axes with range == 0 are locked to the object's current position on that axis.
    /// </summary>
    private Vector3 RandomPointInBounds()
    {
        Vector3 center = GetCenter();

        // For locked axes, stay at the object's current position (not the center),
        // so you can manually place the object on any locked-axis value you like.
        float x = rangeX > 0f
            ? Random.Range(center.x - rangeX, center.x + rangeX)
            : transform.position.x;

        float y = rangeY > 0f
            ? Random.Range(center.y - rangeY, center.y + rangeY)
            : transform.position.y;

        float z = rangeZ > 0f
            ? Random.Range(center.z - rangeZ, center.z + rangeZ)
            : transform.position.z;

        return new Vector3(x, y, z);
    }

    /// <summary>
    /// Clamps a world-space point so it stays within the current bounding box.
    /// Locked axes (range == 0) pass through unchanged.
    /// </summary>
    private Vector3 ClampToBounds(Vector3 point)
    {
        Vector3 center = GetCenter();

        float x = rangeX > 0f
            ? Mathf.Clamp(point.x, center.x - rangeX, center.x + rangeX)
            : point.x;

        float y = rangeY > 0f
            ? Mathf.Clamp(point.y, center.y - rangeY, center.y + rangeY)
            : point.y;

        float z = rangeZ > 0f
            ? Mathf.Clamp(point.z, center.z - rangeZ, center.z + rangeZ)
            : point.z;

        return new Vector3(x, y, z);
    }

    /// <summary>Selects a new random waypoint and resets the safety timer.</summary>
    private void PickNewWaypoint()
    {
        _currentWaypoint = RandomPointInBounds();
        _waypointTimer   = changeInterval;
    }
}