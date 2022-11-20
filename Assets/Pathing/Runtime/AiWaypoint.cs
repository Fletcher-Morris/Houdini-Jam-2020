using System;
using System.Collections.Generic;
using Sirenix.Serialization;
using UnityEngine;

namespace Pathing
{
    [System.Serializable]
    public class AiWaypoint : IAiTarget
    {
        [SerializeField] private ushort _id;
        [SerializeField] private byte _cluster;
        [SerializeField] private Vector3 _position;
        [SerializeField, HideInInspector] private List<ushort> _connections = new List<ushort>();
        [SerializeField, HideInInspector] private List<ushort> _history = new List<ushort>();

        public List<ushort> History { get => _history; set => _history = value; }
        public List<ushort> Connections { get => _connections; set => _connections = value; }
        public byte Cluster { get => _cluster; set => _cluster = value; }
        public ushort Id { get => _id; set => _id = value; }
        public Vector3 Position { get => _position; set => _position = value; }

        public AiWaypoint(Vector3 pos)
        {
            _position = pos;
            _connections = new List<ushort>();
            _history = new List<ushort>();
            _id = 0;
            _cluster = 0;
        }

        void IAiTarget.SetPosition(Vector3 pos)
        {
            _position = pos;
        }

        Vector3 IAiTarget.GetPosition()
        {
            return _position;
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

        public int Length()
        {
            if (Path == null) return 0;
            return Path.Count;
        }

        public float Distance()
        {
            float dist = 0;
            if (Path == null) return 0;
            if (Length() == 0) return 0;
            Vector3 lastPos = WaypointManager.Instance.GetWaypoint(0).Position;
            Path.ForEach(n =>
            {
                AiWaypoint wp = WaypointManager.Instance.GetWaypoint(n);
                Vector3 wpPos = wp.Position;
                dist += Vector3.Distance(lastPos, wpPos);
                lastPos = wpPos;
            });
            return dist;
        }
    }
}