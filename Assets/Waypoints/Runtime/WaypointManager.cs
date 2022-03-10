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
	[SerializeField] private bool _reinitialise;

	[SerializeField, Range(0.0f, 1.0f)] private float _lineDebugOpacity = 0;
	public float LineDebugOpacity { get => _lineDebugOpacity; set => _lineDebugOpacity = value; }

	[SerializeField] private bool _showClusters = true;
	[SerializeField] private int _clusterCount = 10;
	[SerializeField] private bool m_cullLines = true;

	private List<List<ushort>> _waypointClusters = new List<List<ushort>>();
	public List<List<ushort>> WaypointClusters { get => _waypointClusters; }

	[SerializeField] private List<AiWaypoint> _waypoints = new List<AiWaypoint>();
	public List<AiWaypoint> Waypoints { get => _waypoints; }
	public static AiWaypoint GetWaypoint(ushort id) => Instance._waypoints.Find(wp => wp.Id == id);

	private void OnValidate()
	{
		if (_deleteAll) DeleteWaypoints();
	}

	public void FixedUpdate()
	{
		if (_reinitialise) UpdateWaypoints();
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
					AiWaypoint connectedWp = GetWaypoint(w2);
                    bool cullLine = false;
					if (_cullCam == null || m_cullLines == false)
					{
					}
					else
					{
						float dist = Mathf.Min(Vector3.Distance(_cullCam.transform.position, w1.Position),
							Vector3.Distance(_cullCam.transform.position, connectedWp.Position));
						cullLine = dist > camToPlanetDist;
					}

					if (cullLine == false)
					{
                        Color lineCol = w1.Cluster == connectedWp.Cluster ? _showClusters ? ((int)connectedWp.Cluster).NumberToColor(_clusterCount) : Color.magenta
							: Color.grey;
						lineCol.a = LineDebugOpacity;
						Debug.DrawLine(w1.Position, connectedWp.Position, lineCol);
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
		_waypointClusters = new List<List<ushort>>();
		_bakedPaths = new List<WaypointPath>();
	}

	public void UpdateWaypoints()
	{
		_reinitialise = false;

		DeleteWaypoints();

		var points = Extensions.FibonacciPoints(_waypointQuantity);

		points.ForEach(p =>
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
		});

		points = Extensions.FibonacciPoints(_clusterCount);
		_waypointClusters = new List<List<ushort>>(_clusterCount);
		for (int i = 0; i < _clusterCount; i++)
		{
			_waypointClusters.Add(new List<ushort>());
		}

        ushort id = 0;
		_waypoints.ForEach(w =>
		{
			w.Id = id;
			id++;
			w.Cluster = (ushort) points.ClosestPoint(w.Position);
			_waypointClusters[w.Cluster].Add(w.Id);
		});

		Debug.Log($"Created {Waypoints.Count} waypoints!");

		UpdateConnections();
	}

	public void UpdateConnections()
	{
		Waypoints.ForEach(w1 =>
		{
			Waypoints.ForEach(w2 =>
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
								}
							}
						}
					}
				}
			});
		});

        AiWaypoint[] remove = Waypoints.FindAll(w => w.Connections.Count > _maxConnections).ToArray();
		while (remove.Length > 0)
		{
			remove[0].Remove();
			remove = Waypoints.FindAll(w => w.Connections.Count > _maxConnections).ToArray();
		}
	}

	public static AiWaypoint Closest(Vector3 pos)
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
			WaypointPath foundBakedPath = Instance.BakedPaths.Find(print => print.A == start.Id && print.B == end.Id);
			if (foundBakedPath != null)
			{
				//Debug.Log($"Using pre-baked path between waypoints '{start.id}' & '{end.id}'.");
				Instance._knownPathsUsed++;
				return foundBakedPath.Path;
			}

			foundBakedPath = Instance.BakedPaths.Find(print => print.A == end.Id && print.B == start.Id);
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
		if (start.Cluster == end.Cluster) result = Breadthwise(start, end, Instance.WaypointClusters[start.Cluster]);
		if (result != null) return result;
		result = Breadthwise(start, end, null);
		return result;
	}

	public static List<ushort> Breadthwise(AiWaypoint start, AiWaypoint end, List<ushort> searchList)
	{
        bool searchAll = true;
		if (searchList == null)
			//  No cluster provided, search all waypoints.
			searchList = Instance.Waypoints.Select(wp => wp.Id).ToList();
		else
			searchAll = searchList.Count == Instance.Waypoints.Count;

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