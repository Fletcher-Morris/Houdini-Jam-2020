using Sirenix.OdinInspector;
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

    private static readonly int _sourceVerticesPropertyId = Shader.PropertyToID("_SourceVertices");
    private static readonly int _sourceTrianglesPropertyId = Shader.PropertyToID("_SourceTriangles");
    private static readonly int _drawTrianglesPropertyId = Shader.PropertyToID("_DrawTriangles");
    private static readonly int _indirectArgsBufferPropertyId = Shader.PropertyToID("_IndirectArgsBuffer");
    private static readonly int _numSourceTrianglesPropertyId = Shader.PropertyToID("_NumSourceTriangles");
    private static readonly int _worldSpaceCameraPosPropertyId = Shader.PropertyToID("_WorldSpaceCameraPos");
    private static readonly int _worldSpaceCameraForwardPropertyId = Shader.PropertyToID("_WorldSpaceCameraForward");
    private static readonly int _localToWorldPropertyId = Shader.PropertyToID("_LocalToWorld");

    [SerializeField] private Mesh _sourceMesh;
    [SerializeField] private ComputeShader _compute;
    [SerializeField] private Material _material;
    [SerializeField] private Transform _cameraTransform;

    public ComputeGrassSettings _grassSettings;
    private readonly int[] _argsBufferReset = { 0, 1, 0, 0 };
    private ComputeBuffer _argsBuffer;
    private int _dispatchSize;
    private ComputeBuffer _drawBuffer;
    private Bounds _localBounds;
    private int _grassKernelId;
    private ComputeGrassSettingsData _prevGrassSettings;
    private ComputeBuffer _sourceTriBuffer;
    private ComputeBuffer _sourceVertBuffer;

    private bool _initialised = false;

    private void Awake()
    {
        Cleanup();
    }

    public void ToggleGrass()
    {
        if(_initialised)
        {
            Cleanup();
        }
        else
        {
            Initialise();
        }
    }

    private void LateUpdate()
    {
        if (_initialised == false) return;

        _drawBuffer.SetCounterValue(0);
        _argsBuffer.SetData(_argsBufferReset);

        if (_prevGrassSettings.Checksum != _grassSettings.SettingsData.Checksum) SubmitGrassSettings();
        if (_cameraTransform == null)
        {
            _compute.SetVector(_worldSpaceCameraPosPropertyId, Vector4.zero);
            _compute.SetVector(_worldSpaceCameraForwardPropertyId, Vector4.one);
        }
        else
        {
            _compute.SetVector(_worldSpaceCameraPosPropertyId, _cameraTransform.position);
            _compute.SetVector(_worldSpaceCameraForwardPropertyId, _cameraTransform.forward);
        }

        Bounds bounds = TransformBounds(_localBounds);

        _compute.SetMatrix(_localToWorldPropertyId, transform.localToWorldMatrix);

        _compute.Dispatch(_grassKernelId, _dispatchSize, 1, 1);
        Graphics.DrawProceduralIndirect(_material, bounds, MeshTopology.Triangles, _argsBuffer, 0,
            null, null, ShadowCastingMode.Off, true, gameObject.layer);
    }

    private void OnEnable()
    {
        if(_initialised) Cleanup();
        DebugHardware();
        Initialise();
    }

    [Button]
    private void Initialise()
    {
        Vector3[] positions = _sourceMesh.vertices;
        int[] tris = _sourceMesh.triangles;

        SourceVertex[] vertices = new SourceVertex[positions.Length];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = new SourceVertex
            {
                position = positions[i]
            };
        }

        int numSourceTriangles = tris.Length / 3;

        _sourceVertBuffer = new ComputeBuffer(vertices.Length, SOURCE_VERT_STRIDE, ComputeBufferType.Structured,
            ComputeBufferMode.Immutable);
        _sourceVertBuffer.SetData(vertices);
        _sourceTriBuffer = new ComputeBuffer(tris.Length, SOURCE_TRI_STRIDE, ComputeBufferType.Structured,
            ComputeBufferMode.Immutable);
        _sourceTriBuffer.SetData(tris);
        _drawBuffer =
            new ComputeBuffer(numSourceTriangles * Mathf.CeilToInt(_grassSettings.SettingsData.grassPerVertex * 0.25f),
                DRAW_STRIDE, ComputeBufferType.Append);
        _drawBuffer.SetCounterValue(0);
        _argsBuffer = new ComputeBuffer(1, INDIRECT_ARGS_STRIDE, ComputeBufferType.IndirectArguments);
        _grassKernelId = _compute.FindKernel("Main");

        _compute.SetBuffer(_grassKernelId, _sourceVerticesPropertyId, _sourceVertBuffer);
        _compute.SetBuffer(_grassKernelId, _sourceTrianglesPropertyId, _sourceTriBuffer);
        _compute.SetBuffer(_grassKernelId, _drawTrianglesPropertyId, _drawBuffer);
        _compute.SetBuffer(_grassKernelId, _indirectArgsBufferPropertyId, _argsBuffer);
        _compute.SetInt(_numSourceTrianglesPropertyId, numSourceTriangles);

        _material.SetBuffer(_drawTrianglesPropertyId, _drawBuffer);

        _compute.GetKernelThreadGroupSizes(_grassKernelId, out uint threadGroupSize, out _, out _);
        _dispatchSize = Mathf.CeilToInt((float)numSourceTriangles / threadGroupSize);

        _localBounds = _sourceMesh.bounds;
        _localBounds.Expand(1);

        _initialised = true;
    }

    private void OnDisable()
    {
        Cleanup();
    }

    [Button]
    private void Cleanup()
    {
        if(_sourceVertBuffer != null) _sourceVertBuffer.Dispose();
        if(_sourceTriBuffer != null) _sourceTriBuffer.Dispose();
        if(_drawBuffer != null) _drawBuffer.Dispose();
        if(_argsBuffer != null) _argsBuffer.Dispose();

        _initialised = false;
    }

    private void DebugHardware()
    {
        Debug.Log($"System Supports Compute Shaders : {SystemInfo.supportsComputeShaders}");
        Debug.Log($"System Supports Instancing : {SystemInfo.supportsInstancing}");
        Debug.Log($"Max Compute Buffer Workgroup Size : {SystemInfo.maxComputeWorkGroupSize}");
    }

    public Bounds TransformBounds(Bounds boundsOS)
    {
        Vector3 center = transform.TransformPoint(boundsOS.center);
        Vector3 extents = boundsOS.extents;
        Vector3 axisX = transform.TransformVector(extents.x, 0, 0);
        Vector3 axisY = transform.TransformVector(0, extents.y, 0);
        Vector3 axisZ = transform.TransformVector(0, 0, extents.z);
        extents.x = Mathf.Abs(axisX.x) + Mathf.Abs(axisY.x) + Mathf.Abs(axisZ.x);
        extents.y = Mathf.Abs(axisX.y) + Mathf.Abs(axisY.y) + Mathf.Abs(axisZ.y);
        extents.z = Mathf.Abs(axisX.z) + Mathf.Abs(axisY.z) + Mathf.Abs(axisZ.z);
        return new Bounds { center = center, extents = extents };
    }

    private void SubmitGrassSettings()
    {
        _prevGrassSettings = _grassSettings.SettingsData;

        _compute.SetFloat("_GrassHeight", _grassSettings.SettingsData.grassHeight);
        _compute.SetFloat("_GrassHeightRandom", _grassSettings.SettingsData.grassHeightRandom);
        _compute.SetFloat("_GrassHeightCuttoff", _grassSettings.SettingsData.grassHeightCuttoff);
        _compute.SetFloat("_GrassWidth", _grassSettings.SettingsData.grassWidth);
        _compute.SetFloat("_GrassWidthRandom", _grassSettings.SettingsData.grassWidthRandom);
        _compute.SetFloat("_BendRandom", _grassSettings.SettingsData.grassBendRandom);
        _compute.SetInt("_GrassSegments", _grassSettings.SettingsData.grassSegments);
        _compute.SetInt("_GrassPerVertex", Mathf.RoundToInt(_grassSettings.SettingsData.grassPerVertex));
        _compute.SetFloat("_RandomPosition", _grassSettings.SettingsData.randomPosition);
        _compute.SetFloat("_MinCamDist", _grassSettings.SettingsData.minCamDist);
        _compute.SetFloat("_MaxCameraDist", Mathf.Max(_grassSettings.SettingsData.maxCameraDist, 0.1f));
        _compute.SetFloat("_MinAltitude", _grassSettings.SettingsData.minAltitude);
        _compute.SetFloat("_MaxAltitude", _grassSettings.SettingsData.maxAltitude);
        _compute.SetFloat("_AltitudeHeightFade", _grassSettings.SettingsData.altitudeFade);
        _compute.SetFloat("_CameraDotCuttoff", _grassSettings.SettingsData.camDotCuttoff);
        _compute.SetFloat("_AvPlanetRadius", _grassSettings.SettingsData.averagePlanetRadius);
        Debug.Log("Submitting Compute Grass Settings!");
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SourceVertex
    {
        public Vector3 position;
    }
}