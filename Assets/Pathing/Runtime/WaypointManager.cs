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
        [SerializeField, Range(1, 10)] private int _pathingAttempts = 5;
        [SerializeField, Range(0.0f, 1.0f)] private float _lineDebugOpacity = 0;
        [SerializeField, Range(0, 50)] private byte _showCluster;
        [SerializeField] private bool _showClusters = true;
        [SerializeField] private bool _cullLines = true;
        public bool ShowNavigatorPaths = true;
        [SerializeField] private List<WaypointCluster> _clusters = new List<WaypointCluster>();
        [SerializeField] private List<AiWaypoint> _waypoints = new List<AiWaypoint>();

        [SerializeField] private WaypointPathingStats _pathingStats = new WaypointPathingStats();

        public WaypointCluster GetCluster(byte id)
        {
            if (id == byte.MaxValue || id > _clusters.Count)
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
            if (Settings.Equals(_prevSettings) == false && RefreshOnChange) Initialise();
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
                                            lineCol = ((int)conWp.Cluster).NumberToColor(_clusters.Count);
                                        }

                                        if (conWp.Cluster == byte.MaxValue) lineCol = Color.black;

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
            _knownPaths = new Dictionary<System.Tuple<ushort, ushort>, WaypointPath>();
            _knownPathsList = new List<WaypointPath>();
            _initialised = false;
            _pathingStats = new WaypointPathingStats();
        }

        public void Start()
        {
            GetKnownPaths();
            if (_waypoints.Count == 0) Initialise();
            _initialised = true;
        }

        [Button]
        public void Initialise()
        {
            _instance = this;

            _initialised = false;

            _prevSettings = Settings;

            DeleteWaypoints();

            List<Vector3> points = Extensions.FibonacciPoints(Settings.MaxWaypoints);
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
                w.Cluster = (byte)points.ClosestPoint(w.Position);
                _clusters[w.Cluster].Waypoints.Add(w.Id);
            });

            _clusters.ForEach(cluster =>
            {
                cluster.FindNewCore();
            });

            Debug.Log($"Created {_waypoints.Count} waypoints!");

            UpdateConnections();
        }

        public void UpdateConnections()
        {
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

            FixClusters(Settings.FixClusterIterations, true);
        }

        public void FixClusters(int passes, bool first)
        {
            passes = passes - 1;

            _waypoints.ForEach(wp =>
            {
                float closestDist = Mathf.Infinity;
                WaypointCluster closestCluster = null;
                _clusters.ForEach(cluster =>
                {
                    AiWaypoint core = GetWaypoint(cluster.ClusterCore);
                    float dist = Vector2.Distance(core.Position, wp.Position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestCluster = cluster;
                    }
                });
                wp.Cluster = closestCluster.Id;
                if (!closestCluster.Waypoints.Contains(wp.Id)) closestCluster.Waypoints.Add(wp.Id);
            });

            _clusters.ForEach(cluster =>
            {
                if (cluster.Waypoints.Count != 0)
                {
                    cluster.Waypoints.ForEach(w =>
                    {
                        AiWaypoint start = GetWaypoint(w);
                        AiWaypoint end = GetWaypoint(cluster.ClusterCore);
                        if (start != end)
                        {
                            List<ushort> path = Breadthwise(start, end, cluster.Id, 0);
                            if (path == null || path.Count == 0)
                            {
                                start.Cluster = byte.MaxValue;
                            }
                        }
                    });
                }
            });

            _clusters.ForEach(cluster =>
            {
                cluster.Waypoints = new List<ushort>();
                _waypoints.ForEach(wp =>
                {
                    if (wp.Cluster == cluster.Id)
                    {
                        cluster.Waypoints.Add(wp.Id);
                    }
                });
                if (cluster.Waypoints.Count < Settings.MinimumClusterSize)
                {
                    cluster.Waypoints.ForEach(wp =>
                    {
                        GetWaypoint(wp).Cluster = byte.MaxValue;
                    });
                    cluster.Id = byte.MaxValue;
                }
            });
            _clusters.RemoveAll(c => c.Id == byte.MaxValue);


            int maxTries = _waypoints.FindAll(wp => wp.Cluster == byte.MaxValue).Count();
            int tries = 0;
            int fixedCount = 0;

            while (tries <= maxTries && fixedCount <= maxTries)
            {
                _waypoints.ForEach(wp =>
                {
                    if (wp.Cluster != byte.MaxValue)
                    {
                        wp.Connections.ForEach(c =>
                        {
                            AiWaypoint cwp = GetWaypoint(c);
                            if (cwp.Cluster == byte.MaxValue)
                            {
                                cwp.Cluster = wp.Cluster;
                                fixedCount++;
                            }
                        });
                    }
                });
                tries++;
            }

            List<AiWaypoint> nonClusteredWaypoints = _waypoints.FindAll(wp => wp.Cluster == byte.MaxValue);
            maxTries = nonClusteredWaypoints.Count();
            tries = 0;

            while (tries <= maxTries)
            {
                nonClusteredWaypoints = _waypoints.FindAll(wp => wp.Cluster == byte.MaxValue);
                if (nonClusteredWaypoints.Count >= 1)
                {
                    int tries2 = nonClusteredWaypoints.Count();
                    byte newCluster = (byte)_clusters.Count;
                    _clusters.Add(new WaypointCluster(newCluster));
                    _clusters[newCluster].ClusterCore = nonClusteredWaypoints[0].Id;
                    nonClusteredWaypoints[0].Cluster = newCluster;
                    while (tries2 <= maxTries)
                    {
                        nonClusteredWaypoints.ForEach(wp =>
                        {
                            if (wp.Cluster != byte.MaxValue)
                            {
                                wp.Connections.ForEach(c =>
                                {
                                    AiWaypoint cwp = GetWaypoint(c);
                                    if (cwp.Cluster == byte.MaxValue)
                                    {
                                        cwp.Cluster = wp.Cluster;
                                        fixedCount++;
                                    }
                                });
                            }
                        });
                        tries2++;
                    }
                }
                tries++;
            }

            _clusters.ForEach(cluster =>
            {
                cluster.FindConnectedClusters();
            });

            _clusters.ForEach(cluster =>
            {
                cluster.FindNewCore();
            });

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
            float dist = Mathf.Infinity;
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
                    _pathingStats.KnownPathsUsed++;
                    return foundBakedPath.Path;
                }

                if (_knownPaths.TryGetValue(new System.Tuple<ushort, ushort>(end.Id, start.Id), out foundBakedPath))
                {
                    _pathingStats.KnownPathsUsed++;
                    return foundBakedPath.Path.Reversed();
                }
            }

            //  Path is not yet baked.

            WaypointPath chosenPath = null;
            List<ushort> chosenPathList = null;
            float shortestPathDistance = Mathf.Infinity;
            float longestPathDistance = Mathf.NegativeInfinity;

            for (int attempt = 0; attempt < _pathingAttempts; attempt++)
            {
                WaypointPath forwardWaypointPath = new WaypointPath(start.Id, end.Id, new List<ushort>());
                List<ushort> forwardPath = Breadthwise(start, end, attempt);
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
                _knownPaths.Add(new System.Tuple<ushort, ushort>(start.Id, end.Id), chosenPath);
                _knownPathsList.Add(chosenPath);
            }
            return chosenPathList;
        }

        private List<ushort> Breadthwise(AiWaypoint start, AiWaypoint end, int attempt)
        {
            List<ushort> result = null;

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
                List<byte> dualClusterIds = new List<byte>();
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

            List<byte> commonClusterIds = startCluster.ConnectedClusters.Intersect(endCluster.ConnectedClusters).ToList();
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

            List<byte> clusterBreathwiseSearch = ClusterBreadthwise(startCluster, endCluster, attempt);
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

        public List<ushort> Breadthwise(AiWaypoint start, AiWaypoint end, byte clusterId, int attempt)
        {
            if (GetCluster(clusterId) != null)
            {
                List<byte> list = new List<byte>();
                list.Add(clusterId);
                return Breadthwise(start, end, list, attempt);
            }
            else
            {
                return null;
            }
        }

        public List<ushort> Breadthwise(AiWaypoint start, AiWaypoint end, List<byte> validClusters, int attempt)
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
                    int j = (i + attempt) % currentWp.Connections.Count;
                    ushort currentNeighbour = currentWp.Connections[j];
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

        private List<byte> ClusterBreadthwise(WaypointCluster start, WaypointCluster end, int attempt)
        {
            List<byte> result = new List<byte>();
            List<byte> visited = new List<byte>();
            Queue<byte> work = new Queue<byte>();

            start.History = new List<byte>();
            visited.Add(start.Id);
            work.Enqueue(start.Id);
            int tries = 0;

            while (work.Count > 0 && tries < _waypoints.Count)
            {
                tries++;
                byte current = work.Dequeue();
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
                    byte currentNeighbour = currentCluster.ConnectedClusters[i];
                    WaypointCluster currentNeighbourCl = GetCluster(currentNeighbour);
                    if (!visited.Contains(currentNeighbour))
                    {
                        currentNeighbourCl.History = new List<byte>(currentCluster.History);
                        currentNeighbourCl.History.Add(current);
                        visited.Add(currentNeighbour);
                        work.Enqueue(currentNeighbour);
                    }
                }
            }

            return null;
        }
    }
}