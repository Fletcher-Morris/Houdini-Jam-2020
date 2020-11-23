using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Sheep : MonoBehaviour
{
    public bool enableMovement = true;
    public float moveSpeed = 3.0f;
    public float bounceHeight = 1.0f;
    public float bounceSpeed = 4.0f;
    private float legBounceAngle = 30.0f;
    private float bounceTilt = 10.0f;
    public AnimationCurve bounceAnimCurve;
    public AnimationCurve legAnimCurve;

    public Transform visual;
    public Transform legs;

    public Vector3 gravityDirection;

    public bool updateRotation = true;

    public Transform followTarget;
    public float updateWaypointInterval = 2.0f;
    public Vector3 targetPosition;
    public Vector3 movementVector;

    private float m_bounceTimer;
    private float m_bounceValue;
    private float m_legValue;
    private float m_bounceDirection = 1.0f;

    private Rigidbody m_body;

    private AudioSource m_audioSource;
    [SerializeField] private List<AudioClip> m_bleats = new List<AudioClip>();
    public float bleatChance = 0.5f;
    [SerializeField] private float m_bleatInterval = 5.0f;
    [SerializeField] private float m_bleatTimer = 5.0f;


    void Start()
    {
        m_audioSource = GetComponent<AudioSource>();
        m_body = GetComponent<Rigidbody>();
        GameManager.AddSheep(this);

        if (followTarget != null) closestWaypointToTarget = WaypointManager.Closest(followTarget.position);
        m_updateWaypointTimer = Random.Range(0.0f, updateWaypointInterval);
    }

    void Update()
    {
        if(updateWaypoint)
        {
            m_updateWaypointTimer = 0.0f;
            updateWaypoint = false;
        }

        m_updateWaypointTimer -= Time.deltaTime;
        if(m_updateWaypointTimer <= 0 && followTarget != null)
        {
            if(Vector3.Distance(transform.position, followTarget.position) > waypointTollerance)
            {
                AiWaypoint newWaypoint = WaypointManager.Closest(followTarget.position);
                if (newWaypoint != null)
                {
                    if(closestWaypointToTarget == null)
                    {
                        if (followTarget != null)
                        {
                            SetPathfindTarget(followTarget.position);
                        }
                    }
                    else if (newWaypoint.id != closestWaypointToTarget.id)
                    {
                        if (followTarget != null)
                        {
                            SetPathfindTarget(followTarget.position);
                        }
                    }
                    closestWaypointToTarget = newWaypoint;
                }
            }
            else
            {
                targetPosition = followTarget.position;
            }

            m_updateWaypointTimer = updateWaypointInterval;
        }

        if (movementVector.magnitude > 0)
        {
            m_bounceTimer += Time.deltaTime * bounceSpeed;
        }
        else
        {
            m_bounceTimer = 0;
        }
        m_bounceDirection = Mathf.Lerp(-1,1, m_bounceTimer.FloorToInt().IsEven().ToInt());
        m_bounceValue = bounceAnimCurve.Evaluate(m_bounceTimer);
        m_legValue = legAnimCurve.Evaluate(m_bounceTimer);
        visual.transform.localPosition = new Vector3(0, m_bounceValue * bounceHeight, 0);
        legs.localEulerAngles = new Vector3(Mathf.LerpAngle(0, legBounceAngle, m_legValue), 0, 0);
        visual.localEulerAngles = new Vector3(0, 0, Mathf.LerpAngle(0.0f, bounceTilt * m_bounceDirection, m_bounceValue));

        if(pathfound.Count > 0)
        {
            AiWaypoint n = GetWaypoint(nextWaypoint);
            if(n != null) Debug.DrawLine(transform.position, n.transform.position, Color.blue);
            for (int i = 0; i < pathfound.Count - 1; i++)
            {
                Debug.DrawLine(pathfound[i].transform.position, pathfound[i + 1].transform.position, Color.green);
            }
        }




        m_bleatTimer -= Time.deltaTime;
        if(m_bleatTimer <= 0.0f)
        {
            m_bleatTimer = m_bleatInterval;

            if(Random.Range(0.0f, 1.0f) <= bleatChance)
            {
                m_audioSource.pitch = Random.Range(0.8f, 1.2f);
                m_audioSource.PlayOneShot(m_bleats.RandomItem());
            }
        }
    }

    void FixedUpdate()
    {
        gravityDirection = -transform.position.normalized;

        m_body.AddForce(gravityDirection * 10.0f);

        Movement();

        if(updateRotation && Vector3.Distance(transform.position, followTarget.position) > 0.05f)
        {
            Vector3 lookAt;
            if(targetPosition != Vector3.zero) lookAt = targetPosition;
            else lookAt = Random.onUnitSphere.normalized;

            Vector3 forwardsVec = -Vector3.Cross(-gravityDirection, Quaternion.AngleAxis(90.0f, -gravityDirection) * lookAt).normalized;
            Debug.DrawLine(transform.position, transform.position + forwardsVec, Color.red, Time.fixedDeltaTime);
            transform.rotation = Quaternion.LookRotation(forwardsVec, -gravityDirection);
        }

    }

    void Movement()
    {
        if(pathfound != null)
        {
            AiWaypoint last = GetWaypoint(Mathf.Max(0,lastWaypoint));
            AiWaypoint next = GetWaypoint(nextWaypoint);
            if (next != null && last != null)
            {
                float lastDist = Vector3.Distance(transform.position, last.transform.position);
                float nextDist = Vector3.Distance(transform.position, next.transform.position);
                if(nextDist < lastDist + waypointTollerance)
                {
                    lastWaypoint++;
                    nextWaypoint++;
                    next = GetWaypoint(nextWaypoint);
                }
                if(nextWaypoint >= pathfound.Count)
                targetPosition = followTarget.position;
                else targetPosition = next.transform.position;
            }
            else if (next != null)
            {
                targetPosition = next.transform.position;
            }

        }

        Vector3 posDiff = targetPosition - transform.position;

        if(posDiff.magnitude > 0.05f && enableMovement && targetPosition != Vector3.zero)
        {
            transform.position += (posDiff.normalized * moveSpeed * Time.fixedDeltaTime);
            Physics.SyncTransforms();
        }
    }

    public void CollectSheep()
    {
        GameManager.CollectSheep(this);
    }

    public List<AiWaypoint> pathfound = new List<AiWaypoint>();
    [SerializeField] int lastWaypoint;
    [SerializeField] int nextWaypoint;
    public bool updateWaypoint = false;
    public void SetPathfindTarget(Vector3 targ)
    {
        pathfound = WaypointManager.GetPath(transform.position, targ);

        if (pathfound == null) return;

        lastWaypoint = -1;
        nextWaypoint = 1;
    }
    private AiWaypoint GetWaypoint(int index)
    {
        if (index == -1 || index >= pathfound.Count || pathfound.Count == 0) return null;
        return pathfound[index];
    }
    private AiWaypoint closestWaypointToTarget;
    public float waypointTollerance = 0.1f;
    private float m_updateWaypointTimer;
}
