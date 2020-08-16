using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sheep : MonoBehaviour
{
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

    public Transform followTarget;
    public Vector3 targetPosition;
    public Vector3 movementVector;

    private float m_bounceTimer;
    private float m_bounceValue;
    private float m_legValue;
    private float m_bounceDirection = 1.0f;


    void Start()
    {
        GameManager.AddSheep(this);
    }

    void Update()
    {
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
    }

    void FixedUpdate()
    {
        gravityDirection = -transform.position.normalized;

        Movement();
    }

    void Movement()
    {
        if (followTarget != null) targetPosition = followTarget.position;

        if (movementVector.magnitude > 0)
        {

        }
    }

    public void CollectSheep()
    {
        GameManager.CollectSheep(this);
    }
}
