using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using System.IO;
using UnityEditor;

#endif

[RequireComponent(typeof(ParticleSystem)), ExecuteInEditMode]
public class BakeParticleSystemToMesh : MonoBehaviour
{
    public enum NormalType
    {
        KeepNormals,
        NormalizedVertexPosition,
        ClearNormals
    }

    public string folderPath = "Meshes";
    public string fileName = "NewBakedParticleSystemMesh";
    public bool keepVertexColors = true;
    public int maxQuads = 200;
    public uint seed;

    public NormalType handleNormals;

    public void NewSeed()
    {
        seed = (uint)Random.Range(int.MinValue, int.MaxValue);
    }

#if UNITY_EDITOR
    [ContextMenu("Bake LOD 0")]
    public void BakeLod0()
    {
        StartCoroutine(BakeLod(0));
    }

    [ContextMenu("Bake LOD 1")]
    public void BakeLod1()
    {
        StartCoroutine(BakeLod(1));
    }

    [ContextMenu("Bake LOD 2")]
    public void BakeLod2()
    {
        StartCoroutine(BakeLod(2));
    }

    [ContextMenu("Bake LOD 3")]
    public void BakeLod3()
    {
        StartCoroutine(BakeLod(3));
    }

    private IEnumerator BakeLod(int lod)
    {
        var pS = GetComponent<ParticleSystem>();
        pS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        yield return new WaitForSecondsRealtime(0.25f);
        pS.randomSeed = seed;
        pS.emission.SetBurst(0, new ParticleSystem.Burst(0, maxQuads / (lod + 1)));
        pS.Play();
        yield return new WaitForSecondsRealtime(0.25f);
        var mesh = new Mesh();
        GetComponent<ParticleSystemRenderer>().BakeMesh(mesh, true);
        if (!keepVertexColors) mesh.colors32 = null;
        switch (handleNormals)
        {
            case NormalType.KeepNormals:
                break;
            case NormalType.NormalizedVertexPosition:
                var normals = mesh.vertices;
                var length = normals.Length;
                for (var i = 0; i < length; i++)
                {
                    normals[i] = normals[i].normalized;
                }

                mesh.normals = normals;
                break;
            default:
            case NormalType.ClearNormals:
                mesh.normals = null;
                break;
        }

        var fileName = Path.GetFileNameWithoutExtension(this.fileName + "_" + lod) + ".asset";
        Directory.CreateDirectory("Assets/" + folderPath);
        var assetPath = "Assets/" + folderPath + "/" + fileName;
        var existingAsset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
        if (existingAsset == null)
        {
            AssetDatabase.CreateAsset(mesh, assetPath);
        }
        else
        {
            if (existingAsset is Mesh)
                (existingAsset as Mesh).Clear();
            EditorUtility.CopySerialized(mesh, existingAsset);
        }

        AssetDatabase.SaveAssets();
        yield return null;
    }
#endif
}