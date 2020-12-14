using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AiWaypoint : MonoBehaviour
{
    public List<AiWaypoint> connections;

    public List<AiWaypoint> history;

    public int id;

    public int cluster;

    public void Remove()
    {
        connections.ForEach(w => w.connections.Remove(this));
        WaypointManager.Instance.Waypoints.Remove(this);
        DestroyImmediate(gameObject);
    }
}
