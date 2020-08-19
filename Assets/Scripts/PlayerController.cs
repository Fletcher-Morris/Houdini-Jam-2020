using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public static PlayerController Instance;
    void Awake()
    {
        Instance = this;
    }


    public float moveSpeed;
    public float gravityForce = 10.0f;
    public float jumpForce = 10.0f;
    public Sheep sheep;
    public bool isCarrying => sheep != null;
    public LayerMask groundMask;


    private Rigidbody m_body;
    [SerializeField] private Vector2 m_inputDir;
    [SerializeField] private bool m_jump;
    [SerializeField] private bool m_grounded;
    [SerializeField] private Vector3 m_gravityDirection;
    [SerializeField] private Camera m_cam;


    void Start()
    {
        m_body = GetComponent<Rigidbody>();
        m_cam = Camera.main;
    }


    void Update()
    {
        m_inputDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        m_jump = Input.GetButtonDown("Jump");


        Movement();
    }

    void FixedUpdate()
    {
        m_gravityDirection = -transform.position.normalized;

        m_body.AddForce(m_gravityDirection * gravityForce);

        {
            Ray ray = new Ray(transform.position, m_gravityDirection);
            RaycastHit hit;
            m_grounded = false;
            if (Physics.Raycast(ray, out hit, 1.0f, groundMask, QueryTriggerInteraction.UseGlobal))
            {
                m_grounded = true;
            }
        }

        if (m_jump && m_grounded) m_body.AddForce(transform.position.normalized * jumpForce, ForceMode.Impulse);
    }


    void Movement()
    {

    }

    void CameraMovement()
    {

    }

}
