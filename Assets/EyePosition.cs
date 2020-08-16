using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyePosition : MonoBehaviour
{
    public Transform followTransform;
    public Transform targetObject;
    public float maxMagnitude = 0.333f;
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
        blinkPropertyId = m_materials[0].shader.GetPropertyNameId(1);
    }

    void Update()
    {
        Vector2 pos = position;
        if (followTransform != null)
        {
            if(targetObject != null)
            {
                followTransform.position = targetObject.position;
            }
            pos = followTransform.localPosition;
            if(pos.magnitude > maxMagnitude)
            {
                pos = pos.normalized * maxMagnitude;
            }
        }
        if (invert) pos = -pos;
        m_materials.ForEach(m => m.SetVector(propertyId, pos));

        m_blinkTimer += Time.deltaTime;
        if(m_blinkTimer >= blinkInterval)
        {
            m_blinkTimer = 0.0f;
            if(blinkChance >= Random.Range(0.0f, 1.0f) && !m_blinking)
            {
                Debug.Log("BLINK!");
                StartCoroutine(Blink());
            }
        }
    }

    private int blinkPropertyId;
    private bool m_blinking;
    public float blinkSpeed = 5.0f;
    public float blinkInterval = 10.0f;
    public float blinkChance = 0.5f;
    private float m_blinkTimer = 0.0f;
    private IEnumerator Blink()
    {
        m_blinking = true;

        float t = 0.0f;
        while(t < 1.0f)
        {
            m_materials.ForEach(m => m.SetInt(blinkPropertyId, (t * 100).RoundToInt()));
            t += Time.deltaTime * blinkSpeed;
            yield return null;
        }
        t = 1.0f;
        while (t > 0.0f)
        {
            m_materials.ForEach(m => m.SetInt(blinkPropertyId, (t * 100).RoundToInt()));
            t -= Time.deltaTime * blinkSpeed;
            yield return null;
        }
        m_materials.ForEach(m => m.SetInt(blinkPropertyId, 0));

        m_blinking = false;

        yield return null;
    }
}
