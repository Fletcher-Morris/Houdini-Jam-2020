using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathing
{
    [System.Serializable]
    public class WaypointCluster
    {
        public int Id;
        public List<int> Waypoints = new List<int>();
        public int ClusterCore;
        public List<int> ConnectedClusters = new List<int>();

        private List<int> _claimedWaypoints = new List<int>();
        private List<int> _clusterSearcHistory = new List<int>();

        public List<int> History { get => _clusterSearcHistory; set => _clusterSearcHistory = value; }
        public List<int> ClaimedWaypoints { get => _claimedWaypoints; }

        public WaypointCluster(int id)
        {
            Id = id;
        }

        public bool HasClaimedWaypoint(int wp)
        {
            return _claimedWaypoints.Contains(wp);
        }

        public void ClaimWaypoint(int wp)
        {
            if (!HasClaimedWaypoint(wp))
            {
                _claimedWaypoints.Add(wp);
            }
        }

        public void UnclaimWaypoint(int wp)
        {
            if (HasClaimedWaypoint(wp))
            {
                _claimedWaypoints.Remove(wp);
            }
        }
        public void FindNewCore()
        {
            Vector3 averagePos = new Vector3();
            foreach (int wp in Waypoints)
            {
                AiWaypoint waypoint = WaypointManager.Instance.GetWaypoint(wp);
                averagePos += waypoint.Position;
            }
            averagePos.x = averagePos.x / Waypoints.Count;
            averagePos.y = averagePos.y / Waypoints.Count;
            averagePos.z = averagePos.z / Waypoints.Count;
            float closestDist = Mathf.Infinity;
            int closest = int.MaxValue;
            foreach (int wp in Waypoints)
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
            Waypoints = new List<int>();
            ConnectedClusters = new List<int>();
            foreach (AiWaypoint wp in WaypointManager.Instance.Waypoints)
            {
                if (wp.Cluster == Id)
                {
                    Waypoints.Add(wp.Id);
                }
            }

            foreach (int wp in Waypoints)
            {
                AiWaypoint waypoint = WaypointManager.Instance.GetWaypoint(wp);
                foreach (int c in waypoint.Connections)
                {
                    AiWaypoint connection = WaypointManager.Instance.GetWaypoint(c);
                    int cl = connection.Cluster;
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