using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathing
{
	[System.Serializable]
	public struct WaypointManagerSettings
	{
		public bool UseRecommendedValues;
		[Range(100, 10000)] public ushort MaxWaypoints;
		[Min(1)] public float MaxConnectionRange;
		[Range(1, ushort.MaxValue / 200)] public ushort DesiredClusters;
		[Min(7)] public int MinimumClusterSize;
		[Range(1, 3)] public int FixClusterIterations;
		[Min(50)] public int RaycastHeight;
		public LayerMask RaycastMask;

		public WaypointManagerSettings(
			ushort maxWaypoints,
			int maxConnectionRange,
			ushort desiredClusters,
			int raycastHeight)
		{
			UseRecommendedValues = true;
			MaxWaypoints = maxWaypoints;
			MaxConnectionRange = maxConnectionRange;
			DesiredClusters = desiredClusters;
			MinimumClusterSize = 7;
			FixClusterIterations = 2;
			RaycastHeight = raycastHeight;
			RaycastMask = new LayerMask();
		}

		public void SetRecommendedValues()
		{
			DesiredClusters = (ushort)(Mathf.Max(1, MaxWaypoints / 200));
		}
	}
}