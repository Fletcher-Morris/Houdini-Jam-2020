using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class WaypointManager : MonoBehaviour
{
    public List<AiWaypoint> Waypoints = new List<AiWaypoint>();
    public GameObject waypointPrefab;

    public LayerMask mask;
    public int waypointQuantity = 50;
    public float raydius = 100.0f;

    public float waypointRange = 2.0f;

    public Transform parent;

    public void UpdateWaypoints()
    {
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
                if (w1.connectedWaypoints.Contains(w2))
                {

                }
                else
                {
                    Vector3 p1 = w1.transform.position;
                    Vector3 p2 = w2.transform.position;

                    float dist = Vector3.Distance(p1, p2);
                    if(dist <= waypointRange)
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
            });
        });
    }

    public bool update = false;

    void Update()
    {
        if (update) UpdateWaypoints();

        Waypoints.ForEach(w => w.gameObject.SetActive(showWaypoints));

        if (drawLines)
        {
            Waypoints.ForEach(w1 =>
            {
                w1.connectedWaypoints.ForEach(w2 =>
                {
                    Debug.DrawLine(w1.transform.position, w2.transform.position, Color.magenta);
                });
            });
        }
    }

    public bool showWaypoints = true;

    public bool drawLines = false;
}
