using UnityEngine;
using UnityEngine.UI;

public class GlobeCam : MonoBehaviour
{
    public bool enableMovement;
    public float minRotateSpeed = 15.0f;
    public float maxRotateSpeed = 45.0f;
    [SerializeField] private float m_rotSpeed = 45.0f;
    public float _scrollZoomSpeed = 4.0f;
    public float _maxZoomSpeed = 10.0f;
    public float closeUpZoomStart = 70.0f;
    public float minZoom = 60.0f;
    public float maxZoom = 150.0f;
    public float defaultZoom = 100.0f;
    [SerializeField] private float _zoomValue = 100.0f;

    public Sheep focusSheep;
    [SerializeField] private bool m_followFocusTarget;
    [SerializeField] private float m_followRotationLerpSpeed = 15.0f;
    [SerializeField] private float rotationDragLerp = 2.0f;

    [SerializeField] private Joystick m_joystick;

    private float m_rotDragX;
    private float m_rotDragY;

    private int m_selectedSheep;

    private Vector2 m_targetCamRotation;
    private Transform m_xAxis;

    private Transform m_yAxis;

    private void Start()
    {
        if (m_yAxis == null)
        {
            m_yAxis = new GameObject("GlobeCam_Y").transform;
            m_yAxis.transform.position = Vector3.zero;
            m_yAxis.transform.rotation = Quaternion.identity;

            m_xAxis = new GameObject("GlobeCam_X").transform;
            m_xAxis.parent = m_yAxis;
            m_xAxis.transform.position = Vector3.zero;
            m_xAxis.transform.rotation = Quaternion.identity;

            transform.parent = m_xAxis;
            transform.position = -m_xAxis.forward * defaultZoom;
        }
    }

    private void Update()
    {
        SelectTargetSheep();

        if (!enableMovement) return;

        var inDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (m_joystick != null)
        {
            var jDir = m_joystick.Direction;
            if (jDir != Vector2.zero) inDir = jDir;
        }

        if (inDir.magnitude >= 0.05f) m_followFocusTarget = false;

        m_rotSpeed = Mathf.Lerp(minRotateSpeed, maxRotateSpeed, Mathf.InverseLerp(minZoom, maxZoom, _zoomValue));

        if (m_followFocusTarget)
        {
            //  Follow the predefined target transform.
            m_targetCamRotation = CalcAxisRotations(focusSheep.transform.position);
            var followLerpSpeed = m_followRotationLerpSpeed;
            m_yAxis.localEulerAngles = new Vector3(0,
                Mathf.LerpAngle(m_yAxis.localEulerAngles.y, m_targetCamRotation.y, followLerpSpeed * Time.deltaTime),
                0);
            m_xAxis.localEulerAngles =
                new Vector3(
                    Mathf.LerpAngle(m_xAxis.localEulerAngles.x, m_targetCamRotation.x,
                        followLerpSpeed * Time.deltaTime), 0, 0);
        }
        else
        {
            //  Follow via input axis.
            m_rotDragX += inDir.x;
            m_rotDragY += inDir.y;
            m_rotDragX = m_rotDragX.Clamp(-1.0f, 1.0f);
            m_rotDragY = m_rotDragY.Clamp(-1.0f, 1.0f);
            m_rotDragX = Mathf.Lerp(m_rotDragX, 0.0f, rotationDragLerp * Time.deltaTime);
            m_rotDragY = Mathf.Lerp(m_rotDragY, 0.0f, rotationDragLerp * Time.deltaTime);
            m_yAxis.Rotate(-Vector3.up, m_rotSpeed * Time.deltaTime * m_rotDragX);
            m_xAxis.Rotate(Vector3.right, m_rotSpeed * Time.deltaTime * m_rotDragY);
            m_xAxis.localEulerAngles = new Vector3(m_xAxis.transform.localEulerAngles.x.ClampAngle(0.0f, 89.0f), 0, 0);
        }

        float prevZoomValue = _zoomValue;
        _zoomValue -= Input.mouseScrollDelta.y * _scrollZoomSpeed;
        _zoomValue += (Input.GetKey(KeyCode.Q).ToFloat() - Input.GetKey(KeyCode.E).ToFloat()) * 0.25f;
        _zoomValue = Mathf.Clamp(_zoomValue, prevZoomValue - _maxZoomSpeed, prevZoomValue + _maxZoomSpeed);
        _zoomValue = _zoomValue.Clamp(minZoom, maxZoom);

        var closeUpLerp = Mathf.InverseLerp(closeUpZoomStart, minZoom, _zoomValue).Clamp(0.0f, 1.0f);
        transform.localPosition = Vector3.Lerp(new Vector3(0, 0, -1) * _zoomValue,
            new Vector3(0, 0, -1) * _zoomValue + new Vector3(0, -7.5f, 0), closeUpLerp);
        transform.localEulerAngles = Vector3.Lerp(Vector3.zero, new Vector3(-60.0f, 0, 0), closeUpLerp);
    }

    private void SelectTargetSheep()
    {
        if (Input.GetKeyDown(KeyCode.RightBracket))
            SelectNextSheep();
        else if (Input.GetKeyDown(KeyCode.LeftBracket)) SelectPrevSheep();
    }

    public void SelectNextSheep()
    {
        m_selectedSheep++;
        m_selectedSheep = m_selectedSheep.Loop(0, GameManager.Instance.SheepList.Count - 1);
        SetFocusTarget(GameManager.Instance.SheepList[m_selectedSheep]);
    }

    public void SelectPrevSheep()
    {
        m_selectedSheep--;
        m_selectedSheep = m_selectedSheep.Loop(0, GameManager.Instance.SheepList.Count - 1);
        SetFocusTarget(GameManager.Instance.SheepList[m_selectedSheep]);
    }

    public void SetFocusTarget(Sheep targ)
    {
        focusSheep = targ;
        m_followFocusTarget = false;
        if (focusSheep != null) m_followFocusTarget = true;
    }

    public void SetZoomFromSlider(Slider _slider)
    {
        _zoomValue = Mathf.Lerp(minZoom, maxZoom, _slider.value);
    }

    public Vector2 CalcAxisRotations(Vector3 pos)
    {
        return new Vector2(Mathf.Asin(pos.normalized.y) * Mathf.Rad2Deg,
            180 + pos.ToTopDownVec2().ToAngle());
    }
}