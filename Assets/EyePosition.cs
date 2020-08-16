using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyePosition : MonoBehaviour
{
    public Transform followTransform;
    public List<Renderer> renderers = new List<Renderer>();
    public Vector2 position;
    public bool invert;

    private List<Material> m_materials = new List<Material>();
    private int propertyId;
    void Start()
    {
        renderers.ForEach(r => m_materials.Add(r.material));
        if (m_materials.Count < 1) return;
        propertyId = m_materials[0].shader.GetPropertyNameId(0);
    }

    void Update()
    {
        Vector2 pos = position;
        if (followTransform != null) { pos = followTransform.localPosition; }
        if (invert) pos = -pos;
        m_materials.ForEach(m => m.SetVector(propertyId, pos));
    }
}
