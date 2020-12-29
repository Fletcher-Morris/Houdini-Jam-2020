using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobeCam : MonoBehaviour
{
    public bool enableMovement = false;
    public float minRotateSpeed = 15.0f;
    public float maxRotateSpeed = 45.0f;
    [SerializeField] private float m_rotSpeed = 45.0f;
    public float zoomSpeed = 10.0f;
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
        if(m_yAxis == null)
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
    void Update()
    {
        SelectTargetSheep();

        if(!enableMovement) return;
        
        Vector2 inDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if(inDir.magnitude >= 0.05f) m_followFocusTarget = false;

        m_rotSpeed = Mathf.Lerp(minRotateSpeed, maxRotateSpeed, Mathf.InverseLerp(minZoom, maxZoom, m_zoomValue));

        if(m_followFocusTarget)
        {
            //  Follow the predefined target transform.
            m_targetCamRotation = CalcAxisRotations(focusTarget.position);
            m_yAxis.localEulerAngles = new Vector3(0,Mathf.LerpAngle(m_yAxis.localEulerAngles.y, m_targetCamRotation.y, m_followRotationLerpSpeed * Time.deltaTime),0);
            m_xAxis.localEulerAngles = new Vector3(Mathf.LerpAngle(m_xAxis.localEulerAngles.x, m_targetCamRotation.x, m_followRotationLerpSpeed * Time.deltaTime),0,0);
        }
        else
        {
            //  Follow via input axis.
            m_rotDragX += inDir.x;
            m_rotDragY += inDir.y;
            m_rotDragX = m_rotDragX.Clamp(-1.0f,1.0f);
            m_rotDragY = m_rotDragY.Clamp(-1.0f,1.0f);
            m_rotDragX = Mathf.Lerp(m_rotDragX,0.0f,rotationDragLerp * Time.deltaTime);
            m_rotDragY = Mathf.Lerp(m_rotDragY,0.0f,rotationDragLerp * Time.deltaTime);
            m_yAxis.Rotate(-Vector3.up, m_rotSpeed * Time.deltaTime * m_rotDragX);
            m_xAxis.Rotate(Vector3.right, m_rotSpeed * Time.deltaTime * m_rotDragY);
            m_xAxis.localEulerAngles = new Vector3(m_xAxis.transform.localEulerAngles.x.ClampAngle(0.0f,89.0f),0,0);
        }

        m_zoomValue -= Input.mouseScrollDelta.y * zoomSpeed;
        m_zoomValue += (Input.GetKey(KeyCode.Q).ToFloat() - Input.GetKey(KeyCode.E).ToFloat()) * 0.25f * zoomSpeed;
        m_zoomValue = m_zoomValue.Clamp(minZoom,maxZoom);
        transform.position = -m_xAxis.forward * m_zoomValue;
    }

    private void SelectTargetSheep()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetFocusTarget(GameManager.Instance.SheepList[0].transform);
        }
        else if(Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetFocusTarget(GameManager.Instance.SheepList[1].transform);
        }
        else if(Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetFocusTarget(GameManager.Instance.SheepList[2].transform);
        }
        else if(Input.GetKeyDown(KeyCode.Alpha4))
        {
            SetFocusTarget(GameManager.Instance.SheepList[3].transform);
        }
        else if(Input.GetKeyDown(KeyCode.Alpha5))
        {
            SetFocusTarget(GameManager.Instance.SheepList[4].transform);
        }
    }

    public void SetFocusTarget(Transform targ)
    {
        focusTarget = targ;
        m_followFocusTarget = false;
        if(focusTarget != null)
        {
            m_followFocusTarget = true;
        }
    }

    public Vector2 CalcAxisRotations(Vector3 pos) => new Vector2(Mathf.Asin(pos.normalized.y) * Mathf.Rad2Deg, 180 + pos.ToTopDownVec2().ToAngle());
}
