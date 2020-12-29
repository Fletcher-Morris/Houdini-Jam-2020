using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiWaypoint
{
    public List<AiWaypoint> connections = default;

    public List<AiWaypoint> history = default;

    public int id;

    public int cluster;
    public List<AiWaypoint> clusterConnections = default;

    public Vector3 position = default;

    public AiWaypoint(Vector3 pos)
    {
        position = pos;
        connections = new List<AiWaypoint>();
        history = new List<AiWaypoint>();
        id = -1;
        cluster = -1;
        clusterConnections = new List<AiWaypoint>();
    }

    public void Remove()
    {
        connections.ForEach(w => w.connections.Remove(this));
        connections.ForEach(w => w.clusterConnections.Remove(this));
        WaypointManager.Instance.waypointClusters[cluster].Remove(this);
        WaypointManager.Instance.Waypoints.Remove(this);
    }
}

[System.Serializable]
public class WaypointPath
{
    public AiWaypoint A;
    public AiWaypoint B;
    [HideInInspector] public List<AiWaypoint> Path;

    public WaypointPath(AiWaypoint a, AiWaypoint b, List<AiWaypoint> path)
    {
        A = a;
        B = b;
        Path = path;
    }
}