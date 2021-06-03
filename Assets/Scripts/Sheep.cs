using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Sheep : MonoBehaviour
{
	public bool enableMovement = true;
	public float moveSpeed = 3.0f;
	public float rotationSpeed = 10.0f;
	public float bounceHeight = 1.0f;
	public float bounceSpeed = 4.0f;
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
	[SerializeField] private List<AudioClip> m_bleats = new List<AudioClip>();
	public float bleatChance = 0.5f;
	[SerializeField] private float m_bleatInterval = 5.0f;
	[SerializeField] private float m_bleatTimer = 5.0f;

	public AiNavigator navigator = new AiNavigator();
	private readonly float bounceTilt = 10.0f;
	private readonly float legBounceAngle = 30.0f;

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
		m_bleatPitch  = Random.Range(0.8f, 1.2f);
		m_body        = GetComponent<Rigidbody>();
		GameManager.AddSheep(this);
		navigator.Initialize(Random.Range(0.0f, updateWaypointInterval), transform);
	}

	private void Update()
	{
		navigator.Update(Time.deltaTime);

		if (movementVector.magnitude > 0)
			m_bounceTimer += Time.deltaTime * bounceSpeed;
		else
			m_bounceTimer = 0;
		m_bounceDirection              = Mathf.Lerp(-1, 1, m_bounceTimer.FloorToInt().IsEven().ToInt());
		m_bounceValue                  = bounceAnimCurve.Evaluate(m_bounceTimer);
		m_legValue                     = legAnimCurve.Evaluate(m_bounceTimer);
		visual.transform.localPosition = new Vector3(0, m_bounceValue * bounceHeight, 0);
		legs.localEulerAngles          = new Vector3(Mathf.LerpAngle(0, legBounceAngle, m_legValue), 0, 0);
		visual.localEulerAngles =
			new Vector3(0, 0, Mathf.LerpAngle(0.0f, bounceTilt * m_bounceDirection, m_bounceValue));

		navigator.DrawLines(transform.position, Time.deltaTime);

		m_bleatTimer -= Time.deltaTime;
		if (m_bleatTimer <= 0.0f)
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
		gravityDirection = -transform.position.normalized;
		m_body.AddForce(gravityDirection * 10.0f);
		Movement(Time.fixedDeltaTime);
		Rotation(Time.fixedDeltaTime);
	}

	private void Movement(float delta)
	{
		if (navigator.pathFound != null)
		{
			var last = navigator.GetWaypointFromIndex(Mathf.Max(0, navigator.prevWaypoint));
			var next = navigator.GetWaypointFromIndex(navigator.nextWaypoint);
			if (next != null && last != null)
			{
				var lastDist = Vector3.Distance(transform.position, last.position);
				var nextDist = Vector3.Distance(transform.position, next.position);
				if (nextDist < lastDist + navigator.waypointTollerance)
				{
					navigator.prevWaypoint++;
					navigator.nextWaypoint++;
					next = navigator.GetWaypointFromIndex(navigator.nextWaypoint);
				}

				if (navigator.nextWaypoint >= navigator.pathFound.Count)
					targetPosition  = transform.position;
				else targetPosition = next.position;
			}
			else if (next != null)
			{
				targetPosition = next.position;
			}
		}

		var posDiff = targetPosition - transform.position;

		if (posDiff.magnitude > 0.05f && enableMovement && targetPosition != Vector3.zero)
		{
			transform.position += posDiff.normalized * moveSpeed * delta;
			Physics.SyncTransforms();
		}
	}

	private void Rotation(float delta)
	{
		if (updateRotation && Vector3.Distance(transform.position, followTarget.position) > 0.05f)
		{
			Vector3 lookAt;
			if (targetPosition != Vector3.zero && targetPosition != transform.position)
			{
				lookAt               = targetPosition;
				m_lastTargetPosition = targetPosition;
			}
			else if (m_lastTargetPosition != Vector3.zero && m_lastTargetPosition != transform.position)
			{
				lookAt = m_lastTargetPosition;
			}
			else
			{
				lookAt               = Random.onUnitSphere.normalized;
				m_lastTargetPosition = lookAt;
			}

			var forwardsVec = -Vector3.Cross(-gravityDirection, Quaternion.AngleAxis(90.0f, -gravityDirection) * lookAt)
				.normalized;
			//Debug.DrawLine(transform.position, transform.position + forwardsVec, Color.red, delta);
			var newRotation = Quaternion.LookRotation(forwardsVec, -gravityDirection);
			transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, rotationSpeed * delta);
		}
	}

	public void CollectSheep()
	{
		GameManager.CollectSheep(this);
	}
}