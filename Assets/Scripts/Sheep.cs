using System.Collections.Generic;
using UnityEngine;
using Pathing;

[ExecuteInEditMode]
public class Sheep : MonoBehaviour
{
    [SerializeField] private bool enableMovement = true;
    [SerializeField] private float moveSpeed = 3.0f;
    [SerializeField] private float rotationSpeed = 10.0f;
    [SerializeField] private float bounceHeight = 1.0f;
    [SerializeField] private float bounceSpeed = 4.0f;
    [SerializeField] private AnimationCurve bounceAnimCurve;
    [SerializeField] private AnimationCurve legAnimCurve;

    [SerializeField] private Transform visual;
    [SerializeField] private Transform legs;

    private Vector3 gravityDirection;
    [SerializeField] private bool updateRotation = true;

    [SerializeField] private Transform followTarget;
    [SerializeField] private float updateWaypointInterval = 2.0f;
    private Vector3 targetPosition;
    private Vector3 movementVector;
    [SerializeField] private List<AudioClip> m_bleats = new List<AudioClip>();
    [SerializeField] private float bleatChance = 0.5f;
    [SerializeField] private float m_bleatInterval = 5.0f;
    private float m_bleatTimer = 5.0f;

    [SerializeField] private Pathing.AiNavigator navigator = new Pathing.AiNavigator();
    [SerializeField] private readonly float bounceTilt = 10.0f;
    [SerializeField] private readonly float legBounceAngle = 30.0f;

    private AudioSource m_audioSource;
    private float m_bleatPitch = 1.0f;

    private Rigidbody m_body;
    private float m_bounceDirection = 1.0f;

    private float m_bounceTimer;
    private float m_bounceValue;
    private Vector3 m_lastTargetPosition;
    private float m_legValue;

    private void Start()
    {
        m_audioSource = GetComponent<AudioSource>();
        m_bleatPitch = Random.Range(0.8f, 1.2f);
        m_body = GetComponent<Rigidbody>();
        GameManager.AddSheep(this);
        navigator.Initialize(Random.Range(0.0f, updateWaypointInterval), transform);
    }

    private void Update()
    {
        float deltaT = Time.deltaTime;

        navigator.Update(deltaT);
        navigator.DrawLines(transform.position, deltaT);

        Bouncing(deltaT);
        Bleating(deltaT);
    }

    private void Bouncing(float deltaT)
    {
        if (movementVector.magnitude > 0)
        {
            m_bounceTimer += deltaT * bounceSpeed;
        }
        else
        {
            m_bounceTimer = 0;
        }

        m_bounceDirection = Mathf.Lerp(-1, 1, m_bounceTimer.FloorToInt().IsEven().ToInt());
        m_bounceValue = bounceAnimCurve.Evaluate(m_bounceTimer);
        m_legValue = legAnimCurve.Evaluate(m_bounceTimer);
        visual.transform.localPosition = new Vector3(0, m_bounceValue * bounceHeight, 0);
        legs.localEulerAngles = new Vector3(Mathf.LerpAngle(0, legBounceAngle, m_legValue), 0, 0);
        visual.localEulerAngles = new Vector3(0, 0, Mathf.LerpAngle(0.0f, bounceTilt * m_bounceDirection, m_bounceValue));
    }

    private void Bleating(float deltaT)
    {
        if ((m_bleatTimer -= deltaT) <= 0.0f)
        {
            m_bleatTimer = m_bleatInterval;

            if (Random.Range(0.0f, 1.0f) <= bleatChance)
            {
                m_audioSource.pitch = m_bleatPitch;
                m_audioSource.PlayOneShot(m_bleats.RandomItem());
            }
        }
    }

    private void FixedUpdate()
    {
        float deltaT = Time.fixedDeltaTime;
        gravityDirection = -transform.position.normalized;
        m_body.AddForce(gravityDirection * 10.0f);
        Movement(deltaT);
        Rotation(deltaT);
    }

    private void Movement(float deltaT)
    {
        if (navigator.pathFound != null)
        {
            AiWaypoint last = WaypointManager.Instance.GetWaypoint(navigator.GetWaypointFromIndex(Mathf.Max(0, navigator.prevWaypoint)));
            AiWaypoint next = WaypointManager.Instance.GetWaypoint(navigator.GetWaypointFromIndex(navigator.nextWaypoint));
            if (next != null && last != null)
            {
                float lastDist = Vector3.Distance(transform.position, last.Position);
                float nextDist = Vector3.Distance(transform.position, next.Position);
                if (nextDist < lastDist + navigator.waypointTollerance)
                {
                    navigator.prevWaypoint++;
                    navigator.nextWaypoint++;
                    next = WaypointManager.Instance.GetWaypoint(navigator.GetWaypointFromIndex(navigator.nextWaypoint));
                }

                if (navigator.nextWaypoint >= navigator.pathFound.Count)
                {
                    targetPosition = transform.position;
                }
                else
                {
                    targetPosition = next.Position;
                }
            }
            else if (next != null)
            {
                targetPosition = next.Position;
            }
        }

        Vector3 posDiff = targetPosition - transform.position;

        if (posDiff.magnitude > 0.05f && enableMovement && targetPosition != Vector3.zero)
        {
            transform.position += posDiff.normalized * moveSpeed * deltaT;
            movementVector = transform.position - m_body.position;
            Physics.SyncTransforms();
        }
        else
        {
            movementVector = Vector3.zero;
        }

    }

    private void Rotation(float deltaT)
    {
        if (updateRotation && Vector3.Distance(transform.position, followTarget.position) > 0.05f)
        {
            Vector3 lookAt;
            if (targetPosition != Vector3.zero && targetPosition != transform.position)
            {
                lookAt = targetPosition;
                m_lastTargetPosition = targetPosition;
            }
            else if (m_lastTargetPosition != Vector3.zero && m_lastTargetPosition != transform.position)
            {
                lookAt = m_lastTargetPosition;
            }
            else
            {
                lookAt = Random.onUnitSphere.normalized;
                m_lastTargetPosition = lookAt;
            }

            Vector3 forwardsVec = -Vector3.Cross(-gravityDirection, Quaternion.AngleAxis(90.0f, -gravityDirection) * lookAt).normalized;
            //Debug.DrawLine(transform.position, transform.position + forwardsVec, Color.red, delta);
            var newRotation = Quaternion.LookRotation(forwardsVec, -gravityDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, rotationSpeed * deltaT);
        }
    }

    public void CollectSheep()
    {
        GameManager.CollectSheep(this);
    }
}