using System.Runtime.InteropServices;
using UnityEngine;

[ExecuteInEditMode]
public class ComputeFlowers : MonoBehaviour
{
	private const int SOURCE_VERT_STRIDE = sizeof(float) * 3;
	private const int SOURCE_TRI_STRIDE = sizeof(int);
	private const int DRAW_STRIDE = sizeof(float) * (3 + (3 + 1) * 3);
	private const int INDIRECT_ARGS_STRIDE = sizeof(int) * 4;

	//	Shader Property IDs
	private static readonly int m_sourceVerticesPropertyId = Shader.PropertyToID("_SourceVertices");
	private static readonly int m_sourceTrianglesPropertyId = Shader.PropertyToID("_SourceTriangles");
	private static readonly int m_drawTrianglesPropertyId = Shader.PropertyToID("_DrawTriangles");
	private static readonly int m_indirectArgsBufferPropertyId = Shader.PropertyToID("_IndirectArgsBuffer");
	private static readonly int m_numSourceTrianglesPropertyId = Shader.PropertyToID("_NumSourceTriangles");
	private static readonly int m_worldSpaceCameraPosPropertyId = Shader.PropertyToID("_WorldSpaceCameraPos");
	private static readonly int m_worldSpaceCameraForwardPropertyId = Shader.PropertyToID("_WorldSpaceCameraForward");
	private static readonly int m_localToWorldPropertyId = Shader.PropertyToID("_LocalToWorld");

	[Header("Core Settings")]
	[SerializeField] private Mesh m_sourceMesh;

	[SerializeField] private ComputeShader m_computeShader;
	[SerializeField] private Material m_material;
	private ComputeBuffer m_argsBuffer = default;
	private ComputeBuffer m_drawBuffer = default;

	//	Important Compute Buffer Values
	private bool m_initialized;
	private int m_kernelId;
	private ComputeBuffer m_sourceTriBuffer = default;
	private ComputeBuffer m_sourceVertBuffer = default;
	private int m_dispatchSize;
	private Bounds m_localBounds;

	private void OnEnable()
	{
		if (m_initialized) DeInitialize();
		Initialize();
	}

	private void OnDisable()
	{
		if (m_initialized) DeInitialize();
	}

	private void Initialize()
	{
		Debug.Assert(m_computeShader == null, "Compute Shader Is Missing Or Invalid!",  gameObject);
		Debug.Assert(m_material == null,      "Flower Material Is Missing Or Invalid!", gameObject);
		Debug.Assert(m_sourceMesh == null,    "Source Mesh Is Missing Or Invalid!",     gameObject);

		var meshVertPositions = m_sourceMesh.vertices;
		var meshTris = m_sourceMesh.triangles;

		var meshVertices = new SourceVertex[meshVertPositions.Length];
		for (var i = 0; i < meshVertices.Length; i++)
		{
			meshVertices[i] = new SourceVertex {position = meshVertPositions[i]};
		}

		var numSourceTriangles = meshTris.Length / 3;

		m_sourceVertBuffer = new ComputeBuffer(meshVertices.Length, SOURCE_VERT_STRIDE, ComputeBufferType.Structured,
			ComputeBufferMode.Immutable);
		m_sourceVertBuffer.SetData(meshVertices);

		m_sourceTriBuffer = new ComputeBuffer(meshTris.Length, SOURCE_TRI_STRIDE, ComputeBufferType.Structured,
			ComputeBufferMode.Immutable);
		m_sourceTriBuffer.SetData(meshTris);

		m_drawBuffer = new ComputeBuffer(numSourceTriangles, DRAW_STRIDE, ComputeBufferType.Append);

		m_argsBuffer = new ComputeBuffer(1, INDIRECT_ARGS_STRIDE, ComputeBufferType.IndirectArguments);

		m_kernelId = m_computeShader.FindKernel("Main");

		m_computeShader.SetBuffer(m_kernelId, m_sourceVerticesPropertyId,     m_sourceVertBuffer);
		m_computeShader.SetBuffer(m_kernelId, m_sourceTrianglesPropertyId,    m_sourceTriBuffer);
		m_computeShader.SetBuffer(m_kernelId, m_drawTrianglesPropertyId,      m_drawBuffer);
		m_computeShader.SetBuffer(m_kernelId, m_indirectArgsBufferPropertyId, m_argsBuffer);
		m_computeShader.SetInt(m_numSourceTrianglesPropertyId, numSourceTriangles);

		m_material.SetBuffer(m_drawTrianglesPropertyId, m_drawBuffer);

		m_computeShader.GetKernelThreadGroupSizes(m_kernelId, out var threadGroupSize, out _, out _);
		m_dispatchSize = Mathf.CeilToInt((float) numSourceTriangles / threadGroupSize);

		m_localBounds = m_sourceMesh.bounds;
		m_localBounds.Expand(1);
	}

	private void DeInitialize()
	{
		m_argsBuffer.Release();
		m_drawBuffer.Release();
		m_sourceTriBuffer.Release();
		m_sourceVertBuffer.Release();
		m_initialized = false;
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct SourceVertex
	{
		public Vector3 position;
	}
}