using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class EyePosition : MonoBehaviour
{
    public Transform followTransform;
    public Transform targetObject;
    public float maxMagnitude = 0.333f;
    public List<Renderer> renderers = new List<Renderer>();
    public Vector2 position;
    public bool invertX;
    public bool invertY = true;

    public Material EyeMaterial;
    private Material m_mat;
    private int propertyId;
    void Start()
    {
        if (renderers.Count < 1) return;
        ConfigMaterial();
    }

    void ConfigMaterial()
    {
        refreshMaterial = false;
        if (EyeMaterial == null) return;
        m_mat = new Material(EyeMaterial);
        propertyId = m_mat.shader.GetPropertyNameId(0);
        blinkPropertyId = m_mat.shader.GetPropertyNameId(1);
        renderers.ForEach(r => r.material = new Material(m_mat));
    }

    public float distanceMultiplier = 1.0f;
    public bool refreshMaterial = false;
    void Update()
    {
        if (refreshMaterial) ConfigMaterial();

        Vector2 pos = position;

        renderers.ForEach(r =>
        {
            Transform t = r.transform;

            if (followTransform != null)
            {
                if (targetObject != null)
                {
                    followTransform.position = targetObject.position;
                }
                float x = followTransform.position.z - t.position.z;
                float y = followTransform.position.y - t.position.y;
                pos.x = x;
                pos.y = y;
            }

            if (pos.magnitude > maxMagnitude) pos = pos.normalized * maxMagnitude;


            if (invertX) pos.x *= -1;
            if (invertY) pos.y *= -1;

            r.material.SetVector(propertyId, pos);

        });


        m_blinkTimer += Time.deltaTime;
        if (m_blinkTimer >= blinkInterval)
        {
            m_blinkTimer = 0.0f;
            if (blinkChance >= Random.Range(0.0f, 1.0f) && !m_blinking)
            {
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
            renderers.ForEach(r => r.material.SetInt(blinkPropertyId, (t * 100).RoundToInt()));
            t += Time.deltaTime * blinkSpeed;
            yield return null;
        }
        t = 1.0f;
        while (t > 0.0f)
        {
            renderers.ForEach(r => r.material.SetInt(blinkPropertyId, (t * 100).RoundToInt()));
            t -= Time.deltaTime * blinkSpeed;
            yield return null;
        }
        renderers.ForEach(r => r.material.SetInt(blinkPropertyId, 0));

        m_blinking = false;

        yield return null;
    }
}
