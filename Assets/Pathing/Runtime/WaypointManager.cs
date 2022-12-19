using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Tick;
using Unity.Burst.CompilerServices;
using UnityEngine;
using static UnityEditor.PlayerSettings;

namespace Pathing
{
    [CreateAssetMenu(fileName = "Waypoint Manager", menuName = "Scriptables/Waypoints/Waypoint Manager")]
    public class WaypointManager : SerializedScriptableObject, IManualUpdate
    {
        [System.Serializable]
        private enum WaypointManagerState { Empty, PlacingNodes, Clustering, ClusterNormalise, Complete }
        [SerializeField, ReadOnly] WaypointManagerState _state;

        private static WaypointManager _instance;
        public static WaypointManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GameManager.Instance.WaypointManager;
                }
                return _instance;
            }
        }

        [SerializeField] private bool _deleteAll;

        [Space]
        public WaypointManagerSettings Settings = new WaypointManagerSettings(2000, 5, 10, 100, 0.1f);
        [SerializeField, HideInInspector] private WaypointManagerSettings _prevSettings;
        [Space]

        [OdinSerialize] private Dictionary<System.Tuple<int, int>, WaypointPath> _knownPaths = new Dictionary<System.Tuple<int, int>, WaypointPath>();
        [OdinSerialize] private List<WaypointPath> _knownPathsList = new List<WaypointPath>();
        [SerializeField] private bool _storeKnownPaths = true;
        [SerializeField, Range(1, 10)] private int _pathingAttempts = 5;
        [SerializeField, Range(0.0f, 1.0f)] private float _lineDebugOpacity = 0;
        [SerializeField, Range(0, 50)] private int _showCluster;
        [SerializeField] private bool _showClusters = true;
        [SerializeField] private bool _cullLines = true;
        public bool ShowNavigatorPaths = true;
        [SerializeField, ReadOnly] private int _averageClusterSize;
        [SerializeField, ReadOnly] private int _clusterSizeDiff;
        [SerializeField] private List<WaypointCluster> _clusters = new List<WaypointCluster>();
        [SerializeField] private List<AiWaypoint> _waypoints = new List<AiWaypoint>();

        [SerializeField] private WaypointPathingStats _pathingStats = new WaypointPathingStats();

        public WaypointCluster GetCluster(int id)
        {
            if (_clusters.Count == 0) return null;

            if (id > _clusters.Count || id < 0)
            {
                return null;
            }
            return _clusters[id];
        }
        public int WaypointCount => _waypoints.Count;

        public List<WaypointCluster> Clusters { get => _clusters; set => _clusters = value; }
        public List<AiWaypoint> Waypoints { get => _waypoints; set => _waypoints = value; }
        public float LineDebugOpacity { get => _lineDebugOpacity; set => _lineDebugOpacity = value; }
        public static bool IsInitialised
        {
            get
            {
                if (GameManager.Instance == null) return false;
                if (Instance == null) return false;
                return Instance._state == WaypointManagerState.Complete;
            }
        }

        public AiWaypoint GetWaypoint(int id)
        {
            if (id == int.MaxValue || id > _waypoints.Count)
            {
                return null;
            }
            return _waypoints[id];
        }

        public AiWaypoint GetWaypointInstance(int id)
        {
            if (id == int.MaxValue || id > _waypoints.Count)
            {
                return null;
            }
            return _waypoints[id];
        }

        public void RemoveWaypoint(AiWaypoint waypoint)
        {
            _waypoints.Remove(waypoint);
        }

        private void OnValidate()
        {
            if (_deleteAll) DeleteWaypoints();
            if (Settings.UseRecommendedValues) Settings.SetRecommendedValues();
        }

        public void GetKnownPaths()
        {
            _knownPaths = new Dictionary<System.Tuple<int, int>, WaypointPath>();
            _knownPathsList.ForEach(p =>
            {
                _knownPaths.Add(new System.Tuple<int, int>(p.Start, p.End), p);
            });
        }

        public void DrawLines(Camera _cullCam)
        {
            if (_lineDebugOpacity > 0.0f)
            {
                float camToPlanetDist = 0.0f;
                if (_cullLines && _cullCam != null)
                    camToPlanetDist = Vector3.Distance(_cullCam.transform.position, Vector3.zero);
                _waypoints.ForEach(w1 =>
                {
                    w1.Connections.ForEach(w2 =>
                    {
                        if (w1.Id > w2)
                        {
                            AiWaypoint conWp = GetWaypointInstance(w2);
                            bool cullLine = false;
                            if (_cullCam == null || _cullLines == false)
                            {
                            }
                            else
                            {
                                float dist = Mathf.Min(Vector3.Distance(_cullCam.transform.position, w1.Position),
                                    Vector3.Distance(_cullCam.transform.position, conWp.Position));
                                cullLine = dist > camToPlanetDist;
                            }

                            int showCluster = _showCluster;
                            if (_showCluster <= 0) showCluster = -1;
                            if ((showCluster == -1 || w1.Cluster == showCluster || conWp.Cluster == showCluster) || !_showClusters)
                            {
                                if (cullLine == false)
                                {
                                    Color lineCol = Color.grey;
                                    if (_showClusters)
                                    {
                                        if (w1.Cluster == conWp.Cluster)
                                        {
                                            lineCol = conWp.Cluster.NumberToColor(_clusters.Count);
                                        }

                                        if (conWp.Cluster < 0) lineCol = Color.black;

                                        lineCol.a = _lineDebugOpacity;

                                        bool coreCluster = false;
                                        WaypointCluster waypointCluster = GetCluster(w1.Cluster);
                                        if (waypointCluster != null)
                                        {
                                            if (waypointCluster.ClusterCore == w1.Id)
                                            {
                                                coreCluster = true;
                                            }
                                        }
                                        waypointCluster = GetCluster(conWp.Cluster);
                                        if (waypointCluster != null)
                                        {
                                            if (waypointCluster.ClusterCore == conWp.Id)
                                            {
                                                coreCluster = true;
                                            }
                                        }
                                        if (coreCluster) lineCol.a = 1;

                                    }
                                    else
                                    {
                                        lineCol.a = _lineDebugOpacity;
                                    }
                                    Debug.DrawLine(w1.Position, conWp.Position, lineCol);
                                }
                            }
                        }
                    });
                });
            }
        }

        public void DebugGui()
        {
            GUILayout.BeginArea(new Rect(5.0f, 55.0f, 100.0f, 50.0f));
            GUILayout.Label("Debug Opacity");
            _lineDebugOpacity = GUILayout.HorizontalSlider(_lineDebugOpacity, 0.0f, 1.0f);
            GUILayout.EndArea();
        }

        private void DeleteWaypoints()
        {
            _deleteAll = false;
            _waypoints = new List<AiWaypoint>();
            _clusters = new List<WaypointCluster>();
            _knownPaths = new Dictionary<System.Tuple<int, int>, WaypointPath>();
            _knownPathsList = new List<WaypointPath>();
            _pathingStats = new WaypointPathingStats();
            _state = WaypointManagerState.Empty;
        }

        [Button]
        private void Initialise()
        {
            _prevSettings = Settings;

            DeleteWaypoints();
            PlaceWaypoints();
            UpdateWaypointConnections();
            GenerateClusters();

            _state = WaypointManagerState.Clustering;
        }

        private void PlaceWaypoints()
        {
            _state = WaypointManagerState.PlacingNodes;

            List<Vector3> points = Extensions.FibonacciPoints(Settings.MaxWaypoints);

            List<WaypointModifierVolume> modVolumes = GameObject.FindObjectsOfType<WaypointModifierVolume>().ToList();

            modVolumes.FindAll(v => v.isActiveAndEnabled && v.ModifierType == WaypointModifierVolume.Modifier.Add).ForEach(v =>
            {
                if (v.PreRaycast)
                {
                    foreach (Vector3 p in v.Points)
                    {
                        AiWaypoint wp = new AiWaypoint(p);
                        wp.Id = _waypoints.Count;
                        _waypoints.Add(wp);
                    }
                }
                else
                {
                    points.AddRange(v.Points);
                }
            });

            foreach (var (pos, ray) in from Vector3 p in points
                                       let pos = p.normalized * Settings.RaycastHeight
                                       let ray = new Ray(pos, -pos)
                                       select (pos, ray))
            {
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, Settings.RaycastMask))
                {
                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                    {
                        Vector3 wpPosition = hit.point + (pos.normalized * Settings.WaypointHeightOffset);

                        bool badPos = false;
                        modVolumes.FindAll(v => v.isActiveAndEnabled && v.ModifierType == WaypointModifierVolume.Modifier.Remove).ForEach(v =>
                        {
                            if (v.PointIsInVolume(wpPosition)) badPos = true;
                        });

                        if (!badPos)
                        {
                            AiWaypoint wp = new AiWaypoint(wpPosition);
                            wp.Id = _waypoints.Count;
                            _waypoints.Add(wp);
                        }
                    }
                }
            }

            Debug.Log($"Created {_waypoints.Count} new waypoints!");
        }


        public void UpdateWaypointConnections()
        {
            int seed = Random.Range(int.MinValue, 0);
            _waypoints.ForEach(w =>
            {
                w.RandomiseConnections(seed);
                seed++;
            });

                _waypoints.ForEach(w1 =>
            {
                _waypoints.ForEach(w2 =>
                {
                    if (w1 != w2)
                    {
                        if (!w1.Connections.Contains(w2.Id))
                        {
                            Vector3 p1 = w1.Position;
                            Vector3 p2 = w2.Position;

                            float dist = Vector3.Distance(p1, p2);
                            if (dist <= Settings.MaxConnectionRange)
                            {
                                RaycastHit hit;
                                Physics.Raycast(p1, p2 - p1, out hit);

                                if (hit.collider == null)
                                {
                                    w1.Connections.Add(w2.Id);
                                    w2.Connections.Add(w1.Id);
                                }
                            }
                        }
                    }
                });
            });
        }

        private void GenerateClusters()
        {
            List<Vector3> points = points = Extensions.FibonacciPoints(Settings.DesiredClusters);
            _clusters = new List<WaypointCluster>();

            for (int point = 0; point < points.Count; point++)
            {
                Vector3 origin = (points[point].normalized) * Settings.RaycastHeight;
                WaypointCluster cluster = new WaypointCluster(_clusters.Count);

                AiWaypoint closest = Closest(origin);

                bool claimed = false;
                foreach (WaypointCluster c in _clusters)
                {
                    if (c.Id != cluster.Id)
                    {
                        if (c.HasClaimedWaypoint(closest.Id))
                        {
                            claimed = true;
                        }
                    }
                }

                if (claimed == false)
                {
                    _clusters.Add(cluster);
                    cluster.ClaimWaypoint(closest.Id);
                    _clusteriseWaypointQueue.Enqueue(closest.Id);
                }
            }

            _clusterSteps = 0;
            Debug.Log($"Generated {_clusters.Count} Clusters!");
        }

        private bool ResolveClusterConflict(WaypointCluster a, WaypointCluster b)
        {
            //  Returns true if A wins the claim.

            int aWaypoints = a.Waypoints.Count;
            aWaypoints += a.ClaimedWaypoints.Count;

            int bWaypoints = b.Waypoints.Count;
            bWaypoints += b.ClaimedWaypoints.Count;

            if (aWaypoints == bWaypoints)
            {
                return a.Id < b.Id;
            }
            else
            {
                return aWaypoints < bWaypoints;
            }
        }

        private Queue<int> _clusteriseWaypointQueue = new Queue<int>();
        [SerializeField, Min(1)] private int _maxPerStep = 10;
        private bool _stepA = false;
        private int _clusterSteps = 0;

        private void Clusterise()
        {
            _clusterSteps++;

            if (_stepA) ClusteriseStepA();
            else ClusteriseStepB();
        }

        private void ClusteriseStepA()
        {
            for (int i = 0; i < Mathf.Min(_maxPerStep, _clusteriseWaypointQueue.Count + 1); i++)
            {
                AiWaypoint wp = GetWaypoint(_clusteriseWaypointQueue.Dequeue());
                foreach (int connectedId in wp.Connections)
                {
                    AiWaypoint connectedWp = GetWaypoint(connectedId);
                    if (connectedWp.Cluster == wp.Cluster)
                    {
                    }
                    else
                    {
                        if (connectedWp.Cluster == -1)
                        {
                            GetCluster(wp.Cluster).ClaimWaypoint(connectedId);
                        }
                    }
                }
            }

            _stepA= false;
        }

        private int _solveClaimTries = 0;
        private void ClusteriseStepB()
        {
            foreach (WaypointCluster cluster in _clusters)
            {
                while (cluster.ClaimedWaypoints.Count > 0 && _solveClaimTries <= 100)
                {
                    if (_solveClaimTries >= 100)
                    {
                        Debug.LogWarning("Claim Tries Exceeding 100!");
                    }
                    _solveClaimTries++;

                    int claimId = cluster.ClaimedWaypoints[0];
                    List<WaypointCluster> competingClaims = _clusters.FindAll(c => c.Id != cluster.Id && c.HasClaimedWaypoint(claimId));

                    if (competingClaims.Count == 0)
                    {
                        cluster.UnclaimWaypoint(claimId);
                        GetWaypoint(claimId).Cluster = cluster.Id;
                        cluster.Waypoints.Add(claimId);
                        _clusteriseWaypointQueue.Enqueue(claimId);
                    }
                    else
                    {
                        if (ResolveClusterConflict(competingClaims[0], cluster))
                        {
                            cluster.UnclaimWaypoint(claimId);
                        }
                        else
                        {
                            competingClaims[0].UnclaimWaypoint(claimId);
                        }
                    }
                }

                _solveClaimTries = 0;
            }

            _stepA = true;

            if (_clusteriseWaypointQueue.Count == 0)
            {
                List<AiWaypoint> unClustered = _waypoints.FindAll(w => w.Cluster == -1);
                if (unClustered.Count > 0)
                {
                    WaypointCluster newCluster = new WaypointCluster(_clusters.Count);
                    newCluster.ClaimWaypoint(unClustered[0].Id);
                    _clusters.Add(newCluster);
                    _clusteriseWaypointQueue.Enqueue(unClustered[0].Id);
                }
                else
                {
                    Debug.Log($"Clusterised waypoints in {_clusterSteps} steps!");
                    _clusterNormaliseStep = 0;
                    _state = WaypointManagerState.ClusterNormalise;
                }
            }
        }

        private int _clusterNormaliseStep = 0;
        private void ClusterNormalise()
        {
            if(_clusterNormaliseStep >= _clusters.Count)
            {
                _averageClusterSize = WaypointCount / Clusters.Count;
                int smallest = int.MaxValue;
                int biggest = int.MinValue;
                foreach (WaypointCluster c in Clusters)
                {
                    int s = c.Waypoints.Count;
                    smallest = Mathf.Min(s, smallest);
                    biggest = Mathf.Max(s, biggest);
                    _clusterSizeDiff = biggest- smallest;
                }

                _state = WaypointManagerState.Complete;
            }
            else
            {
                _clusters[_clusterNormaliseStep].FindNewCore();
                _clusters[_clusterNormaliseStep].FindConnectedClusters();
                _clusterNormaliseStep++;
            }
        }

        private AiWaypoint _closestNode = null;
        public AiWaypoint Closest(Vector3 pos)
        {
            if (Instance == null) return null;
            if (_waypoints == null) return null;
            if (_waypoints.Count == 0) return null;

            _closestNode = null;
            float dist = Mathf.Infinity;

            foreach (AiWaypoint w in _waypoints)
            {
                float d  = (pos - w.Position).sqrMagnitude;
                if (d < dist)
                {
                    _closestNode = w;
                    dist = d;
                }
            }

            if (_closestNode == null) Debug.LogWarning("Cannot Find Closest Waypoint To Target!");
            return _closestNode;
        }

        public List<int> GetPath(Vector3 start, Vector3 end)
        {
            AiWaypoint startNode = Closest(start);
            AiWaypoint endNode = Closest(end);

            if (startNode == null)
            {
                Debug.LogWarning($"Could not find stating node close to '{start}'!");
                return null;
            }

            if (endNode == null)
            {
                Debug.LogWarning($"Could not find ending node close to '{end}'!");
                return null;
            }

            return GetBakedPath(startNode, endNode);
        }

        public List<int> GetBakedPath(AiWaypoint start, AiWaypoint end)
        {
            WaypointPath foundBakedPath = new WaypointPath(start.Id, end.Id, new List<int>());

            if (_storeKnownPaths)
            {
                if (_knownPaths.TryGetValue(new System.Tuple<int, int>(start.Id, end.Id), out foundBakedPath))
                {
                    _pathingStats.KnownPathsUsed++;
                    return foundBakedPath.Path;
                }

                if (_knownPaths.TryGetValue(new System.Tuple<int, int>(end.Id, start.Id), out foundBakedPath))
                {
                    _pathingStats.KnownPathsUsed++;
                    return foundBakedPath.Path.Reversed();
                }
            }

            //  Path is not yet baked.

            WaypointPath chosenPath = null;
            List<int> chosenPathList = null;
            float shortestPathDistance = Mathf.Infinity;
            float longestPathDistance = Mathf.NegativeInfinity;

            for (int attempt = 0; attempt < _pathingAttempts; attempt++)
            {
                WaypointPath forwardWaypointPath = new WaypointPath(start.Id, end.Id, new List<int>());
                List<int> forwardPath = Breadthwise(start, end, attempt);
                if (forwardPath != null)
                {
                    forwardWaypointPath.Path = forwardPath;
                    _pathingStats.TotalPathsCalculated++;

                    float forwardPathDist = forwardWaypointPath.Distance();
                    if (forwardPathDist > longestPathDistance)
                    {
                        longestPathDistance = forwardPathDist;
                    }
                    if (forwardPathDist < shortestPathDistance)
                    {
                        shortestPathDistance = forwardPathDist;
                        chosenPath = forwardWaypointPath;
                        chosenPathList = forwardPath;
                    }
                }
            }

            if (_storeKnownPaths)
            {
                _knownPaths.Add(new System.Tuple<int, int>(start.Id, end.Id), chosenPath);
                _knownPathsList.Add(chosenPath);
            }
            return chosenPathList;
        }

        private List<int> Breadthwise(AiWaypoint start, AiWaypoint end, int attempt)
        {
            if(_clusters.Count == 0)
            {
                Debug.LogWarning("No Clusters!");
                return null;
            }

            List<int> result = null;

            if (start.Cluster == end.Cluster)
            {
                result = Breadthwise(start, end, start.Cluster, attempt);
                if (result != null)
                {
                    _pathingStats.SingleClusterSearches++;
                    return result;
                }
                else
                {
                    Debug.LogWarning($"Nodes '{start.Id}' and '{end.Id}' are both in cluster '{start.Cluster}', but a path could not be found within that cluster!");
                }
            }

            WaypointCluster startCluster = GetCluster(start.Cluster);
            WaypointCluster endCluster = GetCluster(end.Cluster);


            if (startCluster.ConnectedClusters.Contains(end.Cluster))
            {
                List<int> dualClusterIds = new List<int>();
                dualClusterIds.Add(start.Cluster);
                dualClusterIds.Add(end.Cluster);
                result = Breadthwise(start, end, dualClusterIds, attempt);
                if (result != null)
                {
                    _pathingStats.NeighbourClusterSearches++;
                    return result;
                }
                else
                {
                    Debug.LogWarning($"Nodes '{start.Id}' and '{end.Id}' are in connected clusters '{start.Cluster}' and '{end.Cluster}', but a path could not be found within those clusters!");
                }
            }

            List<int> commonClusterIds = startCluster.ConnectedClusters.Intersect(endCluster.ConnectedClusters).ToList();
            if (commonClusterIds.Count >= 1)
            {
                commonClusterIds.Add(start.Cluster);
                commonClusterIds.Add(end.Cluster);
                result = Breadthwise(start, end, commonClusterIds, attempt);
                if (result != null)
                {
                    _pathingStats.CommonNeighbourClusterSearches++;
                    return result;
                }
            }

            List<int> clusterBreathwiseSearch = ClusterBreadthwise(startCluster, endCluster, attempt);
            if (clusterBreathwiseSearch != null)
            {
                if (clusterBreathwiseSearch.Count >= 1)
                {
                    result = Breadthwise(start, end, clusterBreathwiseSearch, attempt);
                    if (result != null)
                    {
                        _pathingStats.MultiClusterSearches++;
                        return result;
                    }
                }
            }

            result = Breadthwise(start, end, null, attempt);
            _pathingStats.AllNodeSearches++;
            return result;
        }

        public List<int> Breadthwise(AiWaypoint start, AiWaypoint end, int clusterId, int attempt)
        {
            if (GetCluster(clusterId) != null)
            {
                List<int> list = new List<int>();
                list.Add(clusterId);
                return Breadthwise(start, end, list, attempt);
            }
            else
            {
                return null;
            }
        }

        public List<int> Breadthwise(AiWaypoint start, AiWaypoint end, List<int> validClusters, int attempt)
        {
            List<int> result = new List<int>();
            List<int> visited = new List<int>();
            Queue<int> work = new Queue<int>();

            start.History = new List<int>();
            visited.Add(start.Id);
            work.Enqueue(start.Id);
            int tries = 0;

            while (work.Count > 0 && tries < _waypoints.Count)
            {
                tries++;
                int current = work.Dequeue();
                AiWaypoint currentWp = GetWaypoint(current);
                if (current == end.Id)
                {
                    //Found Node
                    result = GetWaypoint(current).History;
                    result.Add(current);
                    return result;
                }

                //Didn't find Node
                for (int i = 0; i < (currentWp.Connections.Count); i++)
                {
                    int j = (i + attempt) % currentWp.Connections.Count;
                    int currentNeighbour = currentWp.Connections[j];
                    AiWaypoint currentNeighbourWp = GetWaypoint(currentNeighbour);
                    if (validClusters == null || validClusters.Count == 0 || validClusters.Contains(currentNeighbourWp.Cluster))
                    {
                        if (!visited.Contains(currentNeighbour))
                        {
                            currentNeighbourWp.History = new List<int>(currentWp.History);
                            currentNeighbourWp.History.Add(current);
                            visited.Add(currentNeighbour);
                            work.Enqueue(currentNeighbour);
                        }
                    }
                }
            }

            //Route not found, loop ends
            Debug.LogWarning($"Could not find path between '{start.Id}' and '{end.Id}' within waypoint search list!");

            return null;
        }

        private List<int> ClusterBreadthwise(WaypointCluster start, WaypointCluster end, int attempt)
        {
            List<int> result = new List<int>();
            List<int> visited = new List<int>();
            Queue<int> work = new Queue<int>();

            start.History = new List<int>();
            visited.Add(start.Id);
            work.Enqueue(start.Id);
            int tries = 0;

            while (work.Count > 0 && tries < _waypoints.Count)
            {
                tries++;
                int current = work.Dequeue();
                WaypointCluster currentCluster = GetCluster(current);
                if (current == end.Id)
                {
                    //Found Cluster
                    result = GetCluster(current).History;
                    result.Add(current);
                    return result;
                }

                //Didn't find Cluster
                for (int i = 0; i < (currentCluster.ConnectedClusters.Count); i++)
                {
                    int currentNeighbour = currentCluster.ConnectedClusters[i];
                    WaypointCluster currentNeighbourCl = GetCluster(currentNeighbour);
                    if (!visited.Contains(currentNeighbour))
                    {
                        currentNeighbourCl.History = new List<int>(currentCluster.History);
                        currentNeighbourCl.History.Add(current);
                        visited.Add(currentNeighbour);
                        work.Enqueue(currentNeighbour);
                    }
                }
            }

            return null;
        }

        UpdateManager IManualUpdate.GetUpdateManager()
        {
            return GameManager.Instance.UpdateManager;
        }

        bool IManualUpdate.OnInitialise()
        {
            _instance = this;

            switch (_state)
            {
                case WaypointManagerState.Empty:
                    Initialise();
                    DrawLines(GameManager.Instance.CullingCam);
                    break;

                case WaypointManagerState.PlacingNodes:
                    break;

                case WaypointManagerState.Clustering:
                    Clusterise();
                    break;

                case WaypointManagerState.ClusterNormalise:
                    ClusterNormalise();
                    break;

                case WaypointManagerState.Complete:
                    return true;
            }

            DrawLines(GameManager.Instance.CullingCam);

            return false;
        }

        void IManualUpdate.OnManualUpdate(float delta)
        {
            DrawLines(GameManager.Instance.CullingCam);
        }

        void IManualUpdate.OnTick(float delta)
        {
        }

        void IManualUpdate.OnManualFixedUpdate(float delta)
        {
        }

        bool IManualUpdate.IsEnabled()
        {
            return true;
        }

        void IManualUpdate.OnApplicationQuit()
        {
            switch (_state)
            {
                case WaypointManagerState.Complete:
                    break;
                default:
                    _state = WaypointManagerState.Empty;
                    break;
            }
        }
    }
}