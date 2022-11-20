using System;
using System.Collections.Generic;
using Sirenix.Serialization;
using UnityEngine;

namespace Pathing
{
    [System.Serializable]
    public class AiWaypoint : IAiTarget
    {
        [SerializeField] private int _id;
        [SerializeField] private int _cluster = -1;
        [SerializeField] private Vector3 _position;
        [SerializeField, HideInInspector] private List<int> _connections = new List<int>();
        [SerializeField, HideInInspector] private List<int> _history = new List<int>();

        public List<int> History { get => _history; set => _history = value; }
        public List<int> Connections { get => _connections; set => _connections = value; }
        public int Cluster { get => _cluster; set => _cluster = value; }
        public int Id { get => _id; set => _id = value; }
        public Vector3 Position { get => _position; set => _position = value; }

        public AiWaypoint(Vector3 pos)
        {
            _position = pos;
            _connections = new List<int>();
            _history = new List<int>();
            _id = 0;
            _cluster = -1;
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
        [OdinSerialize] private System.Tuple<int, int> t = new System.Tuple<int, int>(int.MaxValue, int.MaxValue);
        [HideInInspector] public List<int> Path;

        public int Start { get => t.Item1; set => t = new System.Tuple<int, int>(value, t.Item2); }
        public int End { get => t.Item2; set => t = new System.Tuple<int, int>(t.Item1, value); }

        public WaypointPath(int start, int stop, List<int> path)
        {
            t = new System.Tuple<int, int>(start, stop);
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