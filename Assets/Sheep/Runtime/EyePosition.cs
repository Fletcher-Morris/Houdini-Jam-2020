using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class EyePosition : MonoBehaviour
{
    [System.Serializable]
    public struct EyeProperties
    {
        public float EyeLaziness;
        public Color InnerColor;
        public float InnerSize;
        public float LayerDepth;
        public float Openness;
        public Color OuterColor;
        public float OuterSize;
        public Color PupilColor;
        public float PupilSize;

        public EyeProperties Lerp(EyeProperties a, EyeProperties b, float t)
        {
            EyeProperties result = a;
            result.EyeLaziness = Mathf.Lerp(a.EyeLaziness, b.EyeLaziness, t);
            result.InnerColor = Color.Lerp(a.InnerColor, b.InnerColor, t);
            result.InnerSize = Mathf.Lerp(a.InnerSize, b.InnerSize, t);
            result.LayerDepth = Mathf.Lerp(a.LayerDepth, b.LayerDepth, t);
            result.Openness = Mathf.Lerp(a.Openness, b.Openness, t);
            result.OuterColor = Color.Lerp(a.OuterColor, b.OuterColor, t);
            result.OuterSize = Mathf.Lerp(a.OuterSize, b.OuterSize, t);
            result.PupilColor = Color.Lerp(a.PupilColor, b.PupilColor, t);
            result.PupilSize = Mathf.Lerp(a.PupilSize, b.PupilSize, t);
            return result;
        }

        public EyeProperties(bool x)
        {
            EyeLaziness = 0;
            InnerColor = Color.blue;
            InnerSize = 0.3f;
            LayerDepth = 1.1f;
            Openness = 0.05f;
            OuterColor = Color.white;
            OuterSize = 0.5f;
            PupilColor = Color.black;
            PupilSize = 0.2f;
        }
    }

    [SerializeField] private Transform followTransform;
    [SerializeField] private Transform targetObject;
    [SerializeField] private Transform _eyesCenter;
    [SerializeField] private float maxMagnitude = 0.333f;
    [SerializeField] private Renderer _renderer1;
    [SerializeField] private Renderer _renderer2;
    [SerializeField] private Vector2 position;
    [SerializeField] private Material EyeMaterial;
    private Material m_mat;
    [SerializeField] private bool invertX;
    [SerializeField] private bool invertY = true;

    [SerializeField] private EyeProperties _eyeProperties = new EyeProperties(true);
    private EyeProperties _prevEyeProperties = new EyeProperties();

    public float distanceMultiplier = 1.0f;
    public bool refreshMaterial;

    public bool autoBlink = true;
    public float manualBlinkValue;
    public float blinkSpeed = 5.0f;
    public float blinkInterval = 10.0f;
    public float blinkChance = 0.5f;
    private float _blinkTimer = 0;
    private bool m_blinking;

    private static int BlinkPropHash = Shader.PropertyToID("_Blink");
    private static int InnerColorPropHash = Shader.PropertyToID("_InnerColor");
    private static int InnerSizePropHash = Shader.PropertyToID("_InnerSize");
    private static int LayerDepthPropHash = Shader.PropertyToID("_LayerDepth");
    private static int LazinessPropHash = Shader.PropertyToID("_Laziness");
    private static int OpenPropHash = Shader.PropertyToID("_Open");
    private static int OuterColorPropHash = Shader.PropertyToID("_OuterColor");
    private static int OuterSizePropHash = Shader.PropertyToID("_OuterSize");
    private static int PupilColorPropHash = Shader.PropertyToID("_PupilColor");
    private static int PupilSizePropHash = Shader.PropertyToID("_PupilSize");
    private static int EyePositionPropHash = Shader.PropertyToID("_EyePosition");

    private void Start()
    {
        if (_renderer1 == null || _renderer2 == null) return;
        ConfigMaterial();
    }

    private void Update()
    {
        if (Application.isPlaying) return;
        UpdateEyes(Time.deltaTime);
    }

    public void SetLookTarget(Transform target)
    {
        targetObject = target;
    }

    public void SetLookTarget(Vector3 target)
    {
        targetObject = null;
        followTransform.position = target;
    }

    public void UpdateEyes(float delta)
    {
        if (refreshMaterial) ConfigMaterial();

        CheckValues(false);

        if (_renderer1 == null || _renderer2 == null) return;
        UpdateRenderer(_renderer1, position, delta);
        UpdateRenderer(_renderer2, position, delta);

        _blinkTimer += delta;
        if (_blinkTimer >= blinkInterval && autoBlink)
        {
            _blinkTimer = 0.0f;
            if (blinkChance >= Random.Range(0.0f, 1.0f) && !m_blinking) StartCoroutine(Blink());
        }

        if (!autoBlink) ManualBlink();

    }

    private void UpdateRenderer(Renderer r, Vector3 pos, float delta)
    {
        if (followTransform != null)
        {
            if (targetObject != null) followTransform.position = targetObject.position;
            _eyesCenter.position = Vector3.Lerp(_renderer1.transform.position, _renderer2.transform.position, 0.5f);

            float angleZ = Vector2.SignedAngle(new Vector2(0, _eyesCenter.localPosition.z), new Vector2(followTransform.localPosition.x, followTransform.localPosition.z)).Abs();
            float angleY = Vector2.SignedAngle(new Vector2(0, _eyesCenter.localPosition.y), new Vector2(followTransform.localPosition.x, followTransform.localPosition.y)).Abs();

            pos.x = Mathf.Lerp(-1, 1, angleZ / 180.0f);
            pos.y = Mathf.Lerp(-1, 1, angleY / 180.0f);
        }

        if (pos.magnitude > maxMagnitude) pos = pos.normalized * maxMagnitude;

        if (invertX) pos.x *= -1;
        if (invertY) pos.y *= -1;

        if (Application.isPlaying) r.material.SetVector(EyePositionPropHash, pos);
    }

    private void ConfigMaterial()
    {
        refreshMaterial = false;
        if (EyeMaterial == null) return;
        m_mat = new Material(EyeMaterial);
        _renderer1.material = new Material(m_mat);
        _renderer2.material = new Material(m_mat);
        CheckValues(true);
    }

    private void CheckValues(bool _overide)
    {
        if (!Application.isPlaying) return;

        if (_eyeProperties.Openness != _prevEyeProperties.Openness || _overide)
        {
            _renderer1.material.SetFloat(OpenPropHash, _eyeProperties.Openness);
            _renderer2.material.SetFloat(OpenPropHash, _eyeProperties.Openness);
        }

        if (_eyeProperties.PupilSize != _prevEyeProperties.PupilSize|| _overide)
        {
            _renderer1.material.SetFloat(PupilSizePropHash, _eyeProperties.PupilSize);
            _renderer2.material.SetFloat(PupilSizePropHash, _eyeProperties.PupilSize);
        }

        if (_eyeProperties.InnerSize != _prevEyeProperties.InnerSize|| _overide)
        {
            _renderer1.material.SetFloat(InnerSizePropHash, _eyeProperties.InnerSize);
            _renderer2.material.SetFloat(InnerSizePropHash, _eyeProperties.InnerSize);
        }

        if (_eyeProperties.OuterSize != _prevEyeProperties.OuterSize|| _overide)
        {
            _renderer1.material.SetFloat(OuterSizePropHash, _eyeProperties.OuterSize);
            _renderer2.material.SetFloat(OuterSizePropHash, _eyeProperties.OuterSize);
        }

        if (_eyeProperties.PupilColor != _prevEyeProperties.PupilColor || _overide)
        {
            _renderer1.material.SetColor(PupilColorPropHash, _eyeProperties.PupilColor);
            _renderer2.material.SetColor(PupilColorPropHash, _eyeProperties.PupilColor);
        }

        if (_eyeProperties.InnerColor != _prevEyeProperties.InnerColor || _overide)
        {
            _renderer1.material.SetColor(InnerColorPropHash, _eyeProperties.InnerColor);
            _renderer2.material.SetColor(InnerColorPropHash, _eyeProperties.InnerColor);
        }

        if (_eyeProperties.OuterColor != _prevEyeProperties.OuterColor|| _overide)
        {
            _renderer1.material.SetColor(OuterColorPropHash, _eyeProperties.OuterColor);
            _renderer2.material.SetColor(OuterColorPropHash, _eyeProperties.OuterColor);
        }

        if (_eyeProperties.LayerDepth != _prevEyeProperties.LayerDepth|| _overide)
        {
            _renderer1.material.SetFloat(LayerDepthPropHash, _eyeProperties.LayerDepth);
            _renderer2.material.SetFloat(LayerDepthPropHash, _eyeProperties.LayerDepth);
        }

        if (_eyeProperties.EyeLaziness != _prevEyeProperties.EyeLaziness || _overide)
        {
            _renderer1.material.SetFloat(LazinessPropHash, _eyeProperties.EyeLaziness);
            _renderer2.material.SetFloat(LazinessPropHash, _eyeProperties.EyeLaziness);
        }

        _prevEyeProperties = _eyeProperties;
    }

    private void ManualBlink()
    {
        if (Application.isPlaying)
        {
            _renderer1.material.SetInt(BlinkPropHash, (manualBlinkValue * 100).RoundToInt());
            _renderer2.material.SetInt(BlinkPropHash, (manualBlinkValue * 100).RoundToInt());
        }
    }

    private IEnumerator Blink()
    {
        m_blinking = true;

        float t = 0.0f;
        while (t < 1.0f)
        {
            if (Application.isPlaying)
            {
                _renderer1.material.SetInt(BlinkPropHash, (t * 100).RoundToInt());
                _renderer2.material.SetInt(BlinkPropHash, (t * 100).RoundToInt());
            }

            t += Time.deltaTime * blinkSpeed;
            yield return null;
        }

        t = 1.0f;
        while (t > 0.0f)
        {
            if (Application.isPlaying)
            {
                _renderer1.material.SetInt(BlinkPropHash, (t * 100).RoundToInt());
                _renderer2.material.SetInt(BlinkPropHash, (t * 100).RoundToInt());
            }

            t -= Time.deltaTime * blinkSpeed;
            yield return null;
        }

        if (Application.isPlaying)
        {
            _renderer1.material.SetInt(BlinkPropHash, 0);
            _renderer2.material.SetInt(BlinkPropHash, 0);
        }

        m_blinking = false;

        yield return null;
    }
}