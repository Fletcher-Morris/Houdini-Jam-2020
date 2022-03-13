using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathing
{
	[System.Serializable]
	public class WaypointCluster
	{
		public ushort Id;
		public List<ushort> Waypoints = new List<ushort>();
		public ushort ClusterCore;
		public List<ushort> ConnectedClusters = new List<ushort>();

		public WaypointCluster(int id)
		{
			Id = (ushort)id;
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
			ConnectedClusters = new List<ushort>();
			foreach (AiWaypoint wp in WaypointManager.Instance.Waypoints)
			{
				if (wp.Cluster == Id)
				{
					Waypoints.Add(wp.Id);
				}

				if (ConnectedClusters.Count < WaypointManager.Instance.Clusters.Count - 1)
				{
					foreach (ushort wpc in wp.Connections)
					{
						AiWaypoint wpcw = WaypointManager.Instance.GetWaypoint(wpc);
						if (Id != wpcw.Cluster)
						{
							if (!ConnectedClusters.Contains(wpcw.Cluster))
							{
								ConnectedClusters.Add(wpcw.Cluster);
							}
						}
					}
				}
			}
		}
	}
}