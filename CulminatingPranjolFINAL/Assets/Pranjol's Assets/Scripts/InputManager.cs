using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    internal enum driver
    {
        AI,
        keyboard
    }

    [SerializeField] driver driveController;

    public float vertical;
    public float horizontal;
    public bool handbrake;
    public bool boosting;

    public TrackWayPoints wayPoints;
    public Transform currentWaypoint;
    public List<Transform> nodes = new List<Transform>();
    public int distanceOffset = 3;
    private float steerForce = 1;
    public float acceleration = 0.5f;
    public int currentNode;

    private void Awake()
    {
        wayPoints = GameObject.FindGameObjectWithTag("path").GetComponent<TrackWayPoints>();
        nodes = wayPoints.nodes;
    }

    private void FixedUpdate()
    {
        if (gameObject.tag == "AI")
        {
            AIDrive();
        }
        else if (gameObject.tag == "Player")
        {
            calculateWaypointDistances();
            keyboardDrive();
        }
    }

    // AI-controlled vehicle logic
    private void AIDrive()
    {
        calculateWaypointDistances();
        AISteer();
        vertical = acceleration;
    }

    // Keyboard-controlled vehicle logic
    private void keyboardDrive()
    {
        vertical = Input.GetAxis("Vertical");
        horizontal = Input.GetAxis("Horizontal");
        handbrake = (Input.GetAxis("Jump") != 0) ? true : false;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            boosting = true;
        }
        else
        {
            boosting = false;
        }
    }

    // Calculate distances to waypoints for AI steering
    private void calculateWaypointDistances()
    {
        Vector3 position = gameObject.transform.position;
        float distance = Mathf.Infinity;

        for (int i = 0; i < nodes.Count; i++)
        {
            Vector3 difference = nodes[i].transform.position - position;
            float currentDistance = difference.magnitude;
            if (currentDistance < distance)
            {
                if ((i + distanceOffset) >= nodes.Count)
                {
                    currentWaypoint = nodes[1];
                    distance = currentDistance;
                }
                else
                {
                    currentWaypoint = nodes[i + distanceOffset];
                    distance = currentDistance;
                }
                currentNode = i;
            }
        }
    }

    // AI steering logic
    private void AISteer()
    {
        Vector3 relative = transform.InverseTransformPoint(currentWaypoint.transform.position);
        relative /= relative.magnitude;

        horizontal = (relative.x / relative.magnitude) * steerForce;
    }

    // Draw a wire sphere to visualize the current waypoint
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(currentWaypoint.position, 3);
    }
}