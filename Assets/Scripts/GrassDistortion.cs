using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class GrassDistortion : MonoBehaviour
{
    public static GrassDistortion Instance;
    private void OnAwake()
    {
        Instance = this;
    }
    private void OnEnable()
    {
        Instance = this;
    }

    const int MAX_OBJECTS = 32;
    public List<GrassObject> trackObjects = new List<GrassObject>();
    private List<GrassObject> sortedList = new List<GrassObject>();
    private Vector4 [] m_values = new Vector4 [MAX_OBJECTS];

    private void Update()
    {
        if (Instance == null) Instance = this;

        if (trackObjects.Count <= 0) return;
        if (trackObjects.Count > MAX_OBJECTS)
        {
            CheckClosestObjects();
        }
        else
        {
            sortedList = trackObjects;
        }
        for (int i = 0; i < MAX_OBJECTS; i++)
        {
            if (sortedList.Count <= i || sortedList [i] == null)
            {
                m_values [i] = Vector4.zero;
            }
            else
            {
                m_values [i] = sortedList [i].GetVector();
            }
        }
    }
    private void LateUpdate()
    {
        UpdateShaderArray();
    }
    private void CheckClosestObjects()
    {
        Vector3 camPos = Camera.main.transform.position;
        sortedList = trackObjects.OrderBy(o => (Vector3.Distance(o.transform.position, camPos) - o.radius).Clamp(0, Mathf.Infinity)).ToList();
    }

    public void UpdateShaderArray()
    {
        Shader.SetGlobalVectorArray("DistortionbObjects", m_values);
    }

    public void AddObject(GrassObject obj)
    {
        if (trackObjects.Contains(obj) == false) trackObjects.Add(obj);
    }
    public void RemoveObject(GrassObject obj)
    {
        if (trackObjects.Contains(obj)) trackObjects.Remove(obj);
    }

}
