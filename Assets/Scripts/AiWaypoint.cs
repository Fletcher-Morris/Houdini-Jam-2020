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