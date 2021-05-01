using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassComputeController : MonoBehaviour
{
    [Header("Critical Properties")]
    [SerializeField] private Mesh m_mesh = default;
    [SerializeField] private ComputeShader m_compute = default;
    [SerializeField] private Material m_mat = default;
    [Header("Grass Settings")]
    [SerializeField] private float m_grassHeight = 1.0f;

    private bool m_computeInitialised = default;
    private ComputeBuffer m_sourceVertBuffer = default;
    private ComputeBuffer m_sourceTriBuffer = default;
    private ComputeBuffer m_drawBuffer = default;
    private ComputeBuffer m_argsBuffer = default;
    private int m_kernelId = default;
    private int m_dispatchSize = default;
    private Bounds m_localBounds = default;
    private const int SOURCE_VERT_STRIDE = sizeof(float) * 3;
    private const int SOURCE_TRI_STRIDE = sizeof(int);
    private const int DRAW_STRIDE = sizeof(float) * (3 + (3 + 1) * 3);
    private const int INDIRECT_ARGS_STRIDE = sizeof(int) * 4;
    private int[] m_argsBufferReset = new int[] { 0, 1, 0, 0 };

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct SourceVertex
    {
        public Vector3 position;
    }

    private void OnEnable()
    {
        //  Initialise.
        Debug.Assert(m_compute != null, "The grass compute shader is null!", gameObject);
        Debug.Assert(m_mat != null, "The grass material is null!", gameObject);
        if (m_computeInitialised)
        {
            OnDisable();
        }
        m_computeInitialised = true;

        //  Get input mesh data.
        Vector3[] positions = m_mesh.vertices;
        int[] tris = m_mesh.triangles;
        SourceVertex[] verts = new SourceVertex[positions.Length];
        for (int v = 0; v < verts.Length; v++)
        {
            verts[v] = new SourceVertex() { position = positions[v] };
        }
        int sourceTriCount = tris.Length / 3;

        //  Create compute buffers.
        m_sourceVertBuffer = new ComputeBuffer(verts.Length,
            SOURCE_VERT_STRIDE,
            ComputeBufferType.Structured,
            ComputeBufferMode.Immutable);
        m_sourceVertBuffer.SetData(verts);
        m_sourceTriBuffer = new ComputeBuffer(
            tris.Length,
            SOURCE_TRI_STRIDE,
            ComputeBufferType.Structured,
            ComputeBufferMode.Immutable);
        m_sourceTriBuffer.SetData(tris);
        m_drawBuffer = new ComputeBuffer(sourceTriCount, DRAW_STRIDE, ComputeBufferType.Append);
        m_drawBuffer.SetCounterValue(0);
        m_argsBuffer = new ComputeBuffer(1, INDIRECT_ARGS_STRIDE, ComputeBufferType.IndirectArguments);

        m_kernelId = m_compute.FindKernel("Main");

        //  Set compute buffer contents.
        m_compute.SetBuffer(m_kernelId, "_SourceVertices", m_sourceVertBuffer);
        m_compute.SetBuffer(m_kernelId, "_SourceTrianges", m_sourceTriBuffer);
        m_compute.SetBuffer(m_kernelId, "_DrawTrianges", m_drawBuffer);
        m_compute.SetBuffer(m_kernelId, "_IndirectArgsBuffer", m_argsBuffer);
        m_compute.SetInt("_SourceTriangleCount", sourceTriCount);
        m_mat.SetBuffer("_DrawTrianges", m_drawBuffer);

        //  Calculate required threads.
        m_compute.GetKernelThreadGroupSizes(m_kernelId, out uint threadGroupSize, out _, out _);
        m_dispatchSize = Mathf.CeilToInt((float)sourceTriCount/threadGroupSize);

        m_localBounds = m_mesh.bounds;
        m_localBounds.Expand(1);
    }
    private void OnDisable()
    {
        if(m_computeInitialised)
        {
            m_sourceVertBuffer.Release();
            m_sourceTriBuffer.Release();
            m_drawBuffer.Release();
            m_argsBuffer.Release();
        }
        m_computeInitialised = false;
    }
    private void LateUpdate()
    {
        if(Application.isPlaying == false)
        {
            OnDisable();
            OnEnable();
        }
        m_drawBuffer.SetCounterValue(0);
        m_argsBuffer.SetData(m_argsBufferReset);
        Bounds bounds = TransformBounds(m_localBounds);
    }
}
