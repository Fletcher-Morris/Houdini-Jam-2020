using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

[RequireComponent(typeof(ParticleSystem))]
[ExecuteInEditMode]
public class BakeParticleSystemToMesh : MonoBehaviour
{

    public string folderPath = "Meshes";
    public string fileName = "NewBakedParticleSystemMesh";
    public bool keepVertexColors = true;
    public int maxQuads = 200;

    public uint seed;
    public void NewSeed() => seed = (uint)Random.Range(int.MinValue, int.MaxValue);

    public enum NormalType
    {
        KeepNormals,
        NormalizedVertexPosition,
        ClearNormals
    }

    public NormalType handleNormals;

#if UNITY_EDITOR
    [ContextMenu("Bake LOD 0")]
    public void BakeLod0() => StartCoroutine(BakeLod(0));
    [ContextMenu("Bake LOD 1")]
    public void BakeLod1() => StartCoroutine(BakeLod(1));
    [ContextMenu("Bake LOD 2")]
    public void BakeLod2() => StartCoroutine(BakeLod(2));
    [ContextMenu("Bake LOD 3")]
    public void BakeLod3() => StartCoroutine(BakeLod(3));
    private IEnumerator BakeLod(int lod)
    {
        ParticleSystem pS = GetComponent<ParticleSystem>();
        pS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        yield return new WaitForSecondsRealtime(0.25f);
        pS.randomSeed = seed;
        pS.emission.SetBurst(0, new ParticleSystem.Burst(0, maxQuads / (lod + 1)));
        pS.Play();
        yield return new WaitForSecondsRealtime(0.25f);


        // Bake
        Mesh mesh = new Mesh();
        GetComponent<ParticleSystemRenderer>().BakeMesh(mesh, true);
        if (!keepVertexColors)
            mesh.colors32 = null;
        switch (handleNormals)
        {
            case NormalType.KeepNormals:
                break;
            case NormalType.NormalizedVertexPosition:
                Vector3[] normals = mesh.vertices;
                int length = normals.Length;
                for (int i = 0; i < length; i++)
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

        string fileName = Path.GetFileNameWithoutExtension(this.fileName + "_" + lod) + ".asset";
        Directory.CreateDirectory("Assets/" + folderPath);
        string assetPath = "Assets/" + folderPath + "/" + fileName;

        Object existingAsset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
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