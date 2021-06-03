using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class WaypointManager : MonoBehaviour
{
	public static WaypointManager Instance;

	public List<WaypointPath> bakedPaths;
	[SerializeField] private bool m_storeKnownPaths = true;
	[SerializeField] private int m_knownPathsUsed;
	[SerializeField] private int m_newPathsCalculated;

	public LayerMask mask;
	public int waypointQuantity = 50;
	public float raydius = 100.0f;

	public float waypointRange = 5.0f;
	public int maxConnections = 10;

	public bool deleteAll;
	public bool updateAll;
	public bool updateConnections;

	[Range(0.0f, 1.0f)] public float lineDebugOpacity = 0.25f;
	public bool showClusters = true;
	[SerializeField] private int clusterCount = 8;
	[SerializeField] private Camera m_lineCullCam;
	[SerializeField] private bool m_cullLines = true;

	[HideInInspector] public List<List<AiWaypoint>> waypointClusters;

	public List<AiWaypoint> Waypoints = new List<AiWaypoint>();

	private void Awake()
	{
		Instance = this;
	}

	private void Update()
	{
		if (deleteAll) DeleteWaypoints();
		if (updateAll) UpdateWaypoints();
		if (updateConnections) UpdateConnections();

		if (lineDebugOpacity > 0.0f)
		{
			var camToPlanetDist = 0.0f;
			if (m_cullLines && m_lineCullCam != null)
				camToPlanetDist = Vector3.Distance(m_lineCullCam.transform.position, transform.position);
			Waypoints.ForEach(w1 =>
			{
				w1.connections.ForEach(w2 =>
				{
					var cullLine = false;
					if (m_lineCullCam == null || m_cullLines == false)
					{
					}
					else
					{
						var dist = Mathf.Min(Vector3.Distance(m_lineCullCam.transform.position, w1.position),
							Vector3.Distance(m_lineCullCam.transform.position,                  w2.position));
						cullLine = dist > camToPlanetDist;
					}

					if (cullLine == false)
					{
						var lineCol = w1.cluster == w2.cluster
							? showClusters ? w1.cluster.NumberToColor(clusterCount) : Color.magenta
							: Color.grey;
						lineCol.a = lineDebugOpacity;
						Debug.DrawLine(w1.position, w2.position, lineCol);
					}
				});
			});
		}
	}

	private void OnGUI()
	{
		GUILayout.BeginArea(new Rect(5.0f, 5.0f, 100.0f, 50.0f));
		GUILayout.Label("Debug Opacity");
		lineDebugOpacity = GUILayout.HorizontalSlider(lineDebugOpacity, 0.0f, 1.0f);
		GUILayout.EndArea();
	}

	private void DeleteWaypoints()
	{
		deleteAll = false;
		Waypoints = new List<AiWaypoint>();
	}

	public void UpdateWaypoints()
	{
		Instance  = this;
		updateAll = false;

		DeleteWaypoints();

		var points = Extensions.FibonacciPoints(Instance.waypointQuantity);

		points.ForEach(p =>
		{
			var pos = p.normalized * raydius;

			var ray = new Ray(pos, -pos);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
				if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
					Waypoints.Add(new AiWaypoint(hit.point + pos.normalized));
		});

		points           = Extensions.FibonacciPoints(clusterCount);
		waypointClusters = new List<List<AiWaypoint>>();
		for (var i = 0; i < clusterCount; i++)
		{
			waypointClusters.Add(new List<AiWaypoint>());
		}

		var id = 0;
		Waypoints.ForEach(w =>
		{
			w.id = id;
			id++;
			w.cluster = points.ClosestPoint(w.position);
			waypointClusters[w.cluster].Add(w);
		});

		Debug.Log($"Created {Waypoints.Count} waypoints!");

		UpdateConnections();
	}

	public void UpdateConnections()
	{
		updateConnections = false;

		Waypoints.ForEach(w1 =>
		{
			Waypoints.ForEach(w2 =>
			{
				if (w1 != w2)
				{
					if (w1.connections.Contains(w2))
					{
					}
					else
					{
						var p1 = w1.position;
						var p2 = w2.position;

						var dist = Vector3.Distance(p1, p2);
						if (dist <= waypointRange)
						{
							RaycastHit hit;
							Physics.Raycast(p1, p2 - p1, out hit);

							if (hit.collider == null)
							{
								w1.connections.Add(w2);
								w2.connections.Add(w1);

								if (w1.cluster == w2.cluster)
								{
									w1.clusterConnections.Add(w2);
									w2.clusterConnections.Add(w1);
								}
							}
						}
					}
				}
			});
		});

		var remove = Waypoints.FindAll(w => w.connections.Count > maxConnections).ToArray();
		while (remove.Length > 0)
		{
			remove[0].Remove();
			remove = Waypoints.FindAll(w => w.connections.Count > maxConnections).ToArray();
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
			var d = Vector3.Distance(pos, w.position);
			if (d < dist)
			{
				node = w;
				dist = d;
			}
		});
		if (node == null) Debug.LogWarning("Cannot Find Clolsest Waypoint To Target!");
		return node;
	}

	public static List<AiWaypoint> GetPath(Vector3 start, Vector3 end)
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

	public static List<AiWaypoint> GetBakedPath(AiWaypoint start, AiWaypoint end)
	{
		if (Instance.m_storeKnownPaths)
		{
			WaypointPath foundBakedPath = null;
			foundBakedPath = Instance.bakedPaths.Find(print => print.A == start && print.B == end);
			if (foundBakedPath != null)
			{
				//Debug.Log($"Using pre-baked path between waypoints '{start.id}' & '{end.id}'.");
				Instance.m_knownPathsUsed++;
				return foundBakedPath.Path;
			}

			foundBakedPath = Instance.bakedPaths.Find(print => print.A == end && print.B == start);
			if (foundBakedPath != null)
			{
				//Debug.Log($"Using pre-baked path between waypoints '{start.id}' & '{end.id}'.");
				Instance.m_knownPathsUsed++;
				return foundBakedPath.Path.Reversed();
			}
		}

		//  Path is not yet baked.
		var newPath = Breadthwise(start, end);
		Instance.m_newPathsCalculated++;
		if (Instance.m_storeKnownPaths) Instance.bakedPaths.Add(new WaypointPath(start, end, newPath));
		return newPath;
	}

	public static List<AiWaypoint> Breadthwise(AiWaypoint start, AiWaypoint end)
	{
		List<AiWaypoint> result = null;
		if (start.cluster == end.cluster) result = Breadthwise(start, end, Instance.waypointClusters[start.cluster]);
		if (result != null) return result;
		result = Breadthwise(start, end, null);
		return result;
	}

	public static List<AiWaypoint> Breadthwise(AiWaypoint start, AiWaypoint end, List<AiWaypoint> searchList)
	{
		var searchAll = true;
		if (searchList == null)
			//  No cluster provided, search all waypoints.
			searchList = Instance.Waypoints;
		else
			searchAll = searchList.Count == Instance.Waypoints.Count;

		var result = new List<AiWaypoint>();
		var visited = new List<AiWaypoint>();
		var work = new Queue<AiWaypoint>();

		start.history = new List<AiWaypoint>();
		visited.Add(start);
		work.Enqueue(start);
		var tries = 0;

		while (work.Count > 0 && tries < searchList.Count)
		{
			tries++;
			var current = work.Dequeue();
			if (current == end)
			{
				//Found Node
				result = current.history;
				result.Add(current);
				return result;
			}

			//Didn't find Node
			for (var i = 0; i < (searchAll ? current.connections.Count : current.clusterConnections.Count); i++)
			{
				var currentNeighbor = searchAll ? current.connections[i] : current.clusterConnections[i];
				if (!visited.Contains(currentNeighbor))
				{
					currentNeighbor.history = new List<AiWaypoint>(current.history);
					currentNeighbor.history.Add(current);
					visited.Add(currentNeighbor);
					work.Enqueue(currentNeighbor);
				}
			}
		}

		//Route not found, loop ends

		if (searchAll)
			Debug.LogWarning($"Could not find path between '{start.id}' and '{end.id}'!");
		else
			Debug.LogWarning(
				$"Could not find path between '{start.id}' and '{end.id}' within cluster '{start.cluster}'");

		return null;
	}
}