using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GlobeCam : MonoBehaviour
{
    public bool enableMovement = false;
    public float minRotateSpeed = 15.0f;
    public float maxRotateSpeed = 45.0f;
    [SerializeField] private float m_rotSpeed = 45.0f;
    public float zoomSpeed = 10.0f;
    public float closeUpZoomStart = 70.0f;
    public float minZoom = 60.0f;
    public float maxZoom = 150.0f;
    public float defaultZoom = 100.0f;
    [SerializeField] private float m_zoomValue = 100.0f;

    private Transform m_yAxis = default;
    private Transform m_xAxis = default;

    public Transform focusTarget = default;
    [SerializeField] private bool m_followFocusTarget = false;
    [SerializeField] private float m_followRotationLerpSpeed = 15.0f;

    void Start()
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

    private float m_rotDragX;
    private float m_rotDragY;
    [SerializeField] private float rotationDragLerp = 2.0f;

    private Vector2 m_targetCamRotation = new Vector2();

    [SerializeField] private Joystick m_joystick = default;

    void Update()
    {
        SelectTargetSheep();

        if (!enableMovement) return;

        Vector2 inDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if(m_joystick != null)
        {
            Vector2 jDir = m_joystick.Direction;
            if (jDir != Vector2.zero) inDir = jDir;
        }

        if (inDir.magnitude >= 0.05f) m_followFocusTarget = false;

        m_rotSpeed = Mathf.Lerp(minRotateSpeed, maxRotateSpeed, Mathf.InverseLerp(minZoom, maxZoom, m_zoomValue));

        if (m_followFocusTarget)
        {
            //  Follow the predefined target transform.
            m_targetCamRotation = CalcAxisRotations(focusTarget.position);
            float followLerpSpeed = m_followRotationLerpSpeed;
            m_yAxis.localEulerAngles = new Vector3(0, Mathf.LerpAngle(m_yAxis.localEulerAngles.y, m_targetCamRotation.y, followLerpSpeed * Time.deltaTime), 0);
            m_xAxis.localEulerAngles = new Vector3(Mathf.LerpAngle(m_xAxis.localEulerAngles.x, m_targetCamRotation.x, followLerpSpeed * Time.deltaTime), 0, 0);
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

        m_zoomValue -= Input.mouseScrollDelta.y * zoomSpeed;
        m_zoomValue += (Input.GetKey(KeyCode.Q).ToFloat() - Input.GetKey(KeyCode.E).ToFloat()) * 0.25f * zoomSpeed;
        m_zoomValue = m_zoomValue.Clamp(minZoom, maxZoom);

        float closeUpLerp = Mathf.InverseLerp(closeUpZoomStart, minZoom, m_zoomValue).Clamp(0.0f, 1.0f);
        transform.localPosition = Vector3.Lerp(new Vector3(0, 0, -1) * m_zoomValue, (new Vector3(0, 0, -1) * m_zoomValue) + (new Vector3(0, -7.5f, 0)), closeUpLerp);
        transform.localEulerAngles = Vector3.Lerp(Vector3.zero, new Vector3(-60.0f, 0, 0), closeUpLerp);
    }

    private int m_selectedSheep = 0;
    private void SelectTargetSheep()
    {
        if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            SelectNextSheep();
        }
        else if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            SelectPrevSheep();
        }
    }

    public void SelectNextSheep()
    {
        m_selectedSheep++;
        m_selectedSheep = m_selectedSheep.Loop(0, GameManager.Instance.SheepList.Count - 1);
        SetFocusTarget(GameManager.Instance.SheepList[m_selectedSheep].transform);
    }
    public void SelectPrevSheep()
    {
        m_selectedSheep--;
        m_selectedSheep = m_selectedSheep.Loop(0, GameManager.Instance.SheepList.Count - 1);
        SetFocusTarget(GameManager.Instance.SheepList[m_selectedSheep].transform);
    }

    public void SetFocusTarget(Transform targ)
    {
        focusTarget = targ;
        m_followFocusTarget = false;
        if (focusTarget != null)
        {
            m_followFocusTarget = true;
        }
    }

    public void SetZoomFromSlider(Slider _slider)
    {
        m_zoomValue = Mathf.Lerp(minZoom, maxZoom, _slider.value);
    }

    public Vector2 CalcAxisRotations(Vector3 pos) => new Vector2(Mathf.Asin(pos.normalized.y) * Mathf.Rad2Deg, 180 + pos.ToTopDownVec2().ToAngle());
}
