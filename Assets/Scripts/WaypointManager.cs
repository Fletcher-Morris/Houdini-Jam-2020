using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class WaypointManager : MonoBehaviour
{
    public static WaypointManager Instance;

    public List<AiWaypoint> Waypoints = new List<AiWaypoint>();
    public GameObject waypointPrefab;

    public List<WaypointPath> bakedPaths = default;
    [SerializeField] private bool m_storeKnownPaths = true;
    [SerializeField] private int m_knownPathsUsed = 0;
    [SerializeField] private int m_newPathsCalculated = 0;

    public LayerMask mask;
    public int waypointQuantity = 50;
    public float raydius = 100.0f;

    public float waypointRange = 5.0f;
    public int maxConnections = 10;

    public Transform parent;

    void Awake()
    {
        Instance = this;
    }

    private void DeleteWaypoints()
    {
        deleteAll = false;
        if (parent != null) DestroyImmediate(parent.gameObject);
        Waypoints = new List<AiWaypoint>();
        parent = new GameObject("WAYPOINT_PARENT").transform;
        parent.position = Vector3.zero;
    }

    public void UpdateWaypoints()
    {
        Instance = this;
        updateAll = false;

        DeleteWaypoints();


        List<Vector3> points = Extensions.FibonacciPoints(Instance.waypointQuantity);


        points.ForEach(p =>
        {
            Vector3 pos = p.normalized * raydius;

            Ray ray = new Ray(pos, -pos);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
            {
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                {
                    //Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.cyan, 10.0f);
                    Quaternion rot = Quaternion.FromToRotation(Vector3.forward, -pos);
                    GameObject newWp = Instantiate(waypointPrefab, hit.point + (pos.normalized), rot, parent);
                    Waypoints.Add(newWp.GetComponent<AiWaypoint>());
                }
            }
        });


        points = Extensions.FibonacciPoints(clusterCount);
        waypointClusters = new List<List<AiWaypoint>>();
        for(int i = 0; i < clusterCount; i++)
        {
            waypointClusters.Add(new List<AiWaypoint>());
        }

        int id = 0;
        Waypoints.ForEach(w => 
        {
            w.id = id; id++;
            w.gameObject.name = "WP_" + w.id;
            w.cluster = points.ClosestPoint(w.transform.position);
            waypointClusters[w.cluster].Add(w);
        });

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
                        Vector3 p1 = w1.transform.position;
                        Vector3 p2 = w2.transform.position;

                        float dist = Vector3.Distance(p1, p2);
                        if (dist <= waypointRange)
                        {
                            RaycastHit hit;
                            Physics.Raycast(p1, p2 - p1, out hit);

                            if (hit.collider == null)
                            {
                                w1.connections.Add(w2);
                                w2.connections.Add(w1);

                                if(w1.cluster == w2.cluster)
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

        AiWaypoint[] remove = Waypoints.FindAll(w => w.connections.Count > maxConnections).ToArray();
        while (remove.Length > 0)
        {
            remove[0].Remove();
            remove = Waypoints.FindAll(w => w.connections.Count > maxConnections).ToArray();
        }
    }

    public bool deleteAll = false;
    public bool updateAll = false;
    public bool updateConnections = false;

    void Update()
    {
        if (deleteAll) DeleteWaypoints();
        if (updateAll) UpdateWaypoints();
        if (updateConnections) UpdateConnections();

        Waypoints.ForEach(w =>
        {
            w.gameObject.SetActive(showWaypoints);
        });

        if (lineDebugOpacity > 0.0f)
        {
            Waypoints.ForEach(w1 =>
            {
                w1.connections.ForEach(w2 =>
                {
                    Color lineCol = (w1.cluster == w2.cluster) ? showClusters ? w1.cluster.NumberToColor(clusterCount) : Color.magenta : Color.grey;
                    lineCol.a = lineDebugOpacity;
                    Debug.DrawLine(w1.transform.position, w2.transform.position, lineCol);
                });
            });
        }
    }

    public bool showWaypoints = true;
    [Range(0.0f,1.0f)] public float lineDebugOpacity = 0.25f;
    public bool showClusters = true;
    [SerializeField] private int clusterCount = 8;

    [HideInInspector] public List<List<AiWaypoint>> waypointClusters = default;

    public static AiWaypoint Closest(Vector3 pos)
    {
        if(Instance == null) return null;
        if(Instance.Waypoints == null) return null;
        if(Instance.Waypoints.Count == 0) return null;
        AiWaypoint node = null;
        float dist = Mathf.Infinity;
        Instance.Waypoints.ForEach(w =>
        {
            float d = Vector3.Distance(pos, w.transform.position);
            if (d < dist)
            {
                node = w;
                dist = d;
            }
        });
        if(node == null)
        {
            Debug.LogWarning("Cannot Find Clolsest Waypoint To Target!");
        }
        return node;
    }

    public static List<AiWaypoint> GetPath(Vector3 start, Vector3 end)
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

    public static List<AiWaypoint> GetBakedPath(AiWaypoint start, AiWaypoint end)
    {
        if(Instance.m_storeKnownPaths)
        {
            WaypointPath foundBakedPath = null;
            foundBakedPath = Instance.bakedPaths.Find(print=>print.A==start&&print.B==end);
            if(foundBakedPath != null)
            {
                Debug.Log($"Using pre-baked path between waypoints '{start.id}' & '{end.id}'.");
                Instance.m_knownPathsUsed++;
                return foundBakedPath.Path;
            }
            foundBakedPath = Instance.bakedPaths.Find(print=>print.A==end&&print.B==start);
            if(foundBakedPath != null)
            {
                Debug.Log($"Using pre-baked path between waypoints '{start.id}' & '{end.id}'.");
                Instance.m_knownPathsUsed++;
                return foundBakedPath.Path.Reversed();
            }
        }
        //  Path is not yet baked.
        List<AiWaypoint> newPath = Breadthwise(start,end);
        Instance.m_newPathsCalculated++;
        if(Instance.m_storeKnownPaths)
        {
            Instance.bakedPaths.Add(new WaypointPath(start,end,newPath));
        }
        return newPath;
    }

    public static List<AiWaypoint> Breadthwise(AiWaypoint start, AiWaypoint end)
    {
        List<AiWaypoint> result = null;
        if(start.cluster == end.cluster) result = Breadthwise(start,end,Instance.waypointClusters[start.cluster]);
        if(result != null) return result;
        result = Breadthwise(start,end,null);
        return result;
    }
    public static List<AiWaypoint> Breadthwise(AiWaypoint start, AiWaypoint end, List<AiWaypoint> searchList)
    {
        bool searchAll = true;
        if(searchList == null)
        {
            //  No cluster provided, search all waypoints.
           searchList = Instance.Waypoints;
        }
        else
        {
            searchAll = searchList.Count == Instance.Waypoints.Count;
        }


        List<AiWaypoint> result = new List<AiWaypoint>();
        List<AiWaypoint> visited = new List<AiWaypoint>();
        Queue<AiWaypoint> work = new Queue<AiWaypoint>();

        start.history = new List<AiWaypoint>();
        visited.Add(start);
        work.Enqueue(start);
        int tries = 0;

        while (work.Count > 0 && tries < searchList.Count)
        {
            tries++;
            AiWaypoint current = work.Dequeue();
            if (current == end)
            {
                //Found Node
                result = current.history;
                result.Add(current);
                return result;
            }
            else
            {
                //Didn't find Node
                for (int i = 0; i < (searchAll ? current.connections.Count : current.clusterConnections.Count); i++)
                {
                    AiWaypoint currentNeighbor = searchAll ? current.connections[i] : current.clusterConnections[i];
                    if (!visited.Contains(currentNeighbor))
                    {
                        currentNeighbor.history = new List<AiWaypoint>(current.history);
                        currentNeighbor.history.Add(current);
                        visited.Add(currentNeighbor);
                        work.Enqueue(currentNeighbor);
                    }
                }
            }
        }

        //Route not found, loop ends

        if(searchAll)
        {
            Debug.LogWarning($"Could not find path between '{start.id}' and '{end.id}'!");
        }
        else
        {
            Debug.LogWarning($"Could not find path between '{start.id}' and '{end.id}' within cluster '{start.cluster}'");
        }

        return null;
    }
}
