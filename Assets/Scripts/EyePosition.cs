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
	public float openness = 0.95f;
	public Color outerColor = Color.white;
	public Color innerColor = Color.blue;
	public Color pupilColor = Color.black;
	public float outerSize = 0.5f;
	public float innerSize = 0.3f;
	public float pupilSize = 0.2f;
	public float layerDepth = 1.1f;
	public float eyeLaziness;

	public float distanceMultiplier = 1.0f;
	public bool refreshMaterial;

	public bool autoBlink = true;
	public float manualBlinkValue;
	public float blinkSpeed = 5.0f;
	public float blinkInterval = 10.0f;
	public float blinkChance = 0.5f;
	private bool m_blinking;

	private int m_blinkPropertyId = 1;
	private float m_blinkTimer;

	private int m_innerColorPropertyId = 4;

	private int m_innerSizePropertyId = 7;

	private int m_layerDepthPropertyId = 9;

	private int m_lazinessPropertyId = 10;
	private Material m_mat;
	private int m_openPropertyId = 2;

	private int m_outerColorPropertyId = 3;

	private int m_outerSizePropertyId = 6;

	private int m_positionPropertyId;
	private float m_prevEyeLaziness = -1.0f;
	private Color m_prevInnerColor = Color.white;
	private float m_prevInnerSize = -1.0f;
	private float m_prevLayerDepth = -1.0f;
	private float m_prevOpenness = -1.0f;
	private Color m_prevOuterColor = Color.black;
	private float m_prevOuterSize = -1.0f;
	private Color m_prevPupilColor = Color.white;
	private float m_prevPupilSize = -1.0f;

	private int m_pupilColorPropertyId = 5;

	private int m_pupilSizePropertyId = 8;

	private void Start()
	{
		if (renderers.Count < 1) return;
		ConfigMaterial();
	}

	private void Update()
	{
		if (refreshMaterial) ConfigMaterial();

		var pos = position;

		renderers.ForEach(r =>
		{
			var t = r.transform;

			if (followTransform != null)
			{
				if (targetObject != null) followTransform.position = targetObject.position;
				var x = followTransform.position.z - t.position.z;
				var y = followTransform.position.y - t.position.y;
				pos.x = x;
				pos.y = y;
			}

			if (pos.magnitude > maxMagnitude) pos = pos.normalized * maxMagnitude;

			if (invertX) pos.x *= -1;
			if (invertY) pos.y *= -1;

			if (Application.isPlaying) r.material.SetVector(m_positionPropertyId, pos);
		});

		m_blinkTimer += Time.deltaTime;
		if (m_blinkTimer >= blinkInterval && autoBlink)
		{
			m_blinkTimer = 0.0f;
			if (blinkChance >= Random.Range(0.0f, 1.0f) && !m_blinking) StartCoroutine(Blink());
		}

		if (!autoBlink) ManualBlink();

		CheckValues(false);
	}

	private void ConfigMaterial()
	{
		refreshMaterial = false;
		if (EyeMaterial == null) return;
		m_mat = new Material(EyeMaterial);
		SetPropertyIds();
		renderers.ForEach(r => r.material = new Material(m_mat));
		CheckValues(true);
	}

	private void SetPropertyIds()
	{
		m_positionPropertyId   = Shader.PropertyToID("_EyePosition");
		m_blinkPropertyId      = Shader.PropertyToID("_Blink");
		m_openPropertyId       = Shader.PropertyToID("_Open");
		m_outerColorPropertyId = Shader.PropertyToID("_OuterColor");
		m_innerColorPropertyId = Shader.PropertyToID("_InnerColor");
		m_pupilColorPropertyId = Shader.PropertyToID("_PupilColor");
		m_pupilSizePropertyId  = Shader.PropertyToID("_PupilSize");
		m_innerSizePropertyId  = Shader.PropertyToID("_InnerSize");
		m_outerSizePropertyId  = Shader.PropertyToID("_OuterSize");
		m_layerDepthPropertyId = Shader.PropertyToID("_LayerDepth");
		m_lazinessPropertyId   = Shader.PropertyToID("_Laziness");
	}

	private void CheckValues(bool _overide)
	{
		if (!Application.isPlaying) return;

		if (openness != m_prevOpenness || _overide)
		{
			m_prevOpenness = openness;
			renderers.ForEach(r => r.material.SetFloat(m_openPropertyId, openness));
		}

		if (m_prevPupilSize != pupilSize || _overide)
		{
			m_prevPupilSize = pupilSize;
			renderers.ForEach(r => r.material.SetFloat(m_pupilSizePropertyId, pupilSize));
		}

		if (m_prevInnerSize != innerSize || _overide)
		{
			m_prevInnerSize = innerSize;
			renderers.ForEach(r => r.material.SetFloat(m_innerSizePropertyId, innerSize));
		}

		if (m_prevOuterSize != outerSize || _overide)
		{
			m_prevOuterSize = outerSize;
			renderers.ForEach(r => r.material.SetFloat(m_outerSizePropertyId, outerSize));
		}

		if (pupilColor != m_prevPupilColor || _overide)
		{
			m_prevPupilColor = pupilColor;
			renderers.ForEach(r => r.material.SetColor(m_pupilColorPropertyId, pupilColor));
		}

		if (innerColor != m_prevInnerColor || _overide)
		{
			m_prevInnerColor = innerColor;
			renderers.ForEach(r => r.material.SetColor(m_innerColorPropertyId, innerColor));
		}

		if (outerColor != m_prevOuterColor || _overide)
		{
			m_prevOuterColor = outerColor;
			renderers.ForEach(r => r.material.SetColor(m_outerColorPropertyId, outerColor));
		}

		if (layerDepth != m_prevLayerDepth || _overide)
		{
			m_prevLayerDepth = layerDepth;
			renderers.ForEach(r => r.material.SetFloat(m_layerDepthPropertyId, layerDepth));
		}

		if (m_prevEyeLaziness != eyeLaziness || _overide)
		{
			m_prevEyeLaziness = eyeLaziness;
			renderers.ForEach(r => r.material.SetFloat(m_lazinessPropertyId, eyeLaziness));
		}
	}

	private void ManualBlink()
	{
		if (Application.isPlaying)
			renderers.ForEach(r => r.material.SetInt(m_blinkPropertyId, (manualBlinkValue * 100).RoundToInt()));
	}

	private IEnumerator Blink()
	{
		m_blinking = true;

		var t = 0.0f;
		while (t < 1.0f)
		{
			if (Application.isPlaying)
				renderers.ForEach(r => r.material.SetInt(m_blinkPropertyId, (t * 100).RoundToInt()));
			t += Time.deltaTime * blinkSpeed;
			yield return null;
		}

		t = 1.0f;
		while (t > 0.0f)
		{
			if (Application.isPlaying)
				renderers.ForEach(r => r.material.SetInt(m_blinkPropertyId, (t * 100).RoundToInt()));
			t -= Time.deltaTime * blinkSpeed;
			yield return null;
		}

		if (Application.isPlaying) renderers.ForEach(r => r.material.SetInt(m_blinkPropertyId, 0));

		m_blinking = false;

		yield return null;
	}
}