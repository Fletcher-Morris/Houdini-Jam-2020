using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AiWaypoint
{
    [SerializeField] private ushort cluster;
    [SerializeField] private ushort id;
    private List<ushort> clusterConnections;
    private List<ushort> connections;
    private List<ushort> history;

    [SerializeField] private Vector3 position;

    public List<ushort> History { get => history; set => history = value; }
    public List<ushort> Connections { get => connections; set => connections = value; }
    public List<ushort> ClusterConnections { get => clusterConnections; set => clusterConnections = value; }
    public ushort Cluster { get => cluster; set => cluster = value; }
    public ushort Id { get => id; set => id = value; }
    public Vector3 Position { get => position; set => position = value; }

    public AiWaypoint(Vector3 pos)
	{
        Position = pos;
        Connections = new List<ushort>();
        History = new List<ushort>();
        Id = 0;
        Cluster = 0;
        ClusterConnections = new List<ushort>();
	}

	public void Remove()
	{
		Connections.ForEach(w => WaypointManager.GetWaypoint(w).Connections.Remove(id));
		Connections.ForEach(w => WaypointManager.GetWaypoint(w).ClusterConnections.Remove(id));
		WaypointManager.Instance.WaypointClusters[Cluster].Remove(id);
		WaypointManager.Instance.Waypoints.Remove(this);
	}
}

public class WaypointPath
{
	public ushort A;
	public ushort B;
	[HideInInspector] public List<ushort> Path;

	public WaypointPath(ushort a, ushort b, List<ushort> path)
	{
        A = a;
        B = b;
        Path = path;
	}
}