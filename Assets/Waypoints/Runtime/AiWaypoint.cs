using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AiWaypoint
{
    [SerializeField] private ushort _cluster;
    [SerializeField] private ushort _id;
    private List<ushort> _clusterConnections = new List<ushort>();
    private List<ushort> _connections = new List<ushort>();
    private List<ushort> _history = new List<ushort>();
    [SerializeField] private Vector3 _position;

    public List<ushort> History { get => _history; set => _history = value; }
    public List<ushort> Connections { get => _connections; set => _connections = value; }
    public List<ushort> ClusterConnections { get => _clusterConnections; set => _clusterConnections = value; }
    public ushort Cluster { get => _cluster; set => _cluster = value; }
    public ushort Id { get => _id; set => _id = value; }
    public Vector3 Position { get => _position; set => _position = value; }

    public AiWaypoint(Vector3 pos)
	{
        _position = pos;
        _connections = new List<ushort>();
        _history = new List<ushort>();
        _id = 0;
        _cluster = 0;
        _clusterConnections = new List<ushort>();
	}

	public void Remove()
	{
		_connections.ForEach(w => WaypointManager.GetWaypoint(w).Connections.Remove(_id));
		_connections.ForEach(w => WaypointManager.GetWaypoint(w).ClusterConnections.Remove(_id));
		WaypointManager.Instance.Clusters[Cluster].Waypoints.Remove(_id);
		WaypointManager.Instance.RemoveWaypoint(this);
	}
}

public class WaypointPath
{
	public ushort _a;
	public ushort _b;
	[HideInInspector] public List<ushort> Path;

	public WaypointPath(ushort a, ushort b, List<ushort> path)
	{
        _a = a;
        _b = b;
        Path = path;
	}
}