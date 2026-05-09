using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds the ordered list of waypoints that define the AI racing line.
/// Rebuilds <see cref="nodes"/> from child Transforms so adding/removing
/// waypoints in the hierarchy is reflected automatically in the editor view.
/// </summary>
public class TrackWayPoints : MonoBehaviour
{
    [SerializeField] private Color  lineColor    = Color.yellow;
    [Range(0f, 5f)]
    [SerializeField] private float  sphereRadius = 1f;

    public List<Transform> nodes = new List<Transform>();

    private void Awake() => RebuildNodes();

    private void OnDrawGizmosSelected()
    {
        RebuildNodes();
        DrawGizmos();
    }

    private void RebuildNodes()
    {
        Transform[] children = GetComponentsInChildren<Transform>();
        nodes = new List<Transform>(children.Length - 1);

        for (int i = 1; i < children.Length; i++)   // skip index 0 (self)
            nodes.Add(children[i]);
    }

    private void DrawGizmos()
    {
        if (nodes.Count == 0) return;

        Gizmos.color = lineColor;
        for (int i = 0; i < nodes.Count; i++)
        {
            Vector3 current  = nodes[i].position;
            Vector3 previous = nodes[(i == 0) ? nodes.Count - 1 : i - 1].position;

            Gizmos.DrawLine(previous, current);
            Gizmos.DrawSphere(current, sphereRadius);
        }
    }
}
