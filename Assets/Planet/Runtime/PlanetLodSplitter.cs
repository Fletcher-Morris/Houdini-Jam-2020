using MeshSplit;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetLodSplitter : MonoBehaviour
{
    [SerializeField, Required] private MeshRenderer _copyRenderer;
    [SerializeField] private Mesh[] _meshes;
    [SerializeField] private MeshSplitParameters _parameters;

    [SerializeField, HideInInspector] private LODGroup[] _lodGroups;

    [Button]
    private void CreateLods()
    {
        ResetLods();

        _copyRenderer.enabled = false;

        MeshSplitter splitter = new MeshSplitter(_parameters);

        List<(Vector3Int gridPoint, Mesh mesh, Vector3 offset)>[] splitMeshes = new List<(Vector3Int gridPoint, Mesh mesh, Vector3 offset)>[_meshes.Length];
        Dictionary<Vector3Int, Tuple<Mesh, Vector3>>[] splitMeshDicArray = new Dictionary<Vector3Int, Tuple<Mesh, Vector3>>[_meshes.Length];

        int minSubmeshCount = int.MaxValue;
        int maxSubmeshCount = int.MinValue;

        for (int lodId = 0; lodId < _meshes.Length; lodId++)
        {
            Mesh lodMesh = _meshes[lodId];
            splitMeshes[lodId] = splitter.SplitAndOffset(lodMesh);
            splitMeshDicArray[lodId] = new Dictionary<Vector3Int, Tuple<Mesh, Vector3>>();

            minSubmeshCount = Mathf.Min(splitMeshes[lodId].Count, minSubmeshCount);
            maxSubmeshCount = Mathf.Max(splitMeshes[lodId].Count, maxSubmeshCount);

            for (int splitMeshId = 0; splitMeshId < splitMeshes[lodId].Count; splitMeshId++)
            {
                Vector3Int gridPoint = splitMeshes[lodId][splitMeshId].gridPoint;
                Mesh mesh = splitMeshes[lodId][splitMeshId].mesh;
                Vector3 offset = splitMeshes[lodId][splitMeshId].offset;
                splitMeshDicArray[lodId].Add(gridPoint, new Tuple<Mesh, Vector3>(mesh, offset));
            }
        }

        _lodGroups = new LODGroup[maxSubmeshCount];

        if (splitMeshDicArray.Length > 0)
        {
            int splitMeshId = 0;
            Vector3 lodOffset = new Vector3();
            foreach (KeyValuePair<Vector3Int, Tuple<Mesh, Vector3>> lod0Pair in splitMeshDicArray[0])
            {
                lodOffset = lod0Pair.Value.Item2;
                LODGroup lodGroup = new GameObject("Submesh " + lod0Pair.Key).AddComponent<LODGroup>();
                lodGroup.transform.parent = transform;
                lodGroup.transform.localPosition = lodOffset;
                lodGroup.transform.localRotation = Quaternion.identity;
                lodGroup.transform.localScale = Vector3.one;

                LOD[] lods = lodGroup.GetLODs();
                for (int mesh = 0; mesh < splitMeshDicArray.Length; mesh++)
                {
                    lods[mesh].renderers = new Renderer[1];
                }

                for (int lodId = 0; lodId < splitMeshDicArray.Length; lodId++)
                {
                    if (splitMeshDicArray[lodId].ContainsKey(lod0Pair.Key))
                    {
                        Tuple<Mesh, Vector3> splitMeshInLod = splitMeshDicArray[lodId][lod0Pair.Key];

                        Vector3 localOffset = splitMeshInLod.Item2 - lodOffset;

                        MeshRenderer renderer = new GameObject("LOD " + lodId).AddComponent<MeshRenderer>();
                        MeshFilter filter = renderer.gameObject.AddComponent<MeshFilter>();
                        renderer.transform.parent = lodGroup.transform;
                        renderer.transform.localPosition = localOffset;
                        renderer.transform.localRotation = Quaternion.identity;
                        renderer.transform.localScale = Vector3.one;

                        renderer.sharedMaterial = _copyRenderer.sharedMaterial;
                        renderer.shadowCastingMode = _copyRenderer.shadowCastingMode;
                        renderer.receiveShadows = _copyRenderer.receiveShadows;

                        filter.mesh = splitMeshInLod.Item1;
                        lods[lodId].renderers[0] = renderer;
                    }
                }

                lodGroup.SetLODs(lods);
                _lodGroups[splitMeshId] = lodGroup;
                splitMeshId++;
            }
        }
    }

    [Button]
    private void ResetLods()
    {
        if (_lodGroups != null)
        {
            for (int i = 0; i < _lodGroups.Length; i++)
            {
                if (_lodGroups[i] != null)
                {
                    DestroyImmediate(_lodGroups[i].gameObject);
                }
            }
        }

        _lodGroups = null;

        _copyRenderer.enabled = true;
    }
}
