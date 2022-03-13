using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    public float moveSpeed;
    public float gravityForce = 10.0f;
    public float jumpForce = 10.0f;
    public Sheep sheep;
    public LayerMask groundMask;
    [SerializeField] private Vector2 m_inputDir;
    [SerializeField] private bool m_jump;
    [SerializeField] private bool m_grounded;
    [SerializeField] private Vector3 m_gravityDirection;
    [SerializeField] private Camera m_cam;

    private Rigidbody m_body;

    public bool isCarrying
    {
        get { return sheep != null; }
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        m_body = GetComponent<Rigidbody>();
        m_cam = Camera.main;
    }

    private void Update()
    {
        m_inputDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        m_jump = Input.GetButtonDown("Jump");

        Movement();
    }

    private void FixedUpdate()
    {
        m_gravityDirection = -transform.position.normalized;

        m_body.AddForce(m_gravityDirection * gravityForce);

        {
            var ray = new Ray(transform.position, m_gravityDirection);
            RaycastHit hit;
            m_grounded = false;
            if (Physics.Raycast(ray, out hit, 1.0f, groundMask, QueryTriggerInteraction.UseGlobal)) m_grounded = true;
        }

        if (m_jump && m_grounded) m_body.AddForce(transform.position.normalized * jumpForce, ForceMode.Impulse);
    }

    private void Movement()
    {
    }

    private void CameraMovement()
    {
    }
}