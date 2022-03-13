using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Pathing
{
    [CreateAssetMenu(fileName = "Waypoint Manager", menuName = "Scriptables/Waypoints/Waypoint Manager")]
    public class WaypointManager : ScriptableObject
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

        [SerializeField] private bool _initialised = false;
        public static bool IsInitialised
        {
            get
            {
                return Instance._initialised;
            }
        }

        [SerializeField] private bool _deleteAll;
        public bool Reinitialise;

        [Space]
        public WaypointManagerSettings Settings = new WaypointManagerSettings(2000, 5, 10, 100);
        [Space]

        [SerializeField] private Dictionary<System.Tuple<ushort, ushort>, WaypointPath> _bakedPaths = new Dictionary<System.Tuple<ushort, ushort>, WaypointPath>();
        public Dictionary<System.Tuple<ushort, ushort>, WaypointPath> BakedPaths { get => _bakedPaths; }
        [SerializeField] private bool _storeKnownPaths = true;
        [SerializeField] private int _knownPathsUsed;
        [SerializeField] private int _newPathsCalculated;

        [SerializeField, Range(0.0f, 1.0f)] private float _lineDebugOpacity = 0;
        public float LineDebugOpacity { get => _lineDebugOpacity; set => _lineDebugOpacity = value; }

        [SerializeField, Range(0.0f, 1.0f)] private float _showCluster;

        [SerializeField] private bool _showClusters = true;
        [SerializeField] private bool m_cullLines = true;
        [SerializeField] private bool _waitTime;


        [SerializeField] private List<WaypointCluster> _clusters = new List<WaypointCluster>();
        public List<WaypointCluster> Clusters { get => _clusters; }
        public WaypointCluster GetCluster(ushort id)
        {
            if (id == ushort.MaxValue || id > _clusters.Count)
            {
                return null;
            }
            return _clusters[id];
        }

        [SerializeField] private List<AiWaypoint> _waypoints = new List<AiWaypoint>();
        public List<AiWaypoint> Waypoints { get => _waypoints; }
        public int WaypointCount => Waypoints.Count;

        public AiWaypoint GetWaypoint(ushort id)
        {
            if (id == ushort.MaxValue || id > Instance.Waypoints.Count)
            {
                return null;
            }
            return Instance.Waypoints[id];
        }

        public AiWaypoint GetWaypointInstance(ushort id)
        {
            if (id == ushort.MaxValue || id > Waypoints.Count)
            {
                return null;
            }
            return Waypoints[id];
        }

        public void RemoveWaypoint(AiWaypoint waypoint)
        {
            Waypoints.Remove(waypoint);
        }

        private void OnValidate()
        {
            if (_deleteAll) DeleteWaypoints();
            if (Settings.UseRecommendedValues) Settings.SetRecommendedValues();
        }

        public void DrawLines(Camera _cullCam)
        {
            if (LineDebugOpacity > 0.0f)
            {
                float camToPlanetDist = 0.0f;
                if (m_cullLines && _cullCam != null)
                    camToPlanetDist = Vector3.Distance(_cullCam.transform.position, Vector3.zero);
                Waypoints.ForEach(w1 =>
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

                            int showCluster = Mathf.Lerp(0.0f, (float)_clusters.Count, _showCluster).CeilToInt();
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

                                        lineCol.a = LineDebugOpacity;

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
                                        lineCol.a = LineDebugOpacity;
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
            LineDebugOpacity = GUILayout.HorizontalSlider(LineDebugOpacity, 0.0f, 1.0f);
            GUILayout.EndArea();
        }

        private void DeleteWaypoints()
        {
            _deleteAll = false;
            _waypoints = new List<AiWaypoint>();
            _clusters = new List<WaypointCluster>();
            _bakedPaths = new Dictionary<System.Tuple<ushort, ushort>, WaypointPath>();
            _initialised = false;
            _knownPathsUsed = 0;
            _newPathsCalculated = 0;
        }

        public void Start()
        {
            if (Reinitialise || Waypoints.Count == 0) Initialise();
        }

        public void Initialise()
        {
            Reinitialise = false;
            _initialised = false;

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
                        Waypoints.Add(new AiWaypoint(hit.point + pos.normalized));
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
            Waypoints.ForEach(w =>
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

            Debug.Log($"Created {Waypoints.Count} waypoints!");

            UpdateConnections();
        }

        public void UpdateConnections()
        {
            foreach (AiWaypoint w1 in Waypoints)
            {
                foreach (AiWaypoint w2 in Waypoints)
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
                foreach (AiWaypoint wp in Waypoints)
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
                foreach (AiWaypoint wp in Waypoints)
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


            int maxTries = Waypoints.FindAll(wp => wp.Cluster == ushort.MaxValue).Count();
            int tries = 0;
            int fixedCount = 0;

            while (tries <= maxTries && fixedCount <= maxTries)
            {
                foreach (AiWaypoint wp in Waypoints)
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

            List<AiWaypoint> waypoints = Waypoints.FindAll(wp => wp.Cluster == ushort.MaxValue);
            maxTries = waypoints.Count();
            tries = 0;

            while (tries <= maxTries)
            {
                waypoints = Waypoints.FindAll(wp => wp.Cluster == ushort.MaxValue);
                if (waypoints.Count >= 1)
                {
                    int tries2 = waypoints.Count();
                    ushort newCluster = (ushort)_clusters.Count;
                    _clusters.Add(new WaypointCluster(newCluster));
                    _clusters[newCluster].ClusterCore = waypoints[0].Id;
                    waypoints[0].Cluster = newCluster;
                    while (tries2 <= maxTries)
                    {
                        foreach (AiWaypoint wp in waypoints)
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

            foreach (AiWaypoint wp in Waypoints)
            {
                wp.ClusterConnections = new List<ushort>();
            }

            foreach (AiWaypoint w1 in Waypoints)
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
            if (Instance.Waypoints == null) return null;
            if (Instance.Waypoints.Count == 0) return null;
            AiWaypoint node = null;
            var dist = Mathf.Infinity;
            Instance.Waypoints.ForEach(w =>
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

            if (Instance._storeKnownPaths)
            {
                if (Instance.BakedPaths.TryGetValue(new System.Tuple<ushort, ushort>(start.Id, end.Id), out foundBakedPath))
                {
                    Instance._knownPathsUsed++;
                    return foundBakedPath.Path;
                }

                if (Instance.BakedPaths.TryGetValue(new System.Tuple<ushort, ushort>(end.Id, start.Id), out foundBakedPath))
                {
                    Instance._knownPathsUsed++;
                    return foundBakedPath.Path.Reversed();
                }
            }

            //  Path is not yet baked.
            List<ushort> newPath = Breadthwise(start, end);
            if (newPath != null && foundBakedPath != null)
            {
                foundBakedPath.Path = newPath;
                Instance._newPathsCalculated++;
                if (Instance._storeKnownPaths)
                {
                    Instance.BakedPaths.Add(new System.Tuple<ushort, ushort>(start.Id, end.Id), foundBakedPath);
                }
            }

            return newPath;
        }

        public List<ushort> Breadthwise(AiWaypoint start, AiWaypoint end)
        {
            List<ushort> result = null;

            if (start.Cluster == end.Cluster)
            {
                result = Breadthwise(start, end, Instance.Clusters[start.Cluster].Waypoints);
                if (result != null)
                {
                    return result;
                }
                else
                {
                    Debug.LogWarning($"Nodes '{start.Id}' and '{end.Id}' are both in cluster '{start.Cluster}', but a path could not be found within that cluster!");
                }
            }
            else if (Instance.GetCluster(start.Cluster).ConnectedClusters.Contains(end.Cluster))
            {
                List<ushort> combinedClusterList = new List<ushort>();
                combinedClusterList.AddRange(Instance.GetCluster(start.Cluster).Waypoints);
                combinedClusterList.AddRange(Instance.GetCluster(end.Cluster).Waypoints);
                result = Breadthwise(start, end, Instance.Clusters[start.Cluster].Waypoints);
                if (result != null)
                {
                    return result;
                }
                else
                {
                    Debug.LogWarning($"Nodes '{start.Id}' and '{end.Id}' are in connected clusters '{start.Cluster}' and '{end.Cluster}', but a path could not be found within those clusters!");
                }
            }

            result = Breadthwise(start, end, null);
            return result;
        }

        public List<ushort> Breadthwise(AiWaypoint start, AiWaypoint end, ushort clusterId)
        {
            if (Instance.GetCluster(clusterId) != null)
            {
                return Breadthwise(start, end, Instance.GetCluster(clusterId).Waypoints);
            }
            else
            {
                return null;
            }
        }

        public List<ushort> Breadthwise(AiWaypoint start, AiWaypoint end, List<ushort> searchList)
        {
            if (searchList == null)
            {
                //  No cluster provided, search all waypoints.
                searchList = Instance.Waypoints.Select(wp => wp.Id).ToList();
            }

            List<ushort> result = new List<ushort>();
            List<ushort> visited = new List<ushort>();
            Queue<ushort> work = new Queue<ushort>();

            start.History = new List<ushort>();
            visited.Add(start.Id);
            work.Enqueue(start.Id);
            int tries = 0;

            while (work.Count > 0 && tries < searchList.Count)
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
                for (var i = 0; i < (currentWp.Connections.Count); i++)
                {
                    ushort currentNeighbour = currentWp.Connections[i];
                    if (searchList.Contains(currentNeighbour))
                    {
                        AiWaypoint currentNeighbourWp = GetWaypoint(currentNeighbour);
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