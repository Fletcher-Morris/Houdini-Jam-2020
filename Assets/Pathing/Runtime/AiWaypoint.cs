using System.Collections.Generic;
using Sirenix.Serialization;
using UnityEngine;

namespace Pathing
{
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
            _connections.ForEach(w => WaypointManager.Instance.GetWaypoint(w).Connections.Remove(_id));
            _connections.ForEach(w => WaypointManager.Instance.GetWaypoint(w).ClusterConnections.Remove(_id));
            WaypointManager.Instance.Clusters[Cluster].Waypoints.Remove(_id);
            WaypointManager.Instance.RemoveWaypoint(this);
        }
    }

    [System.Serializable]
    public class WaypointPath
    {
        [OdinSerialize] private System.Tuple<ushort, ushort> t = new System.Tuple<ushort, ushort>(ushort.MaxValue, ushort.MaxValue);
        [HideInInspector] public List<ushort> Path;

        public ushort Start { get => t.Item1; set => t = new System.Tuple<ushort, ushort>(value, t.Item2); }
        public ushort End { get => t.Item2; set => t = new System.Tuple<ushort, ushort>(t.Item1, value); }

        public WaypointPath(ushort start, ushort stop, List<ushort> path)
        {
            t = new System.Tuple<ushort, ushort>(start, stop);
            Path = path;
        }
    }
}