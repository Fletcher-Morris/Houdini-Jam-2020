using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class WaypointManager : MonoBehaviour
{
    public static WaypointManager Instance;

    public List<AiWaypoint> Waypoints = new List<AiWaypoint>();
    public GameObject waypointPrefab;

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


        List<Vector3> points = new List<Vector3>();
        int samples = Instance.waypointQuantity;
        float phi = Mathf.PI * (3.0f - Mathf.Sqrt(5.0f));
        for(int i = 0; i < samples; i++)
        {
            float y = 1.0f - (i / (float)(samples - 1) * 2.0f);
            float radius = Mathf.Sqrt(1.0f - (y * y));
            float theta = phi * i;
            float x = Mathf.Cos(theta) * radius;
            float z = Mathf.Sin(theta) * radius;
            points.Add(new Vector3(x, y, z));
        }


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

        int id = 0;
        Waypoints.ForEach(w => { w.id = id; id++; });

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
            w.gameObject.SetActive(showWaypoints && (showClusters < 0 || (w.connections.Count <= showClusters)));
        });

        if (drawLines)
        {
            Waypoints.ForEach(w1 =>
            {
                w1.connections.ForEach(w2 =>
                {
                    if(w1.connections.Count <= showClusters || showClusters < 0)
                    Debug.DrawLine(w1.transform.position, w2.transform.position, Color.magenta);
                });
            });
        }
    }

    public bool showWaypoints = true;
    public int showClusters = 2;
    public bool drawLines = false;

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


        return Breadthwise(startNode, endNode);
    }

    public static List<AiWaypoint> Breadthwise(AiWaypoint start, AiWaypoint end)
    {

        List<AiWaypoint> result = new List<AiWaypoint>();
        List<AiWaypoint> visited = new List<AiWaypoint>();
        Queue<AiWaypoint> work = new Queue<AiWaypoint>();

        start.history = new List<AiWaypoint>();
        visited.Add(start);
        work.Enqueue(start);
        int tries = 0;

        while (work.Count > 0 && tries < Instance.Waypoints.Count)
        {
            tries++;
            AiWaypoint current = work.Dequeue();
            if (current == end)
            {
                //Found Node
                result = current.history;
                result.Add(current);
                //Debug.Log($"Found path between '{start.transform.position}' and '{end.transform.position}' after '{tries}' tries!");
                return result;
            }
            else
            {
                //Didn't find Node
                for (int i = 0; i < current.connections.Count; i++)
                {
                    AiWaypoint currentNeighbor = current.connections[i];
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

        Debug.LogWarning($"Could not find path between '{start.transform.position}' and '{end.transform.position}' within '{tries}' tries!");
        return null;
    }
}
