using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class EyePosition : MonoBehaviour
{
    [System.Serializable]
    public struct EyeProperties
    {
        [System.Serializable]
        public struct Colors
        {
            public Color PupilColor;
            public Color InnerColor;
            public Color OuterColor;
        }

        public Colors EyeColors;
        public float EyeLaziness;
        public float InnerSize;
        public float LayerDepth;
        public float Openness;
        public float OuterSize;
        public float PupilSize;

        public EyeProperties Lerp(EyeProperties a, EyeProperties b, float t, bool lerpColors)
        {
            EyeProperties result = a;
            result.EyeLaziness = Mathf.Lerp(a.EyeLaziness, b.EyeLaziness, t);
            result.InnerSize = Mathf.Lerp(a.InnerSize, b.InnerSize, t);
            result.LayerDepth = Mathf.Lerp(a.LayerDepth, b.LayerDepth, t);
            result.Openness = Mathf.Lerp(a.Openness, b.Openness, t);
            result.OuterSize = Mathf.Lerp(a.OuterSize, b.OuterSize, t);
            result.PupilSize = Mathf.Lerp(a.PupilSize, b.PupilSize, t);

            if (lerpColors)
            {
                result.EyeColors.PupilColor = Color.Lerp(a.EyeColors.PupilColor, b.EyeColors.PupilColor, t);
                result.EyeColors.InnerColor = Color.Lerp(a.EyeColors.InnerColor, b.EyeColors.InnerColor, t);
                result.EyeColors.OuterColor = Color.Lerp(a.EyeColors.OuterColor, b.EyeColors.OuterColor, t);
            }
            return result;
        }

        public EyeProperties(bool x)
        {
            EyeLaziness = 0;
            InnerSize = 0.3f;
            LayerDepth = 1.1f;
            Openness = 0.05f;
            OuterSize = 0.5f;
            PupilSize = 0.2f;
            EyeColors.PupilColor = Color.black;
            EyeColors.InnerColor = Color.blue;
            EyeColors.OuterColor = Color.white;
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

    public void ApplyAyePreset(EyePreset preset)
    {
        if (preset == null) return;
        StartCoroutine(LerpToEyeProperties(preset.Properties, 1.0f));
    }

    private bool _lerpingEyeProperties;
    private IEnumerator LerpToEyeProperties(EyeProperties newProperties, float lerpTime)
    {
        if(_lerpingEyeProperties == false)
        {
            _lerpingEyeProperties = true;
            EyeProperties prev = _eyeProperties;

            float t = 0;
            while(t < lerpTime)
            {
                _eyeProperties = _eyeProperties.Lerp(prev, newProperties, t / lerpTime, false);
                t += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            _eyeProperties = _eyeProperties.Lerp(newProperties, newProperties, 1, false);

            _lerpingEyeProperties = false;
        }
        yield return null;
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
        UpdateRenderer(position);

        _blinkTimer += delta;
        if (_blinkTimer >= blinkInterval && autoBlink)
        {
            _blinkTimer = 0.0f;
            if (blinkChance >= Random.Range(0.0f, 1.0f) && !m_blinking) StartCoroutine(Blink());
        }

        if (!autoBlink) ManualBlink();

    }

    private void UpdateRenderer(Vector3 targetPos)
    {
        if (followTransform != null)
        {
            if (targetObject != null) followTransform.position = targetObject.position;
            _eyesCenter.position = Vector3.Lerp(_renderer1.transform.position, _renderer2.transform.position, 0.5f);

            float angleZ = Vector2.SignedAngle(new Vector2(0, _eyesCenter.localPosition.z), new Vector2(followTransform.localPosition.x, followTransform.localPosition.z)).Abs();
            float angleY = Vector2.SignedAngle(new Vector2(0, _eyesCenter.localPosition.y), new Vector2(followTransform.localPosition.x, followTransform.localPosition.y)).Abs();

            targetPos.x = Mathf.Lerp(-1, 1, angleZ / 180.0f);
            targetPos.y = Mathf.Lerp(-1, 1, angleY / 180.0f);
        }

        if (targetPos.magnitude > maxMagnitude) targetPos = targetPos.normalized * maxMagnitude;

        if (invertX) targetPos.x *= -1;
        if (invertY) targetPos.y *= -1;

        if (Application.isPlaying) m_mat.SetVector(EyePositionPropHash, targetPos);
    }

    private void ConfigMaterial()
    {
        refreshMaterial = false;
        m_mat = new Material(EyeMaterial);
        _renderer1.material = m_mat;
        _renderer2.material = m_mat;
        CheckValues(true);
    }

    private void CheckValues(bool _overide)
    {
        if (!Application.isPlaying) return;

        if (_eyeProperties.Openness != _prevEyeProperties.Openness || _overide)
        {
            m_mat.SetFloat(OpenPropHash, _eyeProperties.Openness);
        }

        if (_eyeProperties.PupilSize != _prevEyeProperties.PupilSize|| _overide)
        {
            m_mat.SetFloat(PupilSizePropHash, _eyeProperties.PupilSize);
        }

        if (_eyeProperties.InnerSize != _prevEyeProperties.InnerSize|| _overide)
        {
            m_mat.SetFloat(InnerSizePropHash, _eyeProperties.InnerSize);
        }

        if (_eyeProperties.OuterSize != _prevEyeProperties.OuterSize|| _overide)
        {
            m_mat.SetFloat(OuterSizePropHash, _eyeProperties.OuterSize);
        }

        if (_eyeProperties.EyeColors.PupilColor != _prevEyeProperties.EyeColors.PupilColor || _overide)
        {
            m_mat.SetColor(PupilColorPropHash, _eyeProperties.EyeColors.PupilColor);
        }

        if (_eyeProperties.EyeColors.InnerColor != _prevEyeProperties.EyeColors.InnerColor || _overide)
        {
            m_mat.SetColor(InnerColorPropHash, _eyeProperties.EyeColors.InnerColor);
        }

        if (_eyeProperties.EyeColors.OuterColor != _prevEyeProperties.EyeColors.OuterColor || _overide)
        {
            m_mat.SetColor(OuterColorPropHash, _eyeProperties.EyeColors.OuterColor);
        }

        if (_eyeProperties.LayerDepth != _prevEyeProperties.LayerDepth|| _overide)
        {
            m_mat.SetFloat(LayerDepthPropHash, _eyeProperties.LayerDepth);
        }

        if (_eyeProperties.EyeLaziness != _prevEyeProperties.EyeLaziness || _overide)
        {
            m_mat.SetFloat(LazinessPropHash, _eyeProperties.EyeLaziness);
        }

        _prevEyeProperties = _eyeProperties;
    }

    private void ManualBlink()
    {
        if (Application.isPlaying)
        {
            m_mat.SetInt(BlinkPropHash, (manualBlinkValue * 100).RoundToInt());
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
                m_mat.SetInt(BlinkPropHash, (t * 100).RoundToInt());
            }

            t += Time.deltaTime * blinkSpeed;
            yield return null;
        }

        t = 1.0f;
        while (t > 0.0f)
        {
            if (Application.isPlaying)
            {
                m_mat.SetInt(BlinkPropHash, (t * 100).RoundToInt());
            }

            t -= Time.deltaTime * blinkSpeed;
            yield return null;
        }

        if (Application.isPlaying)
        {
            m_mat.SetInt(BlinkPropHash, 0);
        }

        m_blinking = false;

        yield return null;
    }
}