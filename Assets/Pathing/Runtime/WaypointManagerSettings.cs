using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathing
{
    [System.Serializable]
    public struct WaypointManagerSettings
    {
        public bool UseRecommendedValues;
        [Range(100, 10000)] public int MaxWaypoints;
        [Min(1)] public float MaxConnectionRange;
        [Range(1, byte.MaxValue - 1)] public byte DesiredClusters;
        [Min(7)] public int MinimumClusterSize;
        [Range(1, 3)] public int FixClusterIterations;
        [Min(50)] public int RaycastHeight;
        public LayerMask RaycastMask;

        public WaypointManagerSettings(
            int maxWaypoints,
            int maxConnectionRange,
            byte desiredClusters,
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
            DesiredClusters = (byte)(Mathf.Max(1, MaxWaypoints / 200));
        }
    }
}