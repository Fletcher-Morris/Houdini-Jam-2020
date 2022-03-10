using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

[ExecuteInEditMode]
public class GrassComputeController : MonoBehaviour
{
	private const int SOURCE_VERT_STRIDE = sizeof(float) * 3;
	private const int SOURCE_TRI_STRIDE = sizeof(int);
	private const int DRAW_STRIDE = sizeof(float) * (3 + (3 + 1) * 3);
	private const int INDIRECT_ARGS_STRIDE = sizeof(int) * 4;

	private static readonly int m_sourceVerticesPropertyId = Shader.PropertyToID("_SourceVertices");
	private static readonly int m_sourceTrianglesPropertyId = Shader.PropertyToID("_SourceTriangles");
	private static readonly int m_drawTrianglesPropertyId = Shader.PropertyToID("_DrawTriangles");
	private static readonly int m_indirectArgsBufferPropertyId = Shader.PropertyToID("_IndirectArgsBuffer");
	private static readonly int m_numSourceTrianglesPropertyId = Shader.PropertyToID("_NumSourceTriangles");
	private static readonly int m_worldSpaceCameraPosPropertyId = Shader.PropertyToID("_WorldSpaceCameraPos");
	private static readonly int m_worldSpaceCameraForwardPropertyId = Shader.PropertyToID("_WorldSpaceCameraForward");
	private static readonly int m_localToWorldPropertyId = Shader.PropertyToID("_LocalToWorld");

	[SerializeField] private Mesh sourceMesh;
	[SerializeField] private ComputeShader m_compute;
	[SerializeField] private Material material;
	[SerializeField] private Transform m_cameraTransform;

	public ComputeGrassSettings grassSettings;
	private readonly int[] argsBufferReset = {0, 1, 0, 0};
	private ComputeBuffer argsBuffer;
	private int dispatchSize;
	private ComputeBuffer drawBuffer;
	private bool initialized;
	private Bounds localBounds;
	[SerializeField] [Range(0,1)] private float m_grassFill = 1.0f;
	private int m_grassKernelId;
	private ComputeGrassSettingsData m_prevGrassSettings;
	private ComputeBuffer sourceTriBuffer;
	private ComputeBuffer sourceVertBuffer;

	private void LateUpdate()
	{
		return;
		if (Application.isPlaying == false)
		{
			OnDisable();
			OnEnable();
		}

		drawBuffer.SetCounterValue(0);
		argsBuffer.SetData(argsBufferReset);

		if (m_prevGrassSettings.Checksum != grassSettings.SettingsData.Checksum) SubmitGrassSettings();
		if (m_cameraTransform is null)
		{
			m_compute.SetVector(m_worldSpaceCameraPosPropertyId,     Vector4.zero);
			m_compute.SetVector(m_worldSpaceCameraForwardPropertyId, Vector4.one);
		}
		else
		{
			m_compute.SetVector(m_worldSpaceCameraPosPropertyId,     m_cameraTransform.position);
			m_compute.SetVector(m_worldSpaceCameraForwardPropertyId, m_cameraTransform.forward);
		}

		var bounds = TransformBounds(localBounds);

		m_compute.SetMatrix(m_localToWorldPropertyId, transform.localToWorldMatrix);

		m_compute.Dispatch(m_grassKernelId, dispatchSize, 1, 1);
		Graphics.DrawProceduralIndirect(material, bounds, MeshTopology.Triangles, argsBuffer, 0,
			null, null, ShadowCastingMode.On, true, gameObject.layer);
	}

