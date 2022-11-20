using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathing
{
    [System.Serializable]
    public class WaypointCluster
    {
        public byte Id;
        public List<ushort> Waypoints = new List<ushort>();
        public ushort ClusterCore;
        public List<byte> ConnectedClusters = new List<byte>();

        private List<byte> _clusterSearcHistory = new List<byte>();
        public List<byte> History { get => _clusterSearcHistory; set => _clusterSearcHistory = value; }

        public WaypointCluster(int id)
        {
            Id = (byte)id;
        }

        public void FindNewCore()
        {
            Vector3 averagePos = new Vector3();
            foreach (ushort wp in Waypoints)
            {
                AiWaypoint waypoint = WaypointManager.Instance.GetWaypoint(wp);
                averagePos += waypoint.Position;
            }
            averagePos.x = averagePos.x / Waypoints.Count;
            averagePos.y = averagePos.y / Waypoints.Count;
            averagePos.z = averagePos.z / Waypoints.Count;
            float closestDist = Mathf.Infinity;
            ushort closest = ushort.MaxValue;
            foreach (ushort wp in Waypoints)
            {
                AiWaypoint waypoint = WaypointManager.Instance.GetWaypoint(wp);
                float dist = Vector3.Distance(waypoint.Position, averagePos);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = wp;
                }
            }
            ClusterCore = closest;
        }

        public void FindConnectedClusters()
        {
            Waypoints = new List<ushort>();
            ConnectedClusters = new List<byte>();
            foreach (AiWaypoint wp in WaypointManager.Instance.Waypoints)
            {
                if (wp.Cluster == Id)
                {
                    Waypoints.Add(wp.Id);
                }
            }

            foreach (ushort wp in Waypoints)
            {
                AiWaypoint waypoint = WaypointManager.Instance.GetWaypoint(wp);
                foreach (byte c in waypoint.Connections)
                {
                    AiWaypoint connection = WaypointManager.Instance.GetWaypoint(c);
                    byte cl = connection.Cluster;
                    if (cl != Id)
                    {
                        WaypointCluster otherCluster = WaypointManager.Instance.GetCluster(cl);
                        if (!ConnectedClusters.Contains(cl))
                        {
                            ConnectedClusters.Add(cl);
                        }
                        if (!otherCluster.ConnectedClusters.Contains(Id))
                        {
                            otherCluster.ConnectedClusters.Add(Id);
                        }
                    }
                }
            }
        }
    }
}