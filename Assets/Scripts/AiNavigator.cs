using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AiNavigator
{
    private bool initialized = false;
    public Transform target;
    [HideInInspector] public Transform self;
    private Vector3 m_prevTargetPosition;
    private Transform m_prevTarget;
    public List<AiWaypoint> pathFound = new List<AiWaypoint>();
    public int prevWaypoint;
    public int nextWaypoint;
    [SerializeField] private AiWaypoint m_closestWaypointToTarget;
    public float waypointTollerance = 0.1f;
    public float pathRefreshInterval = 3.0f;
    private float m_pathRefreshTimer;
    public NavPathUpdateMode updateMode = NavPathUpdateMode.Always;

    public bool sphereRaycastTarget = true;
    public LayerMask raycastLayermask = default;

    [HideInInspector] Vector3 targetPosition;

    [SerializeField] private bool debugLines = true;

    public void Initialize(float offset, Transform setSelf)
    {
        if(!Application.isPlaying) return;
        m_pathRefreshTimer = offset;
        self = setSelf;
        initialized = true;
    }

    public void SetTarget(Transform newTarget)
    {
        if(updateMode == NavPathUpdateMode.TargetChanged)
        {
            RecalculatePath();
        }
        target = newTarget;
        if(pathFound == null) return;
        prevWaypoint = -1;
        nextWaypoint = 1;
    }
    public void SetSelfTransform(Transform newTransform)
    {
        self = newTransform;
    }

    public void Update(float delta)
    {
        if(!initialized)
        {
            return;
        }
        m_pathRefreshTimer -= delta;
        if(m_pathRefreshTimer <= 0.0f)
        {
            m_pathRefreshTimer = pathRefreshInterval;
            RecalculateCheck(delta);
        }
    }

    private void RecalculateCheck(float delta)
    {
        if(!initialized)
        {
            Debug.LogWarning("Not initialized!");
            return;
        }
        switch (updateMode)
        {
            case NavPathUpdateMode.Manual:
            break;
            case NavPathUpdateMode.TargetChanged:
                if(m_prevTarget != target && target != null)
                {
                    RecalculatePath(target.position);
                }
            break;
            case NavPathUpdateMode.TargetPositionChanged:
                if(target != null)
                {
                    if(m_prevTargetPosition != target.position)
                    {
                        RecalculatePath();
                    }
                }
            break;
            case NavPathUpdateMode.Always:
                {
                    if(target != null && self != null)
                    {
                        float distToTarget = self.Distance(target);
                        if(distToTarget > waypointTollerance)
                        {
                            AiWaypoint newWaypoint = WaypointManager.Closest(target.position);
                            if (newWaypoint != null)
                            {
                                if(m_closestWaypointToTarget == null)
                                {
                                    RecalculatePath();
                                }
                                else if (newWaypoint.id != m_closestWaypointToTarget.id)
                                {
                                    RecalculatePath();
                                }
                                m_closestWaypointToTarget = newWaypoint;
                            }
                        }
                    }
                }
            break;
        }
    }

    public void RecalculatePath()
    {
        if(target == null)
        {
            Debug.LogWarning("TARGET IS NULL!");
            return;
        }
        m_prevTargetPosition = target.transform.position;
        RecalculatePath(target.transform.position);
    }
    public void RecalculatePath(Vector3 end)
    {
        if(!initialized)
        {
            Debug.LogWarning("Not initialized!");
            return;
        }
        if(self == null)
        {
            Debug.LogWarning("Self transform is NULL!");
            return;
        }
        pathFound = null;
        m_prevTarget = target;


        if(sphereRaycastTarget)
        {
            RaycastHit hit;
            if(Physics.Raycast(end.normalized * 1000, -end.normalized, out hit, 2000, raycastLayermask))
            {
                if(debugLines)
                {
                    Debug.DrawLine(end.normalized * 1000, hit.point, Color.yellow, pathRefreshInterval);
                }
                end = hit.point;
            }
        }

        if(target != null)
        {
            m_prevTargetPosition = end;
            pathFound = WaypointManager.GetPath(self.position, end);
            if(pathFound == null)
            {
                Debug.LogWarning($"Could not find path from '{self.position}' to '{end}'!");
                return;
            }
            prevWaypoint = -1;
            nextWaypoint = 1;
        }
    }

    public AiWaypoint GetWaypointFromIndex(int index)
    {
        if (index == -1 || index >= pathFound.Count || pathFound.Count == 0) return null;
        return pathFound[index];
    }

    public void DrawLines(Vector3 start, float delta)
    {
        if(!initialized)
        {
            return;
        }
        if(pathFound.Count > 0)
        {
            AiWaypoint n = GetWaypointFromIndex(nextWaypoint);
            if(n != null && debugLines) Debug.DrawLine(start, n.transform.position, Color.blue);
            for (int i = 0; i < pathFound.Count - 1; i++)
            {
                if(pathFound[i] != null)
                Debug.DrawLine(pathFound[i].transform.position, pathFound[i + 1].transform.position, Color.green);
            }
        }
    }
}

[System.Serializable]
public enum NavPathUpdateMode
{
    //  Recalculate is called manually.
    Manual,

    //  Recalculate when the target changes.
    TargetChanged,

    //  Recalculate when the target's position changes.
    TargetPositionChanged,

    //  Recalculate every cycle.
    Always
}
