using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Dual-mode input controller: reads keyboard axes for the human player or
/// runs an autonomous waypoint-following AI for opponent karts.
///
/// AI difficulty scales acceleration and look-ahead distance, producing
/// noticeably different opponent behaviours without separate code paths.
/// </summary>
public class InputManager : MonoBehaviour
{
    // ── Inspector ─────────────────────────────────────────────────────────────

    internal enum DriverType { Keyboard, AI }

    [SerializeField] private DriverType driverType = DriverType.Keyboard;

    public enum AIDifficulty { Easy, Medium, Hard }
    [SerializeField] private AIDifficulty difficulty = AIDifficulty.Medium;

    public TrackWayPoints wayPoints;
    [Tooltip("How many waypoints ahead the AI targets (higher = smoother, less reactive).")]
    public int distanceOffset = 3;

    // ── Runtime state (read by PlayerController) ──────────────────────────────

    public float vertical;
    public float horizontal;
    public bool  handbrake;
    public bool  boosting;
    public int   currentNode;

    public List<Transform> nodes = new List<Transform>();
    public Transform currentWaypoint;

    // ── Difficulty presets ────────────────────────────────────────────────────

    private static readonly float[] AccelerationByDifficulty = { 0.35f, 0.55f, 0.85f };
    private static readonly int[]   LookAheadByDifficulty    = { 2,     3,     5     };

    private float _aiAcceleration;

    // ── Unity messages ────────────────────────────────────────────────────────

    private void Awake()
    {
        GameObject pathObject = GameObject.FindGameObjectWithTag("path");
        if (pathObject != null)
        {
            wayPoints = pathObject.GetComponent<TrackWayPoints>();
            nodes     = wayPoints.nodes;
        }
        else
        {
            Debug.LogError("No GameObject tagged 'path' found — AI navigation disabled.");
        }

        int d          = (int)difficulty;
        _aiAcceleration = AccelerationByDifficulty[d];
        distanceOffset  = LookAheadByDifficulty[d];
    }

    private void FixedUpdate()
    {
        if (nodes.Count == 0) return;

        UpdateCurrentNode();

        if (gameObject.CompareTag("AI"))
            DriveAI();
        else
            DriveKeyboard();
    }

    // ── Player input ──────────────────────────────────────────────────────────

    private void DriveKeyboard()
    {
        vertical   = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");
        handbrake  = Input.GetAxis("Jump") != 0f;
        boosting   = Input.GetKey(KeyCode.LeftShift);
    }

    // ── AI control ────────────────────────────────────────────────────────────

    private void DriveAI()
    {
        SteerTowardWaypoint();

        // Ease off throttle when the required steering is large (sharp corner).
        float steerMagnitude = Mathf.Abs(horizontal);
        vertical = _aiAcceleration * Mathf.Lerp(1f, 0.4f, steerMagnitude);
    }

    private void SteerTowardWaypoint()
    {
        if (currentWaypoint == null) return;

        // Project target into local space; normalise so only direction matters.
        Vector3 local = transform.InverseTransformPoint(currentWaypoint.position).normalized;
        horizontal = local.x;   // [-1, 1] maps directly to steering input
    }

    /// <summary>
    /// Scans all waypoints to find the closest one, then targets the node
    /// <see cref="distanceOffset"/> steps ahead of it.
    /// </summary>
    private void UpdateCurrentNode()
    {
        float minDist = Mathf.Infinity;

        for (int i = 0; i < nodes.Count; i++)
        {
            float dist = (nodes[i].position - transform.position).sqrMagnitude;
            if (dist < minDist)
            {
                minDist     = dist;
                currentNode = i;

                int ahead = (i + distanceOffset) % nodes.Count;
                currentWaypoint = nodes[ahead];
            }
        }
    }

    // ── Editor gizmo ─────────────────────────────────────────────────────────

    private void OnDrawGizmos()
    {
        if (currentWaypoint == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(currentWaypoint.position, 1.5f);
    }
}
