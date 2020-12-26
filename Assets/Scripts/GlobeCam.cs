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
    void Update()
    {
        if(!enableMovement) return;

        m_rotSpeed = Mathf.Lerp(minRotateSpeed, maxRotateSpeed, Mathf.InverseLerp(minZoom, maxZoom, m_zoomValue));
        m_rotDragX += Input.GetAxisRaw("Horizontal");
        m_rotDragY += Input.GetAxisRaw("Vertical");
        m_rotDragX = m_rotDragX.Clamp(-1.0f,1.0f);
        m_rotDragY = m_rotDragY.Clamp(-1.0f,1.0f);
        m_rotDragX = Mathf.Lerp(m_rotDragX,0.0f,rotationDragLerp * Time.deltaTime);
        m_rotDragY = Mathf.Lerp(m_rotDragY,0.0f,rotationDragLerp * Time.deltaTime);
        m_yAxis.Rotate(-Vector3.up, m_rotSpeed * Time.deltaTime * m_rotDragX);
        m_xAxis.Rotate(Vector3.right, m_rotSpeed * Time.deltaTime * m_rotDragY);
        m_xAxis.localEulerAngles = new Vector3(m_xAxis.transform.localEulerAngles.x.ClampAngle(0.0f,89.0f),0,0);

        m_zoomValue -= Input.mouseScrollDelta.y * zoomSpeed;
        m_zoomValue += (Input.GetKey(KeyCode.Q).ToFloat() - Input.GetKey(KeyCode.E).ToFloat()) * 0.25f * zoomSpeed;
        m_zoomValue = m_zoomValue.Clamp(minZoom,maxZoom);
        transform.position = -m_xAxis.forward * m_zoomValue;
    }
}
