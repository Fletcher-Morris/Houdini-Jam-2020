using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

	[SerializeField, HideInInspector] private bool _initialised = false;
    public static bool IsInitialised
    {
        get
        {
            return Instance._initialised;
        }
    }

    [SerializeField] private List<WaypointPath> _bakedPaths = new List<WaypointPath>();
	public List<WaypointPath> BakedPaths { get => _bakedPaths; }
	[SerializeField] private bool _storeKnownPaths = true;
	[SerializeField] private int _knownPathsUsed;
	[SerializeField] private int _newPathsCalculated;

	[SerializeField] private LayerMask _mask;
	[SerializeField] private int _waypointQuantity = 2500;
	[SerializeField] private float _raydius = 100.0f;

	[SerializeField] private float _waypointRange = 5.0f;
	[SerializeField] private int _maxConnections = 10;

	[SerializeField] private bool _deleteAll;
	public bool Reinitialise;

	[SerializeField, Range(0.0f, 1.0f)] private float _lineDebugOpacity = 0;
	public float LineDebugOpacity { get => _lineDebugOpacity; set => _lineDebugOpacity = value; }

	[SerializeField, Range(0.0f, 1.0f)] private float _showCluster;

	[SerializeField] private bool _showClusters = true;
	[SerializeField] private int _clusterCount = 10;
	[SerializeField] private bool m_cullLines = true;
	[SerializeField, Range(0,1)] private float _waitTime = 0.01f;

	[System.Serializable]
    public class WaypointCluster
    {
		public ushort Id;
		public List<ushort> Waypoints = new List<ushort>();
		public ushort ClusterCore;

        public WaypointCluster(int id)
        {
            Id = (ushort)id;
        }
    }
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
	public int WaypointCount => _waypoints.Count;
    public static AiWaypoint GetWaypoint(ushort id)
    {
		if(id == ushort.MaxValue || id > Instance._waypoints.Count)
        {
			return null;
        }
        return Instance._waypoints[id];
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
	}

	public void DrawLines(Camera _cullCam)
	{
		if (LineDebugOpacity > 0.0f)
		{
			float camToPlanetDist = 0.0f;
			if (m_cullLines && _cullCam != null)
				camToPlanetDist = Vector3.Distance(_cullCam.transform.position, Vector3.zero);
			_waypoints.ForEach(w1 =>
			{
                w1.Connections.ForEach(w2 =>
				{
					if(w1.Id > w2)
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
		_bakedPaths = new List<WaypointPath>();
		_initialised = false;
	}

	public void Start()
    {
		if (Reinitialise || _waypoints.Count == 0) Initialise();
    }

	public IEnumerator Initialise()
	{
		Reinitialise = false;
		_initialised = false;

		DeleteWaypoints();

        List<Vector3> points = Extensions.FibonacciPoints(_waypointQuantity);

		foreach(Vector3 p in points)
		{
            Vector3 pos = p.normalized * _raydius;

            Ray ray = new Ray(pos, -pos);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, Mathf.Infinity, _mask))
			{
				if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
				{
					_waypoints.Add(new AiWaypoint(hit.point + pos.normalized));
				}
			}
		}

		points = Extensions.FibonacciPoints(_clusterCount);
		_clusters = new List<WaypointCluster>();
		for (int i = 0; i < _clusterCount; i++)
		{
			_clusters.Add(new WaypointCluster(i));
		}

        ushort id = 0;
		_waypoints.ForEach(w =>
		{
			w.Id = id;
			id++;
			w.Cluster = (ushort) points.ClosestPoint(w.Position);
			_clusters[w.Cluster].Waypoints.Add(w.Id);
		});

		Debug.Log($"Created {_waypoints.Count} waypoints!");

		yield return UpdateConnections();
	}

	public IEnumerator UpdateConnections()
	{
		foreach(AiWaypoint w1 in _waypoints)
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
						if (dist <= _waypointRange)
						{
							RaycastHit hit;
							Physics.Raycast(p1, p2 - p1, out hit);

							if (hit.collider == null)
							{
								w1.Connections.Add(w2.Id);
								w2.Connections.Add(w1.Id);

								if (w1.Cluster == w2.Cluster)
								{
									w1.ClusterConnections.Add(w2.Id);
									w2.ClusterConnections.Add(w1.Id);

									if(_waitTime > 0) yield return null;
								}
							}
						}
					}
				}
			}
		}

        AiWaypoint[] remove = _waypoints.FindAll(w => w.Connections.Count > _maxConnections).ToArray();
		while (remove.Length > 0)
		{
			remove[0].Remove();
			remove = _waypoints.FindAll(w => w.Connections.Count > _maxConnections).ToArray();
		}

		yield return FixClusters();
	}

	public IEnumerator FixClusters()
    {
		foreach(WaypointCluster cluster in _clusters)
        {
			if (cluster.Waypoints.Count == 0) continue;
			Vector3 avPos = Vector3.zero;
            cluster.Waypoints.ForEach(wp =>
			{
				avPos += GetWaypoint(wp).Position;
			});
			avPos.x = avPos.x / cluster.Waypoints.Count;
			avPos.y = avPos.y / cluster.Waypoints.Count;
			avPos.z = avPos.z / cluster.Waypoints.Count;
            float dist = Mathf.Infinity;
			cluster.Waypoints.ForEach(w =>
			{
				AiWaypoint wp = GetWaypoint(w);
				var d = Vector3.Distance(avPos, wp.Position);
				if (d < dist)
				{
					cluster.ClusterCore = wp.Id;
					dist = d;
				}
			});
			cluster.Waypoints.ForEach(w =>
			{
				AiWaypoint start = GetWaypoint(w);
				AiWaypoint end = GetWaypoint(cluster.ClusterCore);
				if(start != end)
                {
					List<ushort> path = GetBakedPath(start, end);
					if (path == null || path.Count == 0)
					{
						start.Cluster = ushort.MaxValue;
					}
					else
					{
						path.ForEach(pwp =>
						{
							AiWaypoint wp = GetWaypoint(pwp);
							if (wp.Cluster != start.Cluster)
							{
								start.Cluster = ushort.MaxValue;
							}
						});
					}
				}
			});
		}

		int maxTries = _waypoints.FindAll(wp => wp.Cluster == ushort.MaxValue).Count();
		int tries = 0;
		int fixedCount = 0;

		while (tries <= maxTries && fixedCount <= maxTries)
		{
			foreach(AiWaypoint wp in _waypoints)
			{
				if (wp.Cluster != ushort.MaxValue)
				{
					foreach(ushort c in wp.Connections)
					{
						AiWaypoint cwp = GetWaypoint(c);
						if (cwp.Cluster == ushort.MaxValue)
						{
							cwp.Cluster = wp.Cluster;
							Debug.Log($"Re-clustered Waypoint '{cwp.Id}'");
							fixedCount++;
							if (_waitTime > 0) yield return new WaitForSecondsRealtime(_waitTime);
						}
					}
				}
			}
			tries++;
		}

		List<AiWaypoint> waypoints = _waypoints.FindAll(wp => wp.Cluster == ushort.MaxValue);
		maxTries = waypoints.Count();
		tries = 0;

		while (tries <= maxTries)
		{
			waypoints = _waypoints.FindAll(wp => wp.Cluster == ushort.MaxValue);
			if(waypoints.Count >= 1)
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
									Debug.Log($"Re-clustered Waypoint '{cwp.Id}'");
									fixedCount++;
									yield return new WaitForSecondsRealtime(_waitTime);
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
			cluster.Waypoints = new List<ushort>();
			foreach(AiWaypoint wp in _waypoints)
            {
				if(wp.Cluster == cluster.Id)
                {
					cluster.Waypoints.Add(wp.Id);
					if (_waitTime > 0) yield return new WaitForSecondsRealtime(_waitTime);
                }
            }
        }

		Debug.Log($"Re-clustered {fixedCount} Waypoints");

		yield return null;
		_initialised = true;
	}

	public static AiWaypoint Closest(Vector3 pos)
	{
		if (Instance == null) return null;
		if (Instance._waypoints == null) return null;
		if (Instance._waypoints.Count == 0) return null;
		AiWaypoint node = null;
		var dist = Mathf.Infinity;
		Instance._waypoints.ForEach(w =>
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

	public static List<ushort> GetPath(Vector3 start, Vector3 end)
	{
		var startNode = Closest(start);
		var endNode = Closest(end);

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

	public static List<ushort> GetBakedPath(AiWaypoint start, AiWaypoint end)
	{
		if (Instance._storeKnownPaths)
		{
			WaypointPath foundBakedPath = Instance.BakedPaths.Find(print => print._a == start.Id && print._b == end.Id);
			if (foundBakedPath != null)
			{
				//Debug.Log($"Using pre-baked path between waypoints '{start.id}' & '{end.id}'.");
				Instance._knownPathsUsed++;
				return foundBakedPath.Path;
			}

			foundBakedPath = Instance.BakedPaths.Find(print => print._a == end.Id && print._b == start.Id);
			if (foundBakedPath != null)
			{
				//Debug.Log($"Using pre-baked path between waypoints '{start.id}' & '{end.id}'.");
				Instance._knownPathsUsed++;
				return foundBakedPath.Path.Reversed();
			}
		}

        //  Path is not yet baked.
        List<ushort> newPath = Breadthwise(start, end);
		Instance._newPathsCalculated++;
		if (Instance._storeKnownPaths)
        {
            Instance.BakedPaths.Add(new WaypointPath(start.Id, end.Id, newPath));
        }

        return newPath;
	}

	public static List<ushort> Breadthwise(AiWaypoint start, AiWaypoint end)
	{
		List<ushort> result = null;
		if (start.Cluster == end.Cluster) result = Breadthwise(start, end, Instance.Clusters[start.Cluster].Waypoints);
		if (result != null) return result;
		result = Breadthwise(start, end, null);
		return result;
	}

	public static List<ushort> Breadthwise(AiWaypoint start, AiWaypoint end, List<ushort> searchList)
	{
        bool searchAll = true;
		if (searchList == null)
        {
            //  No cluster provided, search all waypoints.
            searchList = Instance._waypoints.Select(wp => wp.Id).ToList();
        }
        else
        {
            searchAll = searchList.Count == Instance._waypoints.Count;
        }

        var result = new List<ushort>();
		var visited = new List<ushort>();
		var work = new Queue<ushort>();

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
			for (var i = 0; i < (searchAll ? currentWp.Connections.Count : currentWp.ClusterConnections.Count); i++)
			{
                ushort currentNeighbour = searchAll ? currentWp.Connections[i] : currentWp.ClusterConnections[i];
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

		//Route not found, loop ends

		if (searchAll)
			Debug.LogWarning($"Could not find path between '{start.Id}' and '{end.Id}'!");
		else
			Debug.LogWarning(
				$"Could not find path between '{start.Id}' and '{end.Id}' within cluster '{start.Cluster}'");

		return null;
	}
}