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
        [Min(50)] public int RaycastHeight;
        public LayerMask RaycastMask;
        [Range(0, 1)] public float WaypointHeightOffset;

        public WaypointManagerSettings(
            int maxWaypoints,
            int maxConnectionRange,
            byte desiredClusters,
            int raycastHeight,
            float waypointHeightOffset)
        {
            UseRecommendedValues = true;
            MaxWaypoints = maxWaypoints;
            MaxConnectionRange = maxConnectionRange;
            DesiredClusters = desiredClusters;
            RaycastHeight = raycastHeight;
            RaycastMask = new LayerMask();
            WaypointHeightOffset = waypointHeightOffset;
        }

        public void SetRecommendedValues()
        {
            DesiredClusters = (byte)(Mathf.Max(1, MaxWaypoints / 200));
        }
    }
}