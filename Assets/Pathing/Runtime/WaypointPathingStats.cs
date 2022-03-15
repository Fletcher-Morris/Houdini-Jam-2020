using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pathing
{
    public struct WaypointPathingStats
    {
        public uint TotalPathsCalculated;
        public uint KnownPathsUsed;
        public uint SingleClusterSearches;
        public uint NeighbourClusterSearches;
        public uint CommonNeighbourClusterSearches;
        public uint MultiClusterSearches;
        public uint AllNodeSearches;
    }
}