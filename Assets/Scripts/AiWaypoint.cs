using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiWaypoint : MonoBehaviour
{
    public List<AiWaypoint> connectedWaypoints;

    public void Remove()
    {
        connectedWaypoints.ForEach(w => w.connectedWaypoints.Remove(this));
        WaypointManager.Instance.Waypoints.Remove(this);
        DestroyImmediate(gameObject);
    }
}
