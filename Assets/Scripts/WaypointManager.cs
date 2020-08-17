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

    public void UpdateWaypoints()
    {
        Instance = this;
        update = false;

        if (parent != null) DestroyImmediate(parent.gameObject);
        Waypoints = new List<AiWaypoint>();
        parent = new GameObject("WAYPOINT_PARENT").transform;
        parent.position = Vector3.zero;

        for (int i = 0; i < waypointQuantity; i++)
        {

            float x = Random.Range(-1.0f, 1.0f);
            float y = Random.Range(-1.0f, 1.0f);
            float z = Random.Range(-1.0f, 1.0f);
            Vector3 pos = new Vector3(x, y, z).normalized * raydius;

            Ray ray = new Ray(pos, -pos);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit, Mathf.Infinity, mask))
            {
                if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                {
                    Quaternion rot = Quaternion.FromToRotation(Vector3.forward, -pos);
                    GameObject newWp = Instantiate(waypointPrefab, hit.point + (pos * 0.01f), rot, parent);
                    Waypoints.Add(newWp.GetComponent<AiWaypoint>());
                }
            }
        }

        Waypoints.ForEach(w1 =>
        {
            Waypoints.ForEach(w2 =>
            {
                if(w1 != w2)
                {
                    if (w1.connectedWaypoints.Contains(w2))
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
                                w1.connectedWaypoints.Add(w2);
                                w2.connectedWaypoints.Add(w1);
                            }
                        }
                    }
                }
            });
        });

        AiWaypoint[] remove = Waypoints.FindAll(w => w.connectedWaypoints.Count > maxConnections).ToArray();
        while(remove.Length > 0)
        {
            remove[0].Remove();
            remove = Waypoints.FindAll(w => w.connectedWaypoints.Count > maxConnections).ToArray();
        }
        
    }

    public bool update = false;

    void Update()
    {
        if (update) UpdateWaypoints();

        Waypoints.ForEach(w => w.gameObject.SetActive(showWaypoints && (showClusters < 0 || (w.connectedWaypoints.Count <= showClusters))));

        if (drawLines)
        {
            Waypoints.ForEach(w1 =>
            {
                w1.connectedWaypoints.ForEach(w2 =>
                {
                    if(w1.connectedWaypoints.Count <= showClusters || showClusters < 0)
                    Debug.DrawLine(w1.transform.position, w2.transform.position, Color.magenta);
                });
            });
        }
    }

    public bool showWaypoints = true;
    public int showClusters = 2;

    public bool drawLines = false;




    Queue<AiWaypoint> q;
    Dictionary<AiWaypoint, AiWaypoint> cameFrom = new Dictionary<AiWaypoint, AiWaypoint>();
    public static List<AiWaypoint> GetPath(Vector3 start, Vector3 end)
    {
        AiWaypoint startNode = null;
        AiWaypoint endNode = null;
        float d1 = Mathf.Infinity;
        float d2 = Mathf.Infinity;
        Instance.Waypoints.ForEach(w =>
        {
            float d = Vector3.Distance(start, w.transform.position);
            if(d < d1)
            {
                startNode = w;
                d1 = d;
            }
            d = Vector3.Distance(end, w.transform.position);
            if (d < d2)
            {
                endNode = w;
                d2 = d;
            }
        });

        if (startNode == null) return null;
        if (endNode == null) return null;

        bool foundPath = false;
        Instance.q = new Queue<AiWaypoint>();
        Instance.q.Enqueue(startNode);
        Instance.cameFrom = new Dictionary<AiWaypoint, AiWaypoint>();
        int tries = 0;
        while (foundPath == false && Instance.q.Count > 0 && tries <= Instance.Waypoints.Count)
        {
            AiWaypoint current = Instance.q.Peek();

            if(current == endNode)
            {
                foundPath = true;
                break;
            }

            current.connectedWaypoints.ForEach(next =>
            {
                if(!Instance.cameFrom.ContainsValue(next))
                {
                    Instance.q.Enqueue(next);
                    Instance.cameFrom[next] = current;
                }
            });
        }

        List<AiWaypoint> path = new List<AiWaypoint>();
        AiWaypoint c = endNode;
        while(c != startNode)
        {
            path.Add(c);
            c = Instance.cameFrom[c];
        }
        path.Add(startNode);
        path.Reverse();


        return path;
    }
}
