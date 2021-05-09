using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class GrassComputeController : MonoBehaviour
{
    [SerializeField] private Mesh sourceMesh = default;
    [SerializeField] private ComputeShader m_compute = default;
    [SerializeField] private Material material = default;
    [SerializeField] private Transform m_cameraTransform = default;

    [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct SourceVertex
    {
        public Vector3 position;
    }
    private bool initialized;
    private ComputeBuffer sourceVertBuffer;
    private ComputeBuffer sourceTriBuffer;
    private ComputeBuffer drawBuffer;
    private ComputeBuffer argsBuffer;
    private int idGrassKernel;
    private int dispatchSize;
    private Bounds localBounds;

    private const int SOURCE_VERT_STRIDE = sizeof(float) * 3;
    private const int SOURCE_TRI_STRIDE = sizeof(int);
    private const int DRAW_STRIDE = sizeof(float) * (3 + (3 + 1) * 3);
    private const int INDIRECT_ARGS_STRIDE = sizeof(int) * 4;
    private int[] argsBufferReset = new int[] { 0, 1, 0, 0 };

    private void OnEnable()
    {
        Debug.Assert(m_compute != null, "The grass compute shader is null", gameObject);
        Debug.Assert(material != null, "The material is null", gameObject);

        if (initialized)
        {
            OnDisable();
        }
        initialized = true;

        Vector3[] positions = sourceMesh.vertices;
        int[] tris = sourceMesh.triangles;

        SourceVertex[] vertices = new SourceVertex[positions.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new SourceVertex()
            {
                position = positions[i],
            };
        }
        int numSourceTriangles = tris.Length / 3;

        sourceVertBuffer = new ComputeBuffer(vertices.Length, SOURCE_VERT_STRIDE, ComputeBufferType.Structured, ComputeBufferMode.Immutable);
        sourceVertBuffer.SetData(vertices);
        sourceTriBuffer = new ComputeBuffer(tris.Length, SOURCE_TRI_STRIDE, ComputeBufferType.Structured, ComputeBufferMode.Immutable);
        sourceTriBuffer.SetData(tris);
        drawBuffer = new ComputeBuffer(numSourceTriangles, DRAW_STRIDE, ComputeBufferType.Append);
        drawBuffer.SetCounterValue(0);
        argsBuffer = new ComputeBuffer(1, INDIRECT_ARGS_STRIDE, ComputeBufferType.IndirectArguments);
        idGrassKernel = m_compute.FindKernel("Main");

        m_compute.SetBuffer(idGrassKernel, "_SourceVertices", sourceVertBuffer);
        m_compute.SetBuffer(idGrassKernel, "_SourceTriangles", sourceTriBuffer);
        m_compute.SetBuffer(idGrassKernel, "_DrawTriangles", drawBuffer);
        m_compute.SetBuffer(idGrassKernel, "_IndirectArgsBuffer", argsBuffer);
        m_compute.SetInt("_NumSourceTriangles", numSourceTriangles);

        material.SetBuffer("_DrawTriangles", drawBuffer);

        m_compute.GetKernelThreadGroupSizes(idGrassKernel, out uint threadGroupSize, out _, out _);
        dispatchSize = Mathf.CeilToInt((float)numSourceTriangles / threadGroupSize);

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
    public Bounds TransformBounds(Bounds boundsOS)
    {
        var center = transform.TransformPoint(boundsOS.center);
        var extents = boundsOS.extents;
        var axisX = transform.TransformVector(extents.x, 0, 0);
        var axisY = transform.TransformVector(0, extents.y, 0);
        var axisZ = transform.TransformVector(0, 0, extents.z);
        extents.x = Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x);
        extents.y = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
        extents.z = Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z);
        return new Bounds { center = center, extents = extents };
    }

    private void LateUpdate()
    {
        if (Application.isPlaying == false)
        {
            OnDisable();
            OnEnable();
        }

        drawBuffer.SetCounterValue(0);
        argsBuffer.SetData(argsBufferReset);

        if (m_prevGrassSettings.Checksum != grassSettings.SettingsData.Checksum) SubmitGrassSettings();
        if(m_cameraTransform != null) m_compute.SetVector("_WorldSpaceCameraPos", m_cameraTransform.position);
        else m_compute.SetVector("_WorldSpaceCameraPos", Vector4.zero);

        Bounds bounds = TransformBounds(localBounds);

        m_compute.SetMatrix("_LocalToWorld", transform.localToWorldMatrix);

        m_compute.Dispatch(idGrassKernel, dispatchSize, 1, 1);

        Graphics.DrawProceduralIndirect(material, bounds, MeshTopology.Triangles, argsBuffer, 0,
            null, null, ShadowCastingMode.Off, true, gameObject.layer);
    }
    public ComputeGrassSettings grassSettings = default;
    private ComputeGrassSettingsData m_prevGrassSettings = default;
    private void SubmitGrassSettings()
    {
        m_prevGrassSettings = grassSettings.SettingsData;

        m_compute.SetFloat("_GrassHeight", grassSettings.SettingsData.grassHeight);
        m_compute.SetFloat("_GrassHeightRanom", grassSettings.SettingsData.grassHeightRandom);
        m_compute.SetFloat("_GrassWidth", grassSettings.SettingsData.grassWidth);
        m_compute.SetFloat("_GrassWidthRandom", grassSettings.SettingsData.grassWidthRandom);
        m_compute.SetInt("_GrassSegments", grassSettings.SettingsData.grassSegments);
        m_compute.SetInt("_GrassPerVertex", grassSettings.SettingsData.grassPerVertex);
        m_compute.SetFloat("_MaxCameraDist", grassSettings.SettingsData.maxCameraDist);
    }
}
