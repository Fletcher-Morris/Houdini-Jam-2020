using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace Pathing
{
    [CreateAssetMenu(fileName = "Waypoint Manager", menuName = "Scriptables/Waypoints/Waypoint Manager")]
    public class WaypointManager : SerializedScriptableObject
    {
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

        private bool _initialised = false;
        [SerializeField] private bool _deleteAll;
        public bool RefreshOnChange;

        [Space]
        public WaypointManagerSettings Settings = new WaypointManagerSettings(2000, 5, 10, 100);
        [SerializeField, HideInInspector] private WaypointManagerSettings _prevSettings;
        [Space]

        [OdinSerialize] private Dictionary<System.Tuple<ushort, ushort>, WaypointPath> _knownPaths = new Dictionary<System.Tuple<ushort, ushort>, WaypointPath>();
        [OdinSerialize] private List<WaypointPath> _knownPathsList = new List<WaypointPath>();
        [SerializeField] private bool _storeKnownPaths = true;
        [SerializeField] private int _knownPathsUsed;
        [SerializeField] private int _newPathsCalculated;
        [SerializeField] private int _clusterSavings;
        [SerializeField] private int _connectedClusterSavings;
        [SerializeField] private int _allNodeSearches;
        [SerializeField, Range(0.0f, 1.0f)] private float _lineDebugOpacity = 0;
        [SerializeField, Range(0, 50)] private int _showCluster;
        [SerializeField] private bool _showClusters = true;
        [SerializeField] private bool m_cullLines = true;
        [SerializeField] private bool _waitTime;
        [SerializeField] private List<WaypointCluster> _clusters = new List<WaypointCluster>();
        [SerializeField] private List<AiWaypoint> _waypoints = new List<AiWaypoint>();
        public WaypointCluster GetCluster(ushort id)
        {
            if (id == ushort.MaxValue || id > _clusters.Count)
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
                if(GameManager.Instance == null) return false;
                if(Instance == null) return false;
                return Instance._initialised;
            }
        }

        public AiWaypoint GetWaypoint(ushort id)
        {
            if (id == ushort.MaxValue || id > _waypoints.Count)
            {
                return null;
            }
            return _waypoints[id];
        }

        public AiWaypoint GetWaypointInstance(ushort id)
        {
            if (id == ushort.MaxValue || id > _waypoints.Count)
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
            if(Settings.Equals(_prevSettings) == false && RefreshOnChange) Initialise();
        }

        public void GetKnownPaths()
        {
            _knownPaths = new Dictionary<System.Tuple<ushort, ushort>, WaypointPath>();
            _knownPathsList.ForEach(p =>
            {
                _knownPaths.Add(new System.Tuple<ushort, ushort>(p.Start, p.End), p);
            });
        }

        public void DrawLines(Camera _cullCam)
        {
            if (_lineDebugOpacity > 0.0f)
            {
                float camToPlanetDist = 0.0f;
                if (m_cullLines && _cullCam != null)
                    camToPlanetDist = Vector3.Distance(_cullCam.transform.position, Vector3.zero);
                _waypoints.ForEach(w1 =>
                {
                    w1.Connections.ForEach(w2 =>
                    {
                        if (w1.Id > w2)
                        {
                            AiWaypoint conWp = GetWaypointInstance(w2);
                            bool cullLine = false;
                            if (_cullCam == null || m_cullLines == false)
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
                                            lineCol = ((int)conWp.Cluster).NumberToColor(_clusters.Count);
                                        }

                                        if (conWp.Cluster == ushort.MaxValue) lineCol = Color.black;

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
            GUILayout.BeginArea(new Rect(5.0f, 5.0f, 100.0f, 50.0f));
            GUILayout.Label("Debug Opacity");
            _lineDebugOpacity = GUILayout.HorizontalSlider(_lineDebugOpacity, 0.0f, 1.0f);
            GUILayout.EndArea();
        }

        private void DeleteWaypoints()
        {
            _deleteAll = false;
            _waypoints = new List<AiWaypoint>();
            _clusters = new List<WaypointCluster>();
            _knownPaths = new Dictionary<System.Tuple<ushort, ushort>, WaypointPath>();
            _knownPathsList = new List<WaypointPath>();
            _initialised = false;
            _knownPathsUsed = 0;
            _newPathsCalculated = 0;
            _clusterSavings = 0;
            _connectedClusterSavings = 0;
            _allNodeSearches = 0;
        }

        public void Start()
        {
            GetKnownPaths();
            if (_waypoints.Count == 0) Initialise();
        }

        public void Initialise()
        {
            _instance = this;

            _initialised = false;

            _prevSettings = Settings;

            DeleteWaypoints();

            List<Vector3> points = Extensions.FibonacciPoints(Settings.MaxWaypoints);

            foreach (Vector3 p in points)
            {
                Vector3 pos = p.normalized * Settings.RaycastHeight;

                Ray ray = new Ray(pos, -pos);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, Settings.RaycastMask))
                {
                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                    {
                        _waypoints.Add(new AiWaypoint(hit.point + pos.normalized));
                    }
                }
            }

            points = Extensions.FibonacciPoints(Settings.DesiredClusters);
            _clusters = new List<WaypointCluster>();
            for (int i = 0; i < Settings.DesiredClusters; i++)
            {
                _clusters.Add(new WaypointCluster(i));
            }

            ushort id = 0;
            _waypoints.ForEach(w =>
            {
                w.Id = id;
                id++;
                w.Cluster = (ushort)points.ClosestPoint(w.Position);
                _clusters[w.Cluster].Waypoints.Add(w.Id);
            });

            foreach (WaypointCluster cluster in _clusters)
            {
                cluster.FindNewCore();
            }

            Debug.Log($"Created {_waypoints.Count} waypoints!");

            UpdateConnections();
        }

        public void UpdateConnections()
        {
            foreach (AiWaypoint w1 in _waypoints)
            {
                foreach (AiWaypoint w2 in _waypoints)
                {
                    if (w1 != w2)
                    {
                        if (w1.Connections.Contains(w2.Id))
                        {
                        }
                        else
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
                }
            }

            FixClusters(Settings.FixClusterIterations, true);
        }

        public void FixClusters(int passes, bool first)
        {
            passes--;

            if (!first)
            {
                foreach (AiWaypoint wp in _waypoints)
                {
                    float closestDist = Mathf.Infinity;
                    WaypointCluster closestCluster = null;
                    foreach (WaypointCluster cluster in _clusters)
                    {
                        AiWaypoint core = GetWaypoint(cluster.ClusterCore);
                        float dist = Vector3.Distance(wp.Position, core.Position);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            closestCluster = cluster;
                        }
                    }
                    wp.Cluster = closestCluster.Id;
                }
            }

            foreach (WaypointCluster cluster in _clusters)
            {
                if (cluster.Waypoints.Count == 0) continue;

                foreach (ushort w in cluster.Waypoints)
                {
                    AiWaypoint start = GetWaypoint(w);
                    AiWaypoint end = GetWaypoint(cluster.ClusterCore);
                    if (start != end)
                    {
                        List<ushort> path = Breadthwise(start, end, cluster.Id);
                        if (path == null)
                        {
                            start.Cluster = ushort.MaxValue;
                        }
                    }
                }
            }

            foreach (WaypointCluster cluster in _clusters)
            {
                cluster.Waypoints = new List<ushort>();
                foreach (AiWaypoint wp in _waypoints)
                {
                    if (wp.Cluster == cluster.Id)
                    {
                        cluster.Waypoints.Add(wp.Id);
                    }
                }
                if (cluster.Waypoints.Count < Settings.MinimumClusterSize)
                {
                    foreach (ushort wp in cluster.Waypoints)
                    {
                        GetWaypoint(wp).Cluster = ushort.MaxValue;
                    }
                    cluster.Id = ushort.MaxValue;
                }
            }
            _clusters.RemoveAll(c => c.Id == ushort.MaxValue);


            int maxTries = _waypoints.FindAll(wp => wp.Cluster == ushort.MaxValue).Count();
            int tries = 0;
            int fixedCount = 0;

            while (tries <= maxTries && fixedCount <= maxTries)
            {
                foreach (AiWaypoint wp in _waypoints)
                {
                    if (wp.Cluster != ushort.MaxValue)
                    {
                        foreach (ushort c in wp.Connections)
                        {
                            AiWaypoint cwp = GetWaypoint(c);
                            if (cwp.Cluster == ushort.MaxValue)
                            {
                                cwp.Cluster = wp.Cluster;
                                fixedCount++;
                            }
                        }
                    }
                }
                tries++;
            }

            List<AiWaypoint> nonClusteredWaypoints = _waypoints.FindAll(wp => wp.Cluster == ushort.MaxValue);
            maxTries = nonClusteredWaypoints.Count();
            tries = 0;

            while (tries <= maxTries)
            {
                nonClusteredWaypoints = _waypoints.FindAll(wp => wp.Cluster == ushort.MaxValue);
                if (nonClusteredWaypoints.Count >= 1)
                {
                    int tries2 = nonClusteredWaypoints.Count();
                    ushort newCluster = (ushort)_clusters.Count;
                    _clusters.Add(new WaypointCluster(newCluster));
                    _clusters[newCluster].ClusterCore = nonClusteredWaypoints[0].Id;
                    nonClusteredWaypoints[0].Cluster = newCluster;
                    while (tries2 <= maxTries)
                    {
                        foreach (AiWaypoint wp in nonClusteredWaypoints)
                        {
                            if (wp.Cluster != ushort.MaxValue)
                            {
                                foreach (ushort c in wp.Connections)
                                {
                                    AiWaypoint cwp = GetWaypoint(c);
                                    if (cwp.Cluster == ushort.MaxValue)
                                    {
                                        cwp.Cluster = wp.Cluster;
                                        fixedCount++;
                                    }
                                }
                            }
                        }
                        tries2++;
                    }
                }
                tries++;
            }

            foreach (WaypointCluster cluster in _clusters)
            {
                cluster.FindConnectedClusters();
            }

            foreach (AiWaypoint wp in _waypoints)
            {
                wp.ClusterConnections = new List<ushort>();
            }

            foreach (AiWaypoint w1 in _waypoints)
            {
                foreach (ushort w2id in w1.Connections)
                {
                    AiWaypoint w2 = GetWaypoint(w2id);
                    if (w1.Cluster == w2.Cluster)
                    {
                        w1.ClusterConnections.Add(w2.Id);
                        w2.ClusterConnections.Add(w1.Id);
                    }
                }
            }

            foreach (WaypointCluster cluster in _clusters)
            {
                cluster.FindNewCore();
            }

            if (passes > 0)
            {
                FixClusters(passes, false);
            }
            else
            {
                _initialised = true;
            }
        }

        public AiWaypoint Closest(Vector3 pos)
        {
            if (Instance == null) return null;
            if (_waypoints == null) return null;
            if (_waypoints.Count == 0) return null;
            AiWaypoint node = null;
            var dist = Mathf.Infinity;
            _waypoints.ForEach(w =>
            {
                var d = Vector3.Distance(pos, w.Position);
                if (d < dist)
                {
                    node = w;
                    dist = d;
                }
            });
            if (node == null) Debug.LogWarning("Cannot Find Closest Waypoint To Target!");
            return node;
        }

        public List<ushort> GetPath(Vector3 start, Vector3 end)
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

        public List<ushort> GetBakedPath(AiWaypoint start, AiWaypoint end)
        {
            WaypointPath foundBakedPath = new WaypointPath(start.Id, end.Id, new List<ushort>());

            if (_storeKnownPaths)
            {
                if (_knownPaths.TryGetValue(new System.Tuple<ushort, ushort>(start.Id, end.Id), out foundBakedPath))
                {
                    _knownPathsUsed++;
                    return foundBakedPath.Path;
                }

                if (_knownPaths.TryGetValue(new System.Tuple<ushort, ushort>(end.Id, start.Id), out foundBakedPath))
                {
                    _knownPathsUsed++;
                    return foundBakedPath.Path.Reversed();
                }
            }

            //  Path is not yet baked.
            foundBakedPath = new WaypointPath(start.Id, end.Id, new List<ushort>());
            List<ushort> newPath = Breadthwise(start, end);
            if (newPath != null)
            {
                foundBakedPath.Path = newPath;
                _newPathsCalculated++;
                if (_storeKnownPaths)
                {
                    _knownPaths.Add(new System.Tuple<ushort, ushort>(start.Id, end.Id), foundBakedPath);
                    _knownPathsList.Add(foundBakedPath);
                }
            }

            return newPath;
        }

        public List<ushort> Breadthwise(AiWaypoint start, AiWaypoint end)
        {
            List<ushort> result = null;

            if (start.Cluster == end.Cluster)
            {
                result = Breadthwise(start, end, start.Cluster);
                if (result != null)
                {
                    _clusterSavings++;
                    return result;
                }
                else
                {
                    Debug.LogWarning($"Nodes '{start.Id}' and '{end.Id}' are both in cluster '{start.Cluster}', but a path could not be found within that cluster!");
                }
            }
            else if (GetCluster(start.Cluster).ConnectedClusters.Contains(end.Cluster))
            {
                List<ushort> clusterIds = new List<ushort>();
                clusterIds.Add(start.Cluster);
                clusterIds.Add(end.Cluster);
                result = Breadthwise(start, end, clusterIds);
                if (result != null)
                {
                    _connectedClusterSavings++;
                    return result;
                }
                else
                {
                    Debug.LogWarning($"Nodes '{start.Id}' and '{end.Id}' are in connected clusters '{start.Cluster}' and '{end.Cluster}', but a path could not be found within those clusters!");
                }
            }

            result = Breadthwise(start, end, null);
            _allNodeSearches++;
            return result;
        }

        public List<ushort> Breadthwise(AiWaypoint start, AiWaypoint end, ushort clusterId)
        {
            if (GetCluster(clusterId) != null)
            {
                List<ushort> list = new List<ushort>();
                list.Add(clusterId);
                return Breadthwise(start, end, list);
            }
            else
            {
                return null;
            }
        }

        public List<ushort> Breadthwise(AiWaypoint start, AiWaypoint end, List<ushort> validClusters)
        {
            List<ushort> result = new List<ushort>();
            List<ushort> visited = new List<ushort>();
            Queue<ushort> work = new Queue<ushort>();

            start.History = new List<ushort>();
            visited.Add(start.Id);
            work.Enqueue(start.Id);
            int tries = 0;

            while (work.Count > 0 && tries < _waypoints.Count)
            {
                tries++;
                ushort current = work.Dequeue();
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
                    ushort currentNeighbour = currentWp.Connections[i];
                    AiWaypoint currentNeighbourWp = GetWaypoint(currentNeighbour);
                    if (validClusters == null || validClusters.Count == 0 || validClusters.Contains(currentNeighbourWp.Cluster))
                    {
                        if (!visited.Contains(currentNeighbour))
                        {
                            currentNeighbourWp.History = new List<ushort>(currentWp.History);
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
    }
}