	private void OnEnable()
	{
		return;
		Debug.Assert(m_compute is null, "The grass compute shader is null", gameObject);
		Debug.Assert(material is null,  "The material is null",             gameObject);

		DebugHardware();

		if (initialized) OnDisable();
		initialized = true;

		var positions = sourceMesh.vertices;
		var tris = sourceMesh.triangles;

		var vertices = new SourceVertex[positions.Length];
		for (var i = 0; i < vertices.Length; i++)
		{
			vertices[i] = new SourceVertex
			{
				position = positions[i]
			};
		}

		var numSourceTriangles = tris.Length / 3;

		sourceVertBuffer = new ComputeBuffer(vertices.Length, SOURCE_VERT_STRIDE, ComputeBufferType.Structured,
			ComputeBufferMode.Immutable);
		sourceVertBuffer.SetData(vertices);
		sourceTriBuffer = new ComputeBuffer(tris.Length, SOURCE_TRI_STRIDE, ComputeBufferType.Structured,
			ComputeBufferMode.Immutable);
		sourceTriBuffer.SetData(tris);
		drawBuffer =
			new ComputeBuffer(numSourceTriangles * Mathf.CeilToInt(grassSettings.SettingsData.grassPerVertex * 0.25f),
				DRAW_STRIDE, ComputeBufferType.Append);
		drawBuffer.SetCounterValue(0);
		argsBuffer      = new ComputeBuffer(1, INDIRECT_ARGS_STRIDE, ComputeBufferType.IndirectArguments);
		m_grassKernelId = m_compute.FindKernel("Main");

		m_compute.SetBuffer(m_grassKernelId, m_sourceVerticesPropertyId,     sourceVertBuffer);
		m_compute.SetBuffer(m_grassKernelId, m_sourceTrianglesPropertyId,    sourceTriBuffer);
		m_compute.SetBuffer(m_grassKernelId, m_drawTrianglesPropertyId,      drawBuffer);
		m_compute.SetBuffer(m_grassKernelId, m_indirectArgsBufferPropertyId, argsBuffer);
		m_compute.SetInt(m_numSourceTrianglesPropertyId, numSourceTriangles);

		material.SetBuffer(m_drawTrianglesPropertyId, drawBuffer);

		m_compute.GetKernelThreadGroupSizes(m_grassKernelId, out var threadGroupSize, out _, out _);
		dispatchSize = Mathf.CeilToInt((float) numSourceTriangles / threadGroupSize);

		localBounds = sourceMesh.bounds;
		localBounds.Expand(1);
	}

	private void OnDisable()
	{
		if (initialized)
		{
			sourceVertBuffer.Release();
			sourceTriBuffer.Release();
			drawBuffer.Release();
			argsBuffer.Release();
		}

		initialized = false;
	}

	private void DebugHardware()
	{
		Debug.Log($"Max Compute Buffer Workgroup Size : {SystemInfo.maxComputeWorkGroupSize}");
	}

	public Bounds TransformBounds(Bounds boundsOS)
	{
		var center = transform.TransformPoint(boundsOS.center);
		var extents = boundsOS.extents;
		var axisX = transform.TransformVector(extents.x, 0,         0);
		var axisY = transform.TransformVector(0,         extents.y, 0);
		var axisZ = transform.TransformVector(0,         0,         extents.z);
		extents.x = Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x);
		extents.y = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
		extents.z = Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z);
		return new Bounds {center = center, extents = extents};
	}

	public void SetGrassFill(Slider _slider)
	{
		m_grassFill = _slider.value;
		SubmitGrassSettings();
	}

	private void SubmitGrassSettings()
	{
		m_prevGrassSettings = grassSettings.SettingsData;

		m_compute.SetFloat("_GrassHeight",        grassSettings.SettingsData.grassHeight);
		m_compute.SetFloat("_GrassHeightRandom",  grassSettings.SettingsData.grassHeightRandom);
		m_compute.SetFloat("_GrassHeightCuttoff", grassSettings.SettingsData.grassHeightCuttoff);
		m_compute.SetFloat("_GrassWidth",         grassSettings.SettingsData.grassWidth);
		m_compute.SetFloat("_GrassWidthRandom",   grassSettings.SettingsData.grassWidthRandom);
		m_compute.SetFloat("_BendRandom",   grassSettings.SettingsData.grassBendRandom);
		m_compute.SetInt("_GrassSegments",  grassSettings.SettingsData.grassSegments);
		m_compute.SetInt("_GrassPerVertex", Mathf.RoundToInt(grassSettings.SettingsData.grassPerVertex * m_grassFill));
		m_compute.SetFloat("_RandomPosition",     grassSettings.SettingsData.randomPosition);
		m_compute.SetFloat("_MinCamDist",         grassSettings.SettingsData.minCamDist);
		m_compute.SetFloat("_MaxCameraDist",      Mathf.Max(grassSettings.SettingsData.maxCameraDist, 0.1f));
		m_compute.SetFloat("_MinAltitude",        grassSettings.SettingsData.minAltitude);
		m_compute.SetFloat("_MaxAltitude",        grassSettings.SettingsData.maxAltitude);
		m_compute.SetFloat("_AltitudeHeightFade", grassSettings.SettingsData.altitudeFade);
		m_compute.SetFloat("_CameraDotCuttoff",   grassSettings.SettingsData.camDotCuttoff);
		m_compute.SetFloat("_AvPlanetRadius",     grassSettings.SettingsData.averagePlanetRadius);
		Debug.Log("Submitting Compute Grass Settings!");
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct SourceVertex
	{
		public Vector3 position;
	}
}