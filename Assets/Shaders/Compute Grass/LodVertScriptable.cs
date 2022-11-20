using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Lod Verts", menuName = "Scriptables/Grass/Lod Verts")]
public class LodVertScriptable : SerializedScriptableObject
{
    public void Init()
    {
        _lodMeshes = new List<LodMesh>();
        _lodMeshes.Clear();
        _lodVertices = new List<LodVertex>();
        _lodVertices.Clear();
        _extractingMeshId = 0;
        _extractVerts = false;
        _vertsExtracted = false;
    }

    private List<LodMesh> _lodMeshes = new List<LodMesh>();
    public void AddLodMesh(Mesh mesh, int lodId)
    {
        _lodMeshes.Add(new LodMesh(mesh, lodId));
    }
    public int MeshCount() => _lodMeshes.Count;

    [OdinSerialize, HideInInspector] private List<LodVertex> _lodVertices = new List<LodVertex>();
    public List<LodVertex> LodVertices { get => _lodVertices; }
    public int ExtractingMeshId { get => _extractingMeshId; }
    public bool ExtractVerts { get => _extractVerts; }
    public int NumVertsExtracted { get => _numVertsExtracted; set => _numVertsExtracted = value; }

    public void ClearVerts() => _lodVertices = new List<LodVertex>();

    public void ExtractVertices()
    {
        _extractVerts = true;
        _lodVertices = new List<LodVertex>();
        _vertsExtracted = false;
        _extractingMeshId = 0;
        _numVertsExtracted = 0;
    }


    public void CancelExtraction()
    {
        _extractVerts = false;
    }

    private bool _extractVerts;
    private bool _vertsExtracted;
    private int _extractingMeshId = 0;
    private int _numVertsExtracted = 0;

    public void Update()
    {
        if (!_extractVerts) return;
        if (_vertsExtracted) return;
        if (_extractingMeshId >= _lodMeshes.Count)
        {
            _vertsExtracted = true;
            _extractVerts = false;
        }
        else
        {
            foreach (Vector3 vert in _lodMeshes[_extractingMeshId].mesh.vertices)
            {
                _lodVertices.Add(new LodVertex(vert, _lodMeshes[_extractingMeshId].lodId));
                _numVertsExtracted++;
            }
            _extractingMeshId++;
        }
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(LodVertScriptable))]
public class LodVertScriptableEditor : Editor
{
    LodVertScriptable obj;

    private void OnEnable()
    {
        obj = (LodVertScriptable)target;
    }

    public override void OnInspectorGUI()
    {
        obj.Update();

        EditorGUILayout.LabelField("Meshes Processed : " + obj.ExtractingMeshId + " / " + obj.MeshCount());
        EditorGUILayout.LabelField("Vertices Extracted : " + obj.NumVertsExtracted);

        if (obj.ExtractVerts)
        {
            if (GUILayout.Button("Cancel Extraction"))
            {
                obj.CancelExtraction();
            }
        }
        else
        {
            if (GUILayout.Button("Extract Vertices"))
            {
                obj.ExtractVertices();
            }

            GUILayout.Space(50);

            if (GUILayout.Button("Reset"))
            {
                obj.Init();
            }
        }
    }
}

#endif

[Serializable]
public struct LodMesh
{
    public Mesh mesh;
    public int lodId;

    public LodMesh(Mesh mesh, int lodId)
    {
        this.mesh = mesh;
        this.lodId = lodId;
    }
}

[StructLayout(LayoutKind.Sequential), Serializable]
public struct LodVertex
{
    public Vector3 position;
    public int lodId;

    public LodVertex(Vector3 position, int lodId)
    {
        this.position = position;
        this.lodId = lodId;
    }
